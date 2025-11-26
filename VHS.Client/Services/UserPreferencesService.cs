using Blazored.LocalStorage;
using System.Text.Json;
using VHS.Services.Auth.DTO;

namespace VHS.Client.Services;

public class UserPreferencesService
{
    private readonly ILocalStorageService _localStorage;
    private UserSettingDTO? _userSettings;
    private bool _isLoaded = false;

    private static readonly Dictionary<string, string> DateOnlyFormats = new()
    {
        // 4-Digit Year
        { "dd-MM-yyyy HH:mm", "dd-MM-yyyy" },
        { "dd-MMM-yyyy HH:mm", "dd-MMM-yyyy" },
        { "d-M-yyyy H:mm", "d-M-yyyy" },
        { "MM/dd/yyyy hh:mm tt", "MM/dd/yyyy" },
        { "MMM d, yyyy h:mm tt", "MMM d, yyyy" },
        { "M/d/yyyy h:mm tt", "M/d/yyyy" },
        { "MMMM d, yyyy h:mm tt", "MMMM d, yyyy" },
        { "yyyy-MM-dd HH:mm", "yyyy-MM-dd" },
        { "dddd, d MMMM yyyy HH:mm", "dddd, d MMMM yyyy" },
        { "dddd, MMMM d, yyyy h:mm tt", "dddd, MMMM d, yyyy" },
        { "ddd, d MMM yyyy HH:mm", "ddd, d MMM yyyy" },
        { "ddd, MMM d, yyyy h:mm tt", "ddd, MMM d, yyyy" },

        // 2-Digit Year
        { "dd-MM-yy HH:mm", "dd-MM-yy" },
        { "dd-MMM-yy HH:mm", "dd-MMM-yy" },
        { "d-M-yy H:mm", "d-M-yy" },
        { "MM/dd/yy hh:mm tt", "MM/dd/yy" },
        { "MMM d, yy h:mm tt", "MMM d, yy" },
        { "M/d/yy h:mm tt", "M/d/yy" },
        { "MMMM d, yy h:mm tt", "MMMM d, yy" },
        { "yy-MM-dd HH:mm", "yy-MM-dd" },
        { "dddd, d MMMM yy HH:mm", "dddd, d MMMM yy" },
        { "dddd, MMMM d, yy h:mm tt", "dddd, MMMM d, yy" },
        { "ddd, d MMM yy HH:mm", "ddd, d MMM yy" },
        { "ddd, MMM d, yy h:mm tt", "ddd, MMM d, yy" }
    };

    public UserPreferencesService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<string> GetDateTimeFormatAsync(bool dateOnly = false)
    {
        await LoadUserSettingsAsync();
        var format = _userSettings?.PreferredDateTimeFormat ?? "dd-MM-yyyy HH:mm";

        if (dateOnly && DateOnlyFormats.TryGetValue(format, out var dateOnlyFormat))
        {
            return dateOnlyFormat;
        }

        return format;
    }

    public async Task<string> GetWeightUnitAsync()
    {
        await LoadUserSettingsAsync();
        return _userSettings?.PreferredWeightUnit ?? "Kilogram";
    }

    public async Task<UserSettingDTO?> GetUserSettingsAsync()
    {
        await LoadUserSettingsAsync();
        return _userSettings;
    }

    private async Task LoadUserSettingsAsync()
    {
        if (_isLoaded) return;

        try
        {
            var settingsJson = await _localStorage.GetItemAsStringAsync("VHS_USER_SETTINGS");
            if (!string.IsNullOrWhiteSpace(settingsJson))
            {
                _userSettings = JsonSerializer.Deserialize<UserSettingDTO>(settingsJson);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading user settings: {ex.Message}");
            _userSettings = null;
        }
        finally
        {
            _isLoaded = true;
        }
    }

    public void ClearCache()
    {
        _userSettings = null;
        _isLoaded = false;
    }
}
