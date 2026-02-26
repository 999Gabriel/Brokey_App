using Microsoft.EntityFrameworkCore;
using Models;

namespace ORM;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripMember> TripMembers => Set<TripMember>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<ExpenseSplit> ExpenseSplits => Set<ExpenseSplit>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<CountryCurrency> CountryCurrencies => Set<CountryCurrency>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.BaseCurrency).IsRequired().HasMaxLength(3);
            entity.Property(u => u.ProfileImagePath).HasMaxLength(500);
        });

        // ── Group ──
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
            entity.Property(g => g.Description).HasMaxLength(500);

            entity.HasOne(g => g.Admin)
                .WithMany(u => u.AdminOfGroups)
                .HasForeignKey(g => g.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── GroupMember ──
        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(gm => gm.Id);
            entity.HasIndex(gm => new { gm.GroupId, gm.UserId }).IsUnique();

            entity.HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Trip ──
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(150);
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.Property(t => t.Country).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Currency).IsRequired().HasMaxLength(3);

            entity.HasOne(t => t.Group)
                .WithMany(g => g.Trips)
                .HasForeignKey(t => t.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.CreatedBy)
                .WithMany(u => u.CreatedTrips)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── TripMember ──
        modelBuilder.Entity<TripMember>(entity =>
        {
            entity.HasKey(tm => tm.Id);
            entity.HasIndex(tm => new { tm.TripId, tm.UserId }).IsUnique();

            entity.HasOne(tm => tm.Trip)
                .WithMany(t => t.Members)
                .HasForeignKey(tm => tm.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tm => tm.User)
                .WithMany(u => u.TripMemberships)
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── ExpenseCategory ──
        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.HasKey(ec => ec.Id);
            entity.Property(ec => ec.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(ec => ec.Name).IsUnique();
            entity.Property(ec => ec.Icon).HasMaxLength(50);
        });

        // ── Expense ──
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Latitude).HasPrecision(10, 7);
            entity.Property(e => e.Longitude).HasPrecision(10, 7);

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.Expenses)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PaidBy)
                .WithMany(u => u.PaidExpenses)
                .HasForeignKey(e => e.PaidByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ExpenseSplit ──
        modelBuilder.Entity<ExpenseSplit>(entity =>
        {
            entity.HasKey(es => es.Id);
            entity.Property(es => es.Amount).HasPrecision(18, 2);

            entity.HasOne(es => es.Expense)
                .WithMany(e => e.Splits)
                .HasForeignKey(es => es.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(es => es.User)
                .WithMany(u => u.ExpenseSplits)
                .HasForeignKey(es => es.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Receipt ──
        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.ImagePath).IsRequired().HasMaxLength(500);

            entity.HasOne(r => r.Expense)
                .WithMany(e => e.Receipts)
                .HasForeignKey(r => r.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── ExchangeRate ──
        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasKey(er => er.Id);
            entity.Property(er => er.FromCurrency).IsRequired().HasMaxLength(3);
            entity.Property(er => er.ToCurrency).IsRequired().HasMaxLength(3);
            entity.Property(er => er.Rate).HasPrecision(18, 6);
            entity.HasIndex(er => new { er.FromCurrency, er.ToCurrency });
        });

        // ── CountryCurrency ──
        modelBuilder.Entity<CountryCurrency>(entity =>
        {
            entity.HasKey(cc => cc.Id);
            entity.Property(cc => cc.Country).IsRequired().HasMaxLength(100);
            entity.HasIndex(cc => cc.Country).IsUnique();
            entity.Property(cc => cc.CurrencyCode).IsRequired().HasMaxLength(3);
        });

        // ── Seed default expense categories ──
        modelBuilder.Entity<ExpenseCategory>().HasData(
            new ExpenseCategory { Id = 1, Name = "Food", Icon = "🍔" },
            new ExpenseCategory { Id = 2, Name = "Night Out", Icon = "🍻" },
            new ExpenseCategory { Id = 3, Name = "Transport", Icon = "🚕" },
            new ExpenseCategory { Id = 4, Name = "Accommodation", Icon = "🏨" },
            new ExpenseCategory { Id = 5, Name = "Activities", Icon = "🎢" },
            new ExpenseCategory { Id = 6, Name = "Shopping", Icon = "🛍️" },
            new ExpenseCategory { Id = 7, Name = "Flights", Icon = "✈️" },
            new ExpenseCategory { Id = 8, Name = "Other", Icon = "📦" }
        );

        // ── Seed common country currencies ──
        modelBuilder.Entity<CountryCurrency>().HasData(
            new CountryCurrency { Id = 1, Country = "Germany", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 2, Country = "France", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 3, Country = "Spain", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 4, Country = "Italy", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 5, Country = "Portugal", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 6, Country = "Netherlands", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 7, Country = "Austria", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 8, Country = "Greece", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 9, Country = "United States", CurrencyCode = "USD" },
            new CountryCurrency { Id = 10, Country = "United Kingdom", CurrencyCode = "GBP" },
            new CountryCurrency { Id = 11, Country = "Switzerland", CurrencyCode = "CHF" },
            new CountryCurrency { Id = 12, Country = "Japan", CurrencyCode = "JPY" },
            new CountryCurrency { Id = 13, Country = "Australia", CurrencyCode = "AUD" },
            new CountryCurrency { Id = 14, Country = "Canada", CurrencyCode = "CAD" },
            new CountryCurrency { Id = 15, Country = "Turkey", CurrencyCode = "TRY" },
            new CountryCurrency { Id = 16, Country = "Thailand", CurrencyCode = "THB" },
            new CountryCurrency { Id = 17, Country = "Mexico", CurrencyCode = "MXN" },
            new CountryCurrency { Id = 18, Country = "Brazil", CurrencyCode = "BRL" },
            new CountryCurrency { Id = 19, Country = "Croatia", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 20, Country = "Czech Republic", CurrencyCode = "CZK" },
            new CountryCurrency { Id = 21, Country = "Hungary", CurrencyCode = "HUF" },
            new CountryCurrency { Id = 22, Country = "Poland", CurrencyCode = "PLN" },
            new CountryCurrency { Id = 23, Country = "Sweden", CurrencyCode = "SEK" },
            new CountryCurrency { Id = 24, Country = "Norway", CurrencyCode = "NOK" },
            new CountryCurrency { Id = 25, Country = "Denmark", CurrencyCode = "DKK" }
        );
    }
}

