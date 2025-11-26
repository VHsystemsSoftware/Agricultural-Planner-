using Microsoft.AspNetCore.Identity;
using VHS.Data.Auth.Models.Auth;

namespace VHS.Services;

public interface IUserSettingService
{
    Task<UserSettingDTO?> GetUserSettingsByUserIdAsync(Guid userId);
    Task<UserSettingDTO> CreateUserSettingsAsync(Guid userId);
    Task<UserSettingDTO> UpdateUserSettingsAsync(UserSettingDTO settingsDto);
}

public class UserSettingService : IUserSettingService
{
    private readonly IUnitOfWorkAuth _unitOfWork;
    private readonly UserManager<User> _userManager;

    public UserSettingService(IUnitOfWorkAuth unitOfWork, UserManager<User> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<UserSettingDTO?> GetUserSettingsByUserIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return null;
        }

        var settings = await _unitOfWork.UserSetting.GetFirstOrDefaultAsync(us => us.UserId == userId);

        if (settings == null)
        {
            return await CreateUserSettingsAsync(userId);
        }

        return new UserSettingDTO
        {
            Id = settings.Id,
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PreferredLanguage = settings.PreferredLanguage,
            PreferredTheme = settings.PreferredTheme,
            PreferredMeasurementSystem = settings.PreferredMeasurementSystem,
            PreferredWeightUnit = settings.PreferredWeightUnit,
            PreferredLengthUnit = settings.PreferredLengthUnit,
            PreferredTemperatureUnit = settings.PreferredTemperatureUnit,
            PreferredVolumeUnit = settings.PreferredVolumeUnit,
            PreferredDateTimeFormat = settings.PreferredDateTimeFormat,
            AddedDateTime = settings.AddedDateTime,
            ModifiedDateTime = settings.ModifiedDateTime
        };
    }

    public async Task<UserSettingDTO> CreateUserSettingsAsync(Guid userId)
    {
        var existingSettings = await _unitOfWork.UserSetting.GetFirstOrDefaultAsync(us => us.UserId == userId);
        if (existingSettings != null)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new InvalidOperationException("Cannot create settings for a non-existent user.");

            return new UserSettingDTO
            {
                Id = existingSettings.Id,
                UserId = existingSettings.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PreferredLanguage = existingSettings.PreferredLanguage,
                PreferredTheme = existingSettings.PreferredTheme,
                PreferredMeasurementSystem = existingSettings.PreferredMeasurementSystem,
                PreferredWeightUnit = existingSettings.PreferredWeightUnit,
                PreferredLengthUnit = existingSettings.PreferredLengthUnit,
                PreferredTemperatureUnit = existingSettings.PreferredTemperatureUnit,
                PreferredVolumeUnit = existingSettings.PreferredVolumeUnit,
                PreferredDateTimeFormat = existingSettings.PreferredDateTimeFormat,
            };
        }

        var defaultSettings = new UserSetting
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PreferredLanguage = "en-US",
            PreferredTheme = "Light",
            PreferredMeasurementSystem = "Metric",
            PreferredWeightUnit = "Kilogram",
            PreferredLengthUnit = "Meter",
            PreferredTemperatureUnit = "Celsius",
            PreferredVolumeUnit = "Liter",
            PreferredDateTimeFormat = "dd-MM-yyyy HH:mm",
        };

        await _unitOfWork.UserSetting.AddAsync(defaultSettings);
        await _unitOfWork.SaveChangesAsync();

        var createdForUser = await _userManager.FindByIdAsync(userId.ToString());
        if (createdForUser == null) throw new InvalidOperationException("User disappeared after creating settings.");

        return new UserSettingDTO
        {
            Id = defaultSettings.Id,
            UserId = defaultSettings.UserId,
            FirstName = createdForUser.FirstName,
            LastName = createdForUser.LastName,
            Email = createdForUser.Email,
            PreferredLanguage = defaultSettings.PreferredLanguage,
            PreferredTheme = defaultSettings.PreferredTheme,
            PreferredMeasurementSystem = defaultSettings.PreferredMeasurementSystem,
            PreferredWeightUnit = defaultSettings.PreferredWeightUnit,
            PreferredLengthUnit = defaultSettings.PreferredLengthUnit,
            PreferredTemperatureUnit = defaultSettings.PreferredTemperatureUnit,
            PreferredVolumeUnit = defaultSettings.PreferredVolumeUnit,
            PreferredDateTimeFormat = defaultSettings.PreferredDateTimeFormat,
        };
    }

    public async Task<UserSettingDTO> UpdateUserSettingsAsync(UserSettingDTO settingsDto)
    {
        var user = await _userManager.FindByIdAsync(settingsDto.UserId.ToString());
        if (user == null)
        {
            throw new Exception("User to update not found.");
        }

        user.FirstName = settingsDto.FirstName;
        user.LastName = settingsDto.LastName;
        user.Email = settingsDto.Email;
        user.UserName = settingsDto.Email;
        user.ModifiedDateTime = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var existingSettings = await _unitOfWork.UserSetting.GetFirstOrDefaultAsync(us => us.UserId == settingsDto.UserId);
        if (existingSettings == null)
        {
            throw new Exception("User settings not found to update.");
        }

        existingSettings.PreferredLanguage = settingsDto.PreferredLanguage;
        existingSettings.PreferredTheme = settingsDto.PreferredTheme;
        existingSettings.PreferredMeasurementSystem = settingsDto.PreferredMeasurementSystem;
        existingSettings.PreferredWeightUnit = settingsDto.PreferredWeightUnit;
        existingSettings.PreferredLengthUnit = settingsDto.PreferredLengthUnit;
        existingSettings.PreferredTemperatureUnit = settingsDto.PreferredTemperatureUnit;
        existingSettings.PreferredVolumeUnit = settingsDto.PreferredVolumeUnit;
        existingSettings.PreferredDateTimeFormat = settingsDto.PreferredDateTimeFormat;
        existingSettings.ModifiedDateTime = DateTime.UtcNow;

        _unitOfWork.UserSetting.Update(existingSettings);

        await _unitOfWork.SaveChangesAsync();

        return settingsDto;
    }
}
