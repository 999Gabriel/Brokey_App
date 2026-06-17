using API_Server.DTOs;
using API_Server.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using ORM;
using ORM.Repositories;

namespace API_Server.Controllers;

// Controller für Gruppen innerhalb eines Trips. Route-Präfix: api/groups (fest, NICHT [controller]).
// Zuständig für: Gruppenmitglieder verwalten, Ausgaben (Expenses) anlegen/ändern/löschen,
// Kategorien liefern und die Abrechnung (Settlement: Balances + Transfers) berechnen.
// [Authorize] auf Klassenebene → JEDE Action verlangt einen gültigen JWT.
[ApiController]      // automatische Validierung der DTO-Attribute + automatische 400-Antworten bei ungültigem Body
[Authorize]          // gilt für alle Actions: ohne gültigen Bearer-Token → 401
[Route("api/groups")]
public class GroupsController : ControllerBase
{
    // Erlaubte Aufteilungs-Modi für eine Ausgabe (als Konstanten, um Tippfehler zu vermeiden):
    private const string SplitModeEqual = "Equal";           // gleichmäßig auf alle Teilnehmer aufteilen
    private const string SplitModePercentage = "Percentage";  // nach Prozentsätzen (müssen 100% ergeben)
    private const string SplitModeAmount = "Amount";          // nach festen Beträgen (Summe = Gesamtbetrag)
    private const decimal SplitTolerance = 0.01m;             // erlaubte Rundungstoleranz (1 Cent) bei der Summenprüfung

    private readonly AppDbContext _context;                       // direkter DB-Zugang (z.B. für User-/Kategorie-Lookups)
    private readonly ExpenseRepository _expenseRepository;        // Ausgaben + Splits laden/schreiben
    private readonly GroupRepository _groupRepository;            // Gruppen laden/erstellen
    private readonly GroupMemberRepository _groupMemberRepository;  // Gruppenmitglieder hinzufügen/entfernen
    private readonly TripRepository _tripRepository;              // Trip laden + Mitgliedschaft des Users prüfen
    private readonly TripMemberRepository _tripMemberRepository;  // User automatisch als Trip-Teilnehmer eintragen

    // Alle Repositories und AppDbContext werden per DI injiziert.
    public GroupsController(
        AppDbContext context,
        ExpenseRepository expenseRepository,
        GroupRepository groupRepository,
        GroupMemberRepository groupMemberRepository,
        TripRepository tripRepository,
        TripMemberRepository tripMemberRepository)
    {
        _context = context;
        _expenseRepository = expenseRepository;
        _groupRepository = groupRepository;
        _groupMemberRepository = groupMemberRepository;
        _tripRepository = tripRepository;
        _tripMemberRepository = tripMemberRepository;
    }

    // POST /api/groups/{id}/members – [Authorize]. Request-DTO: AddGroupMemberRequest. Response: GroupMemberResponse.
    // Fügt einen User (per Username oder E-Mail) zur Gruppe hinzu und automatisch als TripMember, falls noch nicht vorhanden.
    // Statuscodes: 200 OK (hinzugefügt), 401 (kein Token), 404 (Gruppe/Trip/User nicht gefunden), 400 (ungültige Rolle).
    [HttpPost("{id:int}/members")]
    public async Task<ActionResult<GroupMemberResponse>> AddMember(
        int id,
        [FromBody] AddGroupMemberRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();  // ID des aufrufenden Users aus dem JWT
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        // Gruppe laden; existiert sie nicht → 404.
        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group == null)
        {
            return NotFound(new { message = "Group not found." });
        }

        // Berechtigungsprüfung: GetByIdForUserAsync liefert den Trip nur, wenn der aufrufende User Trip-Mitglied ist.
        var trip = await _tripRepository.GetByIdForUserAsync(group.TripId, currentUserId.Value, cancellationToken);
        if (trip == null)
        {
            return NotFound(new { message = "Trip not found." });
        }

        // Ziel-User per E-Mail ODER Username suchen (case-insensitiv).
        var normalizedIdentifier = request.UsernameOrEmail.Trim().ToLowerInvariant();
        var targetUser = await _context.Users.FirstOrDefaultAsync(
            u => u.Email.ToLower() == normalizedIdentifier || u.Username.ToLower() == normalizedIdentifier,
            cancellationToken);

        if (targetUser == null)
        {
            return NotFound(new { message = "User not found." });
        }

        // Rolle auf "Admin"/"Member" normalisieren; ungültige Eingabe → 400.
        var normalizedRole = NormalizeGroupRole(request.Role);
        if (normalizedRole == null)
        {
            return BadRequest(new { message = "Role must be Admin or Member." });
        }

        // Ziel-User muss Trip-Mitglied sein, bevor er Gruppenmitglied werden kann → automatisch als "Member" eintragen (idempotent).
        await _tripMemberRepository.AddParticipantAsync(group.TripId, targetUser.Id, "Member", cancellationToken);
        // Eigentliches Hinzufügen zur Gruppe; Repository liefert das (neue oder bestehende) GroupMember-Objekt zurück.
        var member = await _groupMemberRepository.AddMemberAsync(id, targetUser.Id, normalizedRole, cancellationToken);
        return Ok(MapMember(member!));  // 200 OK + DTO des Gruppenmitglieds
    }

    // DELETE /api/groups/{id}/members/{userId} – entfernt einen User aus der Gruppe.
    // Prüft: Gruppe+Trip existieren und aufrufender User ist Trip-Mitglied.
    [HttpDelete("{id:int}/members/{userId:int}")]
    public async Task<IActionResult> RemoveMember(int id, int userId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group == null)
        {
            return NotFound(new { message = "Group not found." });
        }

        var trip = await _tripRepository.GetByIdForUserAsync(group.TripId, currentUserId.Value, cancellationToken);
        if (trip == null)
        {
            return NotFound(new { message = "Trip not found." });
        }

        var removed = await _groupMemberRepository.RemoveMemberAsync(id, userId, cancellationToken);
        if (!removed)
        {
            return NotFound(new { message = "Member not found in this group." });
        }

        return NoContent();
    }

    // GET /api/groups/{id}/members – gibt alle Mitglieder einer Gruppe zurück (Admin zuerst, dann alphabetisch).
    // → AddExpenseViewModel und GroupDetailViewModel laden diese Liste.
    [HttpGet("{id:int}/members")]
    public async Task<ActionResult<List<GroupMemberResponse>>> GetMembers(int id, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group == null)
        {
            return NotFound(new { message = "Group not found." });
        }

        var trip = await _tripRepository.GetByIdForUserAsync(group.TripId, currentUserId.Value, cancellationToken);
        if (trip == null)
        {
            return NotFound(new { message = "Trip not found." });
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(id, cancellationToken);
        return Ok(members.Select(MapMember).ToList());
    }

    // GET /api/groups/{id}/expense-categories – liefert alle verfügbaren Kategorien (z.B. Food, Transport).
    // Kategorien sind in der DB vorgeseeded; Zugriff nur für Trip-Mitglieder.
    // → AddExpenseViewModel füllt damit das Kategorie-Dropdown.
    [HttpGet("{id:int}/expense-categories")]
    public async Task<ActionResult<List<ExpenseCategoryResponse>>> GetExpenseCategories(
        int id,
        CancellationToken cancellationToken)
    {
        var access = await GetAccessibleGroupAsync(id, cancellationToken);
        if (access.error != null)
        {
            return access.error;
        }

        var categories = await _expenseRepository.GetCategoriesAsync(cancellationToken);
        return Ok(categories.Select(c => new ExpenseCategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Icon = c.Icon
        }).ToList());
    }

    // GET /api/groups/{id}/expenses – gibt alle Ausgaben der Gruppe zurück, neueste zuerst.
    // Jede Expense enthält alle Splits mit Beträgen und Settled-Status.
    // → GroupDetailViewModel.Expenses, TripService.GetGroupExpensesAsync.
    [HttpGet("{id:int}/expenses")]
    public async Task<ActionResult<List<ExpenseResponse>>> GetExpenses(
        int id,
        CancellationToken cancellationToken)
    {
        var access = await GetAccessibleGroupAsync(id, cancellationToken);
        if (access.error != null)
        {
            return access.error;
        }

        var expenses = await _expenseRepository.GetGroupExpensesAsync(id, cancellationToken);
        return Ok(expenses.Select(MapExpense).ToList());
    }

    // GET /api/groups/{id}/expenses/{expenseId} – einzelne Ausgabe mit allen Splits laden.
    // → ExpenseDetailViewModel und AddExpenseViewModel (Edit-Modus).
    [HttpGet("{id:int}/expenses/{expenseId:int}")]
    public async Task<ActionResult<ExpenseResponse>> GetExpenseById(
        int id,
        int expenseId,
        CancellationToken cancellationToken)
    {
        var access = await GetAccessibleGroupAsync(id, cancellationToken);
        if (access.error != null)
        {
            return access.error;
        }

        var expense = await _expenseRepository.GetGroupExpenseByIdAsync(id, expenseId, cancellationToken);
        if (expense == null)
        {
            return NotFound(new { message = "Expense not found." });
        }

        return Ok(MapExpense(expense));
    }

    // POST /api/groups/{id}/expenses – erstellt eine neue Ausgabe mit Splits.
    // Nutzt BuildExpenseMutationContextAsync für Validierung und Split-Berechnung,
    // dann ExpenseRepository.CreateGroupExpenseAsync für die DB-Persistenz.
    [HttpPost("{id:int}/expenses")]
    public async Task<ActionResult<ExpenseResponse>> CreateExpense(
        int id,
        [FromBody] CreateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var mutation = await BuildExpenseMutationContextAsync(id, request, cancellationToken);
        if (mutation.error != null)
        {
            return mutation.error;
        }

        var context = mutation.context!;
        var created = await _expenseRepository.CreateGroupExpenseAsync(
            tripId: context.Group.TripId,
            groupId: context.Group.Id,
            paidByUserId: context.PaidByUserId,
            categoryId: context.CategoryId,
            title: context.Title,
            description: context.Description,
            amount: context.Amount,
            expenseDate: context.ExpenseDate,
            latitude: context.Latitude,
            longitude: context.Longitude,
            splitUserIds: context.SplitUserIds,
            splitAmountsByUser: context.SplitAmountsByUser,
            cancellationToken: cancellationToken);

        if (created == null)
        {
            return BadRequest(new { message = "Expense must have at least one participant." });
        }

        return CreatedAtAction(nameof(GetExpenseById), new { id, expenseId = created.Id }, MapExpense(created));
    }

    // PUT /api/groups/{id}/expenses/{expenseId} – aktualisiert eine bestehende Ausgabe.
    // Löscht alle alten Splits und erstellt neue; dieselbe Mutations-Validierung wie beim Erstellen.
    [HttpPut("{id:int}/expenses/{expenseId:int}")]
    public async Task<ActionResult<ExpenseResponse>> UpdateExpense(
        int id,
        int expenseId,
        [FromBody] CreateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var mutation = await BuildExpenseMutationContextAsync(id, request, cancellationToken);
        if (mutation.error != null)
        {
            return mutation.error;
        }

        var context = mutation.context!;
        var updated = await _expenseRepository.UpdateGroupExpenseAsync(
            groupId: id,
            expenseId: expenseId,
            paidByUserId: context.PaidByUserId,
            categoryId: context.CategoryId,
            title: context.Title,
            description: context.Description,
            amount: context.Amount,
            expenseDate: context.ExpenseDate,
            latitude: context.Latitude,
            longitude: context.Longitude,
            splitUserIds: context.SplitUserIds,
            splitAmountsByUser: context.SplitAmountsByUser,
            cancellationToken: cancellationToken);

        if (updated == null)
        {
            return NotFound(new { message = "Expense not found." });
        }

        return Ok(MapExpense(updated));
    }

    // DELETE /api/groups/{id}/expenses/{expenseId} – löscht eine Ausgabe und ihre Splits (Cascade).
    [HttpDelete("{id:int}/expenses/{expenseId:int}")]
    public async Task<IActionResult> DeleteExpense(
        int id,
        int expenseId,
        CancellationToken cancellationToken)
    {
        var access = await GetAccessibleGroupAsync(id, cancellationToken);
        if (access.error != null)
        {
            return access.error;
        }

        var removed = await _expenseRepository.DeleteGroupExpenseAsync(id, expenseId, cancellationToken);
        if (!removed)
        {
            return NotFound(new { message = "Expense not found." });
        }

        return NoContent();
    }

    // GET /api/groups/{id}/settlement – berechnet Abrechnung für die Gruppe:
    // Balances (wer hat wie viel bezahlt/geschuldet) + Transfers (wer muss wem was zahlen).
    // Daten: alle Expenses der Gruppe → BuildBalances + BuildTransfers → GroupSettlementResponse.
    // → GroupDetailViewModel zeigt Settlement-Tab an.
    [HttpGet("{id:int}/settlement")]
    public async Task<ActionResult<GroupSettlementResponse>> GetSettlement(
        int id,
        CancellationToken cancellationToken)
    {
        var access = await GetAccessibleGroupAsync(id, cancellationToken);
        if (access.error != null)
        {
            return access.error;
        }

        var group = access.group!;
        var expenses = await _expenseRepository.GetGroupExpensesAsync(id, cancellationToken);
        var balances = BuildBalances(group, expenses);
        var transfers = BuildTransfers(expenses);

        return Ok(new GroupSettlementResponse
        {
            GroupId = group.Id,
            TripId = group.TripId,
            Currency = group.Trip.BaseCurrency,
            Balances = balances
                .OrderByDescending(b => b.NetBalance)
                .ThenBy(b => b.Username)
                .ToList(),
            Transfers = transfers
        });
    }

    // POST /api/groups/{id}/settlement/mark-settled – markiert alle offenen Splits zwischen zwei Usern als bezahlt.
    // Nur Admin oder einer der beiden beteiligten User darf diese Aktion ausführen.
    [HttpPost("{id:int}/settlement/mark-settled")]
    public async Task<IActionResult> MarkSettlementAsSettled(
        int id,
        [FromBody] MarkSettlementRequest request,
        CancellationToken cancellationToken)
    {
        if (request.FromUserId == request.ToUserId)
        {
            return BadRequest(new { message = "From and to users must be different." });
        }

        var access = await GetAccessibleGroupAsync(id, cancellationToken);
        if (access.error != null)
        {
            return access.error;
        }

        var group = access.group!;
        var currentUserId = access.currentUserId!.Value;
        var memberIds = group.Members.Select(member => member.UserId).ToHashSet();
        if (!memberIds.Contains(request.FromUserId) || !memberIds.Contains(request.ToUserId))
        {
            return BadRequest(new { message = "Both users must be members of this group." });
        }

        var isCurrentUserAdmin = group.Members.Any(member =>
            member.UserId == currentUserId &&
            string.Equals(member.Role, "Admin", StringComparison.OrdinalIgnoreCase));

        if (!isCurrentUserAdmin && currentUserId != request.FromUserId && currentUserId != request.ToUserId)
        {
            return Forbid();
        }

        var markedCount = await _expenseRepository.MarkSplitSettledAsync(
            groupId: id,
            fromUserId: request.FromUserId,
            toUserId: request.ToUserId,
            cancellationToken: cancellationToken);

        if (markedCount == 0)
        {
            return NotFound(new { message = "No unsettled transfer found for this payer/payee pair." });
        }

        return NoContent();
    }

    // Zentrale Validierungs- und Vorbereitungs-Methode für Create und Update einer Ausgabe.
    // Prüft: Betrag > 0, Titel vorhanden, Gruppe zugänglich, Kategorie gültig, Zahler ist Mitglied,
    // Split-Modus gültig. Bei Equal: splittet gleichmäßig. Bei Percentage/Amount: nur Admin darf custom split.
    // Gibt einen fertig befüllten ExpenseMutationContext zurück oder einen Fehler-ActionResult.
    private async Task<(ExpenseMutationContext? context, ActionResult? error)> BuildExpenseMutationContextAsync(
        int groupId,
        CreateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return (null, BadRequest(new { message = "Amount must be greater than 0." }));
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return (null, BadRequest(new { message = "Title is required." }));
        }

        var access = await GetAccessibleGroupAsync(groupId, cancellationToken);
        if (access.error != null)
        {
            return (null, access.error);
        }

        var group = access.group!;
        var currentUserId = access.currentUserId!.Value;

        var categoryExists = await _context.ExpenseCategories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            return (null, BadRequest(new { message = "Invalid expense category." }));
        }

        var membersByUserId = group.Members.ToDictionary(member => member.UserId, member => member);
        if (!membersByUserId.ContainsKey(currentUserId))
        {
            return (null, Forbid());
        }

        var paidByUserId = request.PaidByUserId ?? GetDefaultAdminUserId(group) ?? currentUserId;
        if (!membersByUserId.ContainsKey(paidByUserId))
        {
            return (null, BadRequest(new { message = "Paid-by user must be a member of this group." }));
        }

        var splitMode = NormalizeSplitMode(request.SplitMode);
        if (splitMode == null)
        {
            return (null, BadRequest(new { message = "Split mode must be Equal, Percentage, or Amount." }));
        }

        var currentMember = membersByUserId[currentUserId];
        IReadOnlyCollection<int> splitUserIds;
        Dictionary<int, decimal>? splitAmountsByUser = null;

        if (splitMode == SplitModeEqual)
        {
            var requestedSplitUserIds = request.SplitUserIds?
                .Where(idValue => idValue > 0)
                .Distinct()
                .ToList();

            splitUserIds = (requestedSplitUserIds == null || requestedSplitUserIds.Count == 0)
                ? membersByUserId.Keys.ToList()
                : requestedSplitUserIds;

            if (splitUserIds.Any(idValue => !membersByUserId.ContainsKey(idValue)))
            {
                return (null, BadRequest(new { message = "Split users must be members of this group." }));
            }
        }
        else
        {
            var isCurrentUserAdmin = string.Equals(currentMember.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isCurrentUserAdmin)
            {
                return (null, Forbid());
            }

            var customSplitResult = BuildCustomSplitAmounts(
                splitMode: splitMode,
                totalAmount: request.Amount,
                splitAllocations: request.SplitAllocations,
                allowedMemberIds: membersByUserId.Keys.ToHashSet());

            if (customSplitResult.errorMessage != null)
            {
                return (null, BadRequest(new { message = customSplitResult.errorMessage }));
            }

            splitAmountsByUser = customSplitResult.splitAmountsByUser;
            splitUserIds = splitAmountsByUser!.Keys.ToList();
        }

        var expenseDate = request.ExpenseDate == default
            ? DateTime.UtcNow.Date
            : request.ExpenseDate.Date;
        expenseDate = DateTime.SpecifyKind(expenseDate, DateTimeKind.Utc);

        return (new ExpenseMutationContext
        {
            Group = group,
            CategoryId = request.CategoryId,
            PaidByUserId = paidByUserId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Amount = request.Amount,
            ExpenseDate = expenseDate,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            SplitUserIds = splitUserIds,
            SplitAmountsByUser = splitAmountsByUser
        }, null);
    }

    // Hilfsmethode: prüft ob der eingeloggte User Zugriff auf eine Gruppe hat
    // (Gruppe existiert + User ist Mitglied des zugehörigen Trips).
    // Wird von fast allen Endpoints dieses Controllers verwendet.
    private async Task<(Group? group, int? currentUserId, ActionResult? error)> GetAccessibleGroupAsync(
        int groupId,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        if (currentUserId == null)
        {
            return (null, null, Unauthorized());
        }

        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);
        if (group == null)
        {
            return (null, currentUserId, NotFound(new { message = "Group not found." }));
        }

        var trip = await _tripRepository.GetByIdForUserAsync(group.TripId, currentUserId.Value, cancellationToken);
        if (trip == null)
        {
            return (null, currentUserId, NotFound(new { message = "Trip not found." }));
        }

        return (group, currentUserId, null);
    }

    // Wandelt ein Expense-Domainobjekt in ein flaches DTO um; berechnet OwesAmount und IsPaidByUser je Split.
    private static ExpenseResponse MapExpense(Expense expense)
    {
        return new ExpenseResponse
        {
            Id = expense.Id,
            TripId = expense.TripId,
            GroupId = expense.GroupId ?? 0,
            GroupName = expense.Group?.Name ?? string.Empty,
            Currency = expense.Group?.Trip.BaseCurrency ?? expense.Trip.BaseCurrency,
            CategoryId = expense.CategoryId,
            CategoryName = expense.Category.Name,
            PaidByUserId = expense.PaidByUserId,
            PaidByUsername = expense.PaidBy.Username,
            Title = expense.Title,
            Description = expense.Description,
            Amount = expense.Amount,
            Latitude = expense.Latitude,
            Longitude = expense.Longitude,
            ExpenseDate = expense.ExpenseDate,
            CreatedAt = expense.CreatedAt,
            Splits = expense.Splits
                .OrderBy(s => s.User.Username)
                .Select(s => new ExpenseSplitResponse
                {
                    UserId = s.UserId,
                    Username = s.User.Username,
                    Amount = s.Amount,
                    PaidToUserId = expense.PaidByUserId,
                    PaidToUsername = expense.PaidBy.Username,
                    OwesAmount = s.UserId == expense.PaidByUserId ? 0 : s.Amount,
                    IsPaidByUser = s.UserId == expense.PaidByUserId,
                    IsSettled = s.IsSettled,
                    SettledAt = s.SettledAt
                })
                .ToList()
        };
    }

    // Wandelt ein GroupMember-Domainobjekt in ein DTO (UserId, Username, Email, Role, JoinedAt) um.
    private static GroupMemberResponse MapMember(GroupMember member)
    {
        return new GroupMemberResponse
        {
            UserId = member.UserId,
            Username = member.User.Username,
            Email = member.User.Email,
            Role = member.Role,
            JoinedAt = member.JoinedAt
        };
    }

    // Gibt die UserId des ältesten Admins der Gruppe zurück (Fallback-Zahler wenn PaidByUserId nicht gesetzt).
    private static int? GetDefaultAdminUserId(Group group)
    {
        var admin = group.Members
            .Where(member => string.Equals(member.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            .OrderBy(member => member.JoinedAt)
            .ThenBy(member => member.UserId)
            .FirstOrDefault();

        return admin?.UserId;
    }

    // Normalisiert die Rollen-Eingabe (case-insensitive) auf "Admin" oder "Member"; null bei ungültiger Eingabe.
    private static string? NormalizeGroupRole(string role)
    {
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return "Admin";
        }

        if (string.Equals(role, "Member", StringComparison.OrdinalIgnoreCase))
        {
            return "Member";
        }

        return null;
    }

    // Normalisiert den Split-Modus (case-insensitive): "Equal", "Percentage" oder "Amount"; null bei ungültig.
    private static string? NormalizeSplitMode(string? splitMode)
    {
        if (string.IsNullOrWhiteSpace(splitMode))
        {
            return SplitModeEqual;
        }

        if (string.Equals(splitMode, SplitModeEqual, StringComparison.OrdinalIgnoreCase))
        {
            return SplitModeEqual;
        }

        if (string.Equals(splitMode, SplitModePercentage, StringComparison.OrdinalIgnoreCase))
        {
            return SplitModePercentage;
        }

        if (string.Equals(splitMode, SplitModeAmount, StringComparison.OrdinalIgnoreCase))
        {
            return SplitModeAmount;
        }

        return null;
    }

    // Berechnet die Split-Beträge für Percentage- oder Amount-Modus.
    // Percentage: prüft ob alle Prozente 100% ergeben, berechnet dann Beträge.
    // Amount: prüft ob Summe dem Gesamtbetrag entspricht.
    // Gibt splitAmountsByUser (UserId → Betrag) zurück oder eine Fehlermeldung.
    private static (Dictionary<int, decimal>? splitAmountsByUser, string? errorMessage) BuildCustomSplitAmounts(
        string splitMode,
        decimal totalAmount,
        IEnumerable<ExpenseSplitInputRequest>? splitAllocations,
        HashSet<int> allowedMemberIds)
    {
        var normalizedAllocations = splitAllocations?
            .Where(a => a.UserId > 0 && a.Value > 0)
            .GroupBy(a => a.UserId)
            .Select(g => (UserId: g.Key, Value: g.Sum(item => item.Value)))
            .OrderBy(item => item.UserId)
            .ToList() ?? [];

        if (normalizedAllocations.Count == 0)
        {
            return (null, "Provide at least one split value.");
        }

        if (normalizedAllocations.Any(item => !allowedMemberIds.Contains(item.UserId)))
        {
            return (null, "All split users must be members of this group.");
        }

        if (splitMode == SplitModePercentage)
        {
            var percentageTotal = normalizedAllocations.Sum(item => item.Value);
            if (decimal.Abs(percentageTotal - 100m) > SplitTolerance)
            {
                return (null, "Percentage split must total 100%.");
            }

            var amounts = normalizedAllocations
                .ToDictionary(
                    item => item.UserId,
                    item => decimal.Round(totalAmount * item.Value / 100m, 2, MidpointRounding.AwayFromZero));

            return EnsureExactAmountTotal(amounts, totalAmount);
        }

        var roundedAmountAllocations = normalizedAllocations
            .ToDictionary(
                item => item.UserId,
                item => decimal.Round(item.Value, 2, MidpointRounding.AwayFromZero));

        var amountTotal = roundedAmountAllocations.Values.Sum();
        if (decimal.Abs(amountTotal - totalAmount) > SplitTolerance)
        {
            return (null, "Amount split must equal the expense total.");
        }

        return EnsureExactAmountTotal(roundedAmountAllocations, totalAmount);
    }

    // Korrigiert Rundungsdifferenzen bei Split-Beträgen: addiert etwaige Cent-Differenz zum ersten Teilnehmer,
    // sodass die Summe exakt dem Gesamtbetrag entspricht.
    private static (Dictionary<int, decimal>? splitAmountsByUser, string? errorMessage) EnsureExactAmountTotal(
        Dictionary<int, decimal> splitAmountsByUser,
        decimal totalAmount)
    {
        var normalized = splitAmountsByUser
            .Where(item => item.Value > 0)
            .OrderBy(item => item.Key)
            .ToList();

        if (normalized.Count == 0)
        {
            return (null, "Split amounts must be greater than 0.");
        }

        var difference = decimal.Round(totalAmount - normalized.Sum(item => item.Value), 2, MidpointRounding.AwayFromZero);
        if (difference != 0)
        {
            var first = normalized[0];
            normalized[0] = new KeyValuePair<int, decimal>(first.Key, first.Value + difference);
        }

        if (normalized.Any(item => item.Value <= 0))
        {
            return (null, "Split amounts must be greater than 0.");
        }

        return (normalized.ToDictionary(item => item.Key, item => item.Value), null);
    }

    // Berechnet pro Mitglied: TotalPaid (hat bezahlt), TotalShare (Schuldenanteil), NetBalance (Differenz).
    // Settled Splits werden herausgerechnet, sodass bereits bezahlte Schulden nicht mehr zählen.
    // → Wird von GetSettlement genutzt um den Balance-Tab zu füllen.
    private static List<SettlementBalanceResponse> BuildBalances(Group group, IEnumerable<Expense> expenses)
    {
        var balances = group.Members.ToDictionary(
            m => m.UserId,
            m => new SettlementBalanceResponse
            {
                UserId = m.UserId,
                Username = m.User.Username,
                TotalPaid = 0,
                TotalShare = 0,
                NetBalance = 0
            });

        foreach (var expense in expenses)
        {
            // Der Zahler hat den vollen Betrag ausgelegt: erhöht sein TotalPaid und sein Guthaben (NetBalance).
            if (balances.TryGetValue(expense.PaidByUserId, out var payer))
            {
                payer.TotalPaid += expense.Amount;
                payer.NetBalance += expense.Amount;
            }

            // Jeder Split ist der Anteil eines Teilnehmers an dieser Ausgabe.
            foreach (var split in expense.Splits)
            {
                if (!balances.TryGetValue(split.UserId, out var splitUser))
                {
                    continue;  // Split-User ist (nicht mehr) Mitglied → überspringen
                }

                // Der Teilnehmer schuldet seinen Anteil: erhöht seinen TotalShare und senkt sein NetBalance.
                splitUser.TotalShare += split.Amount;
                splitUser.NetBalance -= split.Amount;

                // Ist dieser Anteil bereits BEZAHLT (settled) und der User nicht selbst der Zahler,
                // wird die Schuld neutralisiert: Saldo wird wieder zurückgebucht, sodass die Salden ausgeglichen wirken.
                if (split.IsSettled && split.UserId != expense.PaidByUserId && balances.TryGetValue(expense.PaidByUserId, out var settledPayer))
                {
                    settledPayer.NetBalance -= split.Amount;  // Zahler bekommt das bereits erhaltene Geld nicht doppelt gutgeschrieben
                    splitUser.NetBalance += split.Amount;     // Schuldner ist seine bereits beglichene Schuld wieder los
                }
            }
        }

        foreach (var item in balances.Values)
        {
            item.TotalPaid = decimal.Round(item.TotalPaid, 2);
            item.TotalShare = decimal.Round(item.TotalShare, 2);
            item.NetBalance = decimal.Round(item.NetBalance, 2);
        }

        return balances.Values.ToList();
    }

    // Aggregiert alle offenen (nicht-settled) Splits zu Transfer-Objekten "User A schuldet User B X€".
    // Gruppiert nach From/To-User-Paar und summiert alle offenen Beträge.
    // → Wird von GetSettlement für den Transfer-Tab verwendet.
    private static List<SettlementTransferResponse> BuildTransfers(
        IEnumerable<Expense> expenses)
    {
        return expenses
            .SelectMany(expense => expense.Splits
                .Where(split => !split.IsSettled && split.UserId != expense.PaidByUserId)
                .Select(split => new
                {
                    FromUserId = split.UserId,
                    FromUsername = split.User.Username,
                    ToUserId = expense.PaidByUserId,
                    ToUsername = expense.PaidBy.Username,
                    split.Amount
                }))
            .GroupBy(item => new
            {
                item.FromUserId,
                item.FromUsername,
                item.ToUserId,
                item.ToUsername
            })
            .Select(group => new SettlementTransferResponse
            {
                FromUserId = group.Key.FromUserId,
                FromUsername = group.Key.FromUsername,
                ToUserId = group.Key.ToUserId,
                ToUsername = group.Key.ToUsername,
                Amount = decimal.Round(group.Sum(item => item.Amount), 2)
            })
            .Where(transfer => transfer.Amount > 0.009m)
            .OrderByDescending(transfer => transfer.Amount)
            .ThenBy(transfer => transfer.FromUsername)
            .ThenBy(transfer => transfer.ToUsername)
            .ToList();
    }

    private sealed class ExpenseMutationContext
    {
        public required Group Group { get; init; }
        public required int CategoryId { get; init; }
        public required int PaidByUserId { get; init; }
        public required string Title { get; init; }
        public string? Description { get; init; }
        public required decimal Amount { get; init; }
        public required DateTime ExpenseDate { get; init; }
        public decimal? Latitude { get; init; }
        public decimal? Longitude { get; init; }
        public required IReadOnlyCollection<int> SplitUserIds { get; init; }
        public IReadOnlyDictionary<int, decimal>? SplitAmountsByUser { get; init; }
    }
}
