using System.Collections.ObjectModel;
using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class AddExpenseViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ITripService _tripService;
    private readonly IAuthService _authService;
    private int _currentUserId;

    // ── Query params ──
    [ObservableProperty] private int _groupId;
    [ObservableProperty] private int _expenseId;
    [ObservableProperty] private string _groupName = string.Empty;

    // ── Form fields ──
    [ObservableProperty] private string _titleText = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _amountText = string.Empty;
    [ObservableProperty] private DateTime _expenseDate = DateTime.Today;

    // ── Error ──
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasError;

    // ── Category ──
    public ObservableCollection<ExpenseCategoryResponse> Categories { get; } = [];
    [ObservableProperty] private ExpenseCategoryResponse? _selectedCategory;

    // ── Split ──
    public ObservableCollection<ExpenseSplitMemberInput> SplitMembers { get; } = [];
    public List<string> SplitModes { get; } = ["Equal", "Percentage", "Amount"];

    [ObservableProperty] private string _selectedSplitMode = "Equal";
    [ObservableProperty] private bool _isCurrentUserAdmin;
    [ObservableProperty] private bool _showSplitValueInputs;
    [ObservableProperty] private string _splitValueHint = string.Empty;
    [ObservableProperty] private bool _showSplitPreview;
    [ObservableProperty] private string _splitPreviewMessage = string.Empty;
    [ObservableProperty] private Color _splitPreviewColor = Colors.Green;

    public bool IsNotCurrentUserAdmin => !IsCurrentUserAdmin;
    public bool CanConfigureCustomSplit => IsCurrentUserAdmin;

    // ── Location ──
    [ObservableProperty] private bool _addLocation;
    [ObservableProperty] private bool _isLoadingLocation;
    [ObservableProperty] private string _locationError = string.Empty;
    [ObservableProperty] private bool _hasLocationError;
    [ObservableProperty] private string _locationAddress = string.Empty;
    [ObservableProperty] private string _locationName = string.Empty;
    [ObservableProperty] private bool _hasLocation;
    [ObservableProperty] private double? _selectedLatitude;
    [ObservableProperty] private double? _selectedLongitude;

    public bool IsNotLoadingLocation => !IsLoadingLocation;

    public AddExpenseViewModel(ITripService tripService, IAuthService authService)
    {
        _tripService = tripService;
        _authService = authService;
        Title = "Add Expense";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("groupId", out var rawGroupId) &&
            int.TryParse(rawGroupId?.ToString(), out var groupId))
        {
            GroupId = groupId;
        }

        if (query.TryGetValue("expenseId", out var rawExpenseId) &&
            int.TryParse(rawExpenseId?.ToString(), out var expenseId))
        {
            ExpenseId = expenseId;
            Title = "Edit Expense";
        }

        if (query.TryGetValue("groupName", out var rawGroupName))
        {
            GroupName = Uri.UnescapeDataString(rawGroupName?.ToString() ?? string.Empty);
        }

        _ = LoadInitialDataAsync();
    }

    private async Task LoadInitialDataAsync()
    {
        if (GroupId <= 0) return;
        IsBusy = true;
        HasError = false;

        try
        {
            var user = await _authService.GetCurrentUserAsync();
            _currentUserId = user.Id;

            var categoriesTask = _tripService.GetExpenseCategoriesAsync(GroupId);
            var membersTask = _tripService.GetGroupMembersAsync(GroupId);
            await Task.WhenAll(categoriesTask, membersTask);

            Categories.Clear();
            var categories = await categoriesTask;
            foreach (var cat in categories)
                Categories.Add(cat);

            var members = await membersTask;
            SplitMembers.Clear();
            foreach (var member in members)
            {
                SplitMembers.Add(new ExpenseSplitMemberInput(member.UserId, member.Username, member.Role, true));
                if (member.UserId == _currentUserId)
                    IsCurrentUserAdmin = string.Equals(member.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            }

            if (ExpenseId > 0)
            {
                var expense = await _tripService.GetGroupExpenseByIdAsync(GroupId, ExpenseId);
                TitleText = expense.Title;
                Description = expense.Description ?? string.Empty;
                AmountText = expense.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                ExpenseDate = expense.ExpenseDate;
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == expense.CategoryId);

                if (expense.HasLocation)
                {
                    SelectedLatitude = (double?)expense.Latitude;
                    SelectedLongitude = (double?)expense.Longitude;
                    LocationName = $"{expense.Latitude:F4}, {expense.Longitude:F4}";
                    HasLocation = true;
                }

                // Split logic
                foreach (var m in SplitMembers) m.IsSelected = false;
                foreach (var split in expense.Splits)
                {
                    var m = SplitMembers.FirstOrDefault(member => member.UserId == split.UserId);
                    if (m != null)
                    {
                        m.IsSelected = true;
                        m.ValueText = split.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
            }

            OnPropertyChanged(nameof(IsNotCurrentUserAdmin));
            OnPropertyChanged(nameof(CanConfigureCustomSplit));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedSplitModeChanged(string value)
    {
        ShowSplitValueInputs = value is "Percentage" or "Amount";
        SplitValueHint = value == "Percentage" ? "%" : value == "Amount" ? "0.00" : string.Empty;
        UpdateSplitPreview();
    }

    partial void OnAmountTextChanged(string value) => UpdateSplitPreview();

    private void UpdateSplitPreview()
    {
        if (SelectedSplitMode is not ("Percentage" or "Amount"))
        {
            ShowSplitPreview = false;
            return;
        }

        var selected = SplitMembers.Where(m => m.IsSelected).ToList();
        if (selected.Count == 0)
        {
            ShowSplitPreview = false;
            return;
        }

        if (SelectedSplitMode == "Percentage")
        {
            var total = selected.Sum(m => decimal.TryParse(m.ValueText, out var v) ? v : 0);
            ShowSplitPreview = true;
            if (Math.Abs(total - 100) <= 0.01m)
            {
                SplitPreviewMessage = "✓ Percentages sum to 100%";
                SplitPreviewColor = Color.FromArgb("#4CAF50");
            }
            else
            {
                SplitPreviewMessage = $"Percentages sum to {total:F1}% (must equal 100%)";
                SplitPreviewColor = Color.FromArgb("#E53935");
            }
        }
        else
        {
            if (!decimal.TryParse(AmountText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var totalAmount) || totalAmount == 0)
            {
                ShowSplitPreview = false;
                return;
            }

            var splitSum = selected.Sum(m => decimal.TryParse(m.ValueText, out var v) ? v : 0);
            ShowSplitPreview = true;
            if (Math.Abs(splitSum - totalAmount) <= 0.01m)
            {
                SplitPreviewMessage = $"✓ Amounts sum to {totalAmount:F2}";
                SplitPreviewColor = Color.FromArgb("#4CAF50");
            }
            else
            {
                SplitPreviewMessage = $"Amounts sum to {splitSum:F2} (must equal {totalAmount:F2})";
                SplitPreviewColor = Color.FromArgb("#E53935");
            }
        }
    }

    // Called from code-behind when the user taps the map
    public void SetLocation(double latitude, double longitude)
    {
        SelectedLatitude = latitude;
        SelectedLongitude = longitude;
        LocationName = $"{latitude:F4}, {longitude:F4}";
        HasLocation = true;
        HasLocationError = false;
    }

    [RelayCommand]
    private async Task UseCurrentLocationAsync()
    {
        IsLoadingLocation = true;
        HasLocationError = false;
        OnPropertyChanged(nameof(IsNotLoadingLocation));

        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                LocationError = "Location permission denied.";
                HasLocationError = true;
                return;
            }

            var location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium));

            if (location == null)
            {
                LocationError = "Could not determine location.";
                HasLocationError = true;
                return;
            }

            SetLocation(location.Latitude, location.Longitude);

            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
            var place = placemarks?.FirstOrDefault();
            if (place != null)
            {
                var parts = new[] { place.FeatureName, place.Locality, place.CountryName }
                    .Where(s => !string.IsNullOrWhiteSpace(s));
                LocationName = string.Join(", ", parts);
            }
        }
        catch (Exception ex)
        {
            LocationError = ex.Message;
            HasLocationError = true;
        }
        finally
        {
            IsLoadingLocation = false;
            OnPropertyChanged(nameof(IsNotLoadingLocation));
        }
    }

    [RelayCommand]
    private async Task GeocodeAddressAsync()
    {
        if (string.IsNullOrWhiteSpace(LocationAddress)) return;

        IsLoadingLocation = true;
        HasLocationError = false;
        OnPropertyChanged(nameof(IsNotLoadingLocation));

        try
        {
            var locations = await Geocoding.Default.GetLocationsAsync(LocationAddress);
            var loc = locations?.FirstOrDefault();
            if (loc == null)
            {
                LocationError = "Address not found.";
                HasLocationError = true;
                return;
            }

            SetLocation(loc.Latitude, loc.Longitude);
            LocationName = LocationAddress;
        }
        catch (Exception ex)
        {
            LocationError = ex.Message;
            HasLocationError = true;
        }
        finally
        {
            IsLoadingLocation = false;
            OnPropertyChanged(nameof(IsNotLoadingLocation));
        }
    }

    [RelayCommand]
    private void ClearLocation()
    {
        SelectedLatitude = null;
        SelectedLongitude = null;
        LocationName = string.Empty;
        LocationAddress = string.Empty;
        HasLocation = false;
    }

    [RelayCommand]
    private async Task SaveExpenseAsync()
    {
        HasError = false;

        if (string.IsNullOrWhiteSpace(TitleText))
        {
            ErrorMessage = "Please enter a title.";
            HasError = true;
            return;
        }

        if (!decimal.TryParse(AmountText, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            ErrorMessage = "Please enter a valid amount greater than 0.";
            HasError = true;
            return;
        }

        if (SelectedCategory == null)
        {
            ErrorMessage = "Please select a category.";
            HasError = true;
            return;
        }

        var selectedMembers = SplitMembers.Where(m => m.IsSelected).ToList();
        if (selectedMembers.Count == 0)
        {
            ErrorMessage = "Select at least one member to split with.";
            HasError = true;
            return;
        }

        if (SelectedSplitMode == "Percentage")
        {
            var pctTotal = selectedMembers.Sum(m => decimal.TryParse(m.ValueText, out var v) ? v : 0);
            if (Math.Abs(pctTotal - 100) > 0.01m)
            {
                ErrorMessage = $"Percentages must sum to 100% (currently {pctTotal:F1}%).";
                HasError = true;
                return;
            }
        }
        else if (SelectedSplitMode == "Amount")
        {
            var splitSum = selectedMembers.Sum(m => decimal.TryParse(m.ValueText, out var v) ? v : 0);
            if (Math.Abs(splitSum - amount) > 0.01m)
            {
                ErrorMessage = $"Split amounts must sum to {amount:F2}.";
                HasError = true;
                return;
            }
        }

        IsBusy = true;

        try
        {
            var request = new CreateExpenseRequest
            {
                Title = TitleText.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                Amount = amount,
                CategoryId = SelectedCategory.Id,
                PaidByUserId = _currentUserId > 0 ? _currentUserId : null,
                ExpenseDate = ExpenseDate,
                Latitude = HasLocation && SelectedLatitude.HasValue ? (decimal?)SelectedLatitude.Value : null,
                Longitude = HasLocation && SelectedLongitude.HasValue ? (decimal?)SelectedLongitude.Value : null,
                SplitMode = SelectedSplitMode,
                SplitUserIds = SelectedSplitMode == "Equal"
                    ? selectedMembers.Select(m => m.UserId).ToList()
                    : null,
                SplitAllocations = SelectedSplitMode != "Equal"
                    ? selectedMembers
                        .Select(m => new ExpenseSplitInputRequest
                        {
                            UserId = m.UserId,
                            Value = decimal.TryParse(m.ValueText, out var v) ? v : 0
                        })
                        .ToList()
                    : null
            };

            if (ExpenseId > 0)
            {
                await _tripService.UpdateExpenseAsync(GroupId, ExpenseId, request);
            }
            else
            {
                await _tripService.CreateExpenseAsync(GroupId, request);
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
