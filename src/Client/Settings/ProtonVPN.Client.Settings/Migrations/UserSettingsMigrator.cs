/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Migrations;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Crypto.Contracts.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Serialization.Contracts.Json;

namespace ProtonVPN.Client.Settings.Migrations;

public class UserSettingsMigrator : IUserSettingsMigrator
{
    private readonly ISettings _settings;
    private readonly ILogger _logger;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ISettingsCorrector _settingsCorrector;
    private readonly IProfilesMigrator _profilesMigrator;

    public UserSettingsMigrator(ISettings settings,
        ILogger logger,
        IJsonSerializer jsonSerializer,
        ISettingsCorrector settingsCorrector,
        IProfilesMigrator profilesMigrator)
    {
        _settings = settings;
        _logger = logger;
        _jsonSerializer = jsonSerializer;
        _settingsCorrector = settingsCorrector;
        _profilesMigrator = profilesMigrator;
    }

    public void Migrate()
    {
        string? username = _settings.Username;
        if (_settings.IsUserSettingsMigrationDone || string.IsNullOrWhiteSpace(username))
        {
            return;
        }

        Dictionary<string, Dictionary<string, string?>>? settingsByUsername = _settings.LegacySettingsByUsername;
        if (settingsByUsername is not null && settingsByUsername.Count > 0)
        {
            KeyValuePair<string, Dictionary<string, string?>>? userSettingsPair = FindSettingsByUsername(username, settingsByUsername);
            if (userSettingsPair is null)
            {
                _logger.Info<AppLog>($"No user settings to migrate for username '{username}'.");
            }
            else
            {
                _logger.Info<AppLog>($"Migrating user settings for username '{username}'.");
                MigrateUserSettings(userSettingsPair.Value.Value);
                _logger.Info<AppLog>($"Finished migrating user settings for username '{username}'. Removing previous settings.");
                RemoveMigratedUserSettings(settingsByUsername, userSettingsPair.Value.Key);
                _logger.Info<AppLog>($"Removed previous settings.");
            }
        }
        else
        {
            _logger.Info<AppLog>("No user settings to migrate.");
        }

        _settings.IsUserSettingsMigrationDone = true;
        _settingsCorrector.Correct();
    }

    private void RemoveMigratedUserSettings(Dictionary<string, Dictionary<string, string?>> settingsByUsername, string username)
    {
        if (!settingsByUsername.Remove(username))
        {
            _logger.Warn<AppLog>("The old user settings were not removed.");
        }
        if (settingsByUsername.Count > 0)
        {
            _logger.Info<AppLog>($"{settingsByUsername.Count} user settings left to migrate.");
            _settings.LegacySettingsByUsername = settingsByUsername;
        }
        else
        {
            _logger.Info<AppLog>("No more user settings left to migrate.");
            _settings.LegacySettingsByUsername = null;
        }
    }

    private KeyValuePair<string, Dictionary<string, string?>>? FindSettingsByUsername(string username,
        Dictionary<string, Dictionary<string, string?>> settingsByUsername)
    {
        IEnumerable<string> usernameComparisonAttempts = GetUsernameComparisonAttempt(username);
        foreach (string usernameComparisonAttempt in usernameComparisonAttempts)
        {
            if (settingsByUsername.TryGetValue(usernameComparisonAttempt, out Dictionary<string, string?>? userSettings) && userSettings is not null)
            {
                return new KeyValuePair<string, Dictionary<string, string?>>(usernameComparisonAttempt, userSettings);
            }
        }
        return null;
    }

    private IEnumerable<string> GetUsernameComparisonAttempt(string username)
    {
        yield return username.ToLowerInvariant();
        yield return username.ToLowerInvariant().Replace(" ", ".");
        yield return username.ToLowerInvariant().Replace(".", " ");

        int indexOfEmailAt = username.IndexOf('@');
        if (indexOfEmailAt >= 0)
        {
            string usernameWithoutEmailSuffix = username.Substring(0, indexOfEmailAt);
            yield return usernameWithoutEmailSuffix.ToLowerInvariant();
            yield return usernameWithoutEmailSuffix.ToLowerInvariant().Replace(".", " ");
        }
    }

    private void MigrateUserSettings(Dictionary<string, string?> userSettings)
    {
        MigrateBoolUserSetting(userSettings, nameof(IUserSettings.IsNotificationEnabled), val => { _settings.IsNotificationEnabled = val; });
        MigrateBoolUserSetting(userSettings, nameof(IUserSettings.IsVpnAcceleratorEnabled), val => { _settings.IsVpnAcceleratorEnabled = val; });
        MigrateBoolUserSetting(userSettings, nameof(IUserSettings.IsIpv6LeakProtectionEnabled), val => { _settings.IsIpv6LeakProtectionEnabled = val; });
        MigrateBoolUserSetting(userSettings, nameof(IUserSettings.IsNetShieldEnabled), val => { _settings.IsNetShieldEnabled = val; });
        MigrateNetShieldMode(userSettings);

        MigrateConnectionKeyPair(userSettings);
        MigrateConnectionCertificate(userSettings);

        MigrateBoolUserSetting(userSettings, nameof(IUserSettings.IsAutoConnectEnabled), val => { _settings.IsAutoConnectEnabled = val; });

        MigrateNatType(userSettings);

        MigrateBoolUserSetting(userSettings, nameof(IUserSettings.IsCustomDnsServersEnabled), val => { _settings.IsCustomDnsServersEnabled = val; });
        MigrateJsonUserSetting<List<CustomDnsServer>>(userSettings, nameof(IUserSettings.CustomDnsServersList), val => { _settings.CustomDnsServersList = val; });

        MigrateBoolUserSetting(userSettings, nameof(IUserSettings.IsPortForwardingNotificationEnabled), val => { _settings.IsPortForwardingNotificationEnabled = val; });
        MigrateBoolUserSetting(userSettings, nameof(IUserSettings.IsPortForwardingEnabled), val => { _settings.IsPortForwardingEnabled = val; });

        MigrateBoolUserSetting(userSettings, nameof(IUserSettings.IsSplitTunnelingEnabled), val => { _settings.IsSplitTunnelingEnabled = val; });
        MigrateJsonUserSetting<List<SplitTunnelingApp>>(userSettings, nameof(IUserSettings.SplitTunnelingInverseAppsList), val => { _settings.SplitTunnelingInverseAppsList = val; });
        MigrateJsonUserSetting<List<SplitTunnelingApp>>(userSettings, nameof(IUserSettings.SplitTunnelingStandardAppsList), val => { _settings.SplitTunnelingStandardAppsList = val; });
        MigrateJsonUserSetting<List<SplitTunnelingIpAddress>>(userSettings, nameof(IUserSettings.SplitTunnelingInverseIpAddressesList), val => { _settings.SplitTunnelingInverseIpAddressesList = val; });
        MigrateJsonUserSetting<List<SplitTunnelingIpAddress>>(userSettings, nameof(IUserSettings.SplitTunnelingStandardIpAddressesList), val => { _settings.SplitTunnelingStandardIpAddressesList = val; });

        MigrateOpenVpnAdapter(userSettings);
        MigrateVpnProtocol(userSettings);

        MigrateProfilesAndQuickConnectProfileId(userSettings);

        ConfigureWelcomeOverlays();
    }

    private void MigrateConnectionKeyPair(Dictionary<string, string?> userSettings)
    {
        if (userSettings.TryGetValue(nameof(IUserSettings.ConnectionKeyPair), out string? rawSettingValue) &&
            !string.IsNullOrWhiteSpace(rawSettingValue))
        {
            try
            {
                ConnectionAsymmetricKeyPair? deserializedValue = _jsonSerializer.DeserializeFromString<ConnectionAsymmetricKeyPair>(rawSettingValue.Decrypt());
                if (deserializedValue is not null)
                {
                    _settings.ConnectionKeyPair = deserializedValue;
                }
            }
            catch (Exception e)
            {
                _logger.Error<AppLog>($"Error while migrating connection key pairs.", e);
            }
        }
    }

    private void MigrateConnectionCertificate(Dictionary<string, string?> userSettings)
    {
        if (_settings.ConnectionKeyPair is null)
        {
            return; // If no connection key pair exists, the certificate is worthless as both are necessary in the connection process
        }

        if (userSettings.TryGetValue(nameof(IUserSettings.ConnectionCertificate), out string? rawSettingValue) &&
            !string.IsNullOrWhiteSpace(rawSettingValue))
        {
            try
            {
                _settings.ConnectionCertificate = new ConnectionCertificate()
                {
                    Pem = rawSettingValue.Decrypt(),
                    RequestUtcDate = DateTimeOffset.MinValue,
                    RefreshUtcDate = DateTimeOffset.MinValue,
                    ExpirationUtcDate = DateTimeOffset.MinValue,
                };
            }
            catch (Exception e)
            {
                _logger.Error<AppLog>($"Error while migrating connection certificate.", e);
            }
        }
    }

    private void MigrateBoolUserSetting(Dictionary<string, string?> userSettings, string settingName, Action<bool> setter)
    {
        if (userSettings.TryGetValue(settingName, out string? rawSettingValue) && rawSettingValue is not null)
        {
            if (bool.TryParse(rawSettingValue, out bool parseResult))
            {
                setter(parseResult);
            }
        }
    }

    private void MigrateJsonUserSetting<T>(Dictionary<string, string?> userSettings, string settingName, Action<T> setter)
        where T : class
    {
        if (userSettings.TryGetValue(settingName, out string? rawSettingValue) && rawSettingValue is not null)
        {
            try
            {
                T? deserializedValue = _jsonSerializer.DeserializeFromString<T>(rawSettingValue);
                if (deserializedValue is not null)
                {
                    setter(deserializedValue);
                }
            }
            catch (Exception e)
            {
                _logger.Error<AppLog>($"Error while migrating user settings.", e);
            }
        }
    }

    private void MigrateNatType(Dictionary<string, string?> userSettings)
    {
        if (userSettings.TryGetValue(nameof(IUserSettings.NatType), out string? rawSettingValue) && rawSettingValue is not null)
        {
            _settings.NatType = rawSettingValue switch
            {
                "true" => NatType.Moderate,
                _ => DefaultSettings.NatType,
            };
        }
    }

    private void MigrateOpenVpnAdapter(Dictionary<string, string?> userSettings)
    {
        if (userSettings.TryGetValue(nameof(IUserSettings.OpenVpnAdapter), out string? rawSettingValue) &&
            rawSettingValue is not null && Enum.TryParse(rawSettingValue, out OpenVpnAdapter result))
        {
            _settings.OpenVpnAdapter = result;
        }
    }

    private void MigrateVpnProtocol(Dictionary<string, string?> userSettings)
    {
        if (userSettings.TryGetValue(nameof(IUserSettings.VpnProtocol), out string? rawSettingValue) &&
            rawSettingValue is not null && Enum.TryParse(rawSettingValue, out VpnProtocol result))
        {
            _settings.VpnProtocol = result;
        }
    }

    private void MigrateNetShieldMode(Dictionary<string, string?> userSettings)
    {
        if (userSettings.TryGetValue(nameof(IUserSettings.NetShieldMode), out string? rawSettingValue) &&
            rawSettingValue is not null && Enum.TryParse(rawSettingValue, out NetShieldMode result))
        {
            _settings.NetShieldMode = result;
        }
    }

    private void MigrateProfilesAndQuickConnectProfileId(Dictionary<string, string?> userSettings)
    {
        try
        {
            _logger.Info<AppLog>("Migrating profiles.");

            userSettings.TryGetValue(GlobalSettingsMigrator.QUICK_CONNECT_PROFILE_ID_SETTING_KEY, out string? quickConnectProfileId);

            userSettings.TryGetValue(GlobalSettingsMigrator.PROFILES_SETTING_KEY, out string? rawProfiles);
            List<LegacyProfile> legacyProfiles = 
                rawProfiles is not null && 
                _jsonSerializer.DeserializeFromString<List<LegacyProfile>?>(rawProfiles) is List<LegacyProfile> deserializedProfiles
                    ? deserializedProfiles
                    : [];

            _profilesMigrator.Migrate(legacyProfiles, quickConnectProfileId);
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>($"Error while migrating profiles.", e);
        }
    }

    private void ConfigureWelcomeOverlays()
    {
        _settings.WasWelcomeOverlayDisplayed = true;
        _settings.WasWelcomeB2BOverlayDisplayed = true;
        _settings.LastSeenWhatsNewOverlayVersion = 0;
    }
}