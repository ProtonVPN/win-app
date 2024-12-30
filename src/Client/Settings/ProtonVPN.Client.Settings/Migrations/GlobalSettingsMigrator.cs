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

using System.Xml;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Migrations;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Crypto.Contracts.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Serialization.Contracts.Json;

namespace ProtonVPN.Client.Settings.Migrations;

public class GlobalSettingsMigrator : IGlobalSettingsMigrator
{
    public const string QUICK_CONNECT_PROFILE_ID_SETTING_KEY = "LegacyQuickConnectProfileId";
    public const string PROFILES_SETTING_KEY = "LegacyProfiles";

    // ISettings is not used to enforce not writing any user settings
    private readonly IGlobalSettings _globalSettings;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly Lazy<XmlDocument> _xmlDocument;

    public GlobalSettingsMigrator(
        IGlobalSettings globalSettings,
        ILogger logger,
        IConfiguration configuration,
        IJsonSerializer jsonSerializer)
    {
        _globalSettings = globalSettings;
        _logger = logger;
        _configuration = configuration;
        _jsonSerializer = jsonSerializer;

        _xmlDocument = new Lazy<XmlDocument>(() => new XmlDocument());
    }

    public void Migrate()
    {
        if (_globalSettings.IsGlobalSettingsMigrationDone)
        {
            return;
        }

        string? legacyUserConfigFilePath = GetLegacyUserConfigFilePath();
        if (legacyUserConfigFilePath is null || !File.Exists(legacyUserConfigFilePath))
        {
            _logger.Info<AppLog>($"The old settings file doesn't exist ({legacyUserConfigFilePath})");
            return;
        }

        try
        {
            _logger.Info<AppLog>($"Migrating the settings file ({legacyUserConfigFilePath})");
            MigrateSettingsFile(legacyUserConfigFilePath);

            if (Directory.Exists(_configuration.LegacyAppLocalData))
            {
                _logger.Info<AppLog>($"Deleting folder ({_configuration.LegacyAppLocalData})");
                Directory.Delete(_configuration.LegacyAppLocalData, true);
            }
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Failed to migrate global settings.", e);
        }
    }

    public string? GetLegacyUserConfigFilePath()
    {
        string appDataFolderPath = _configuration.LegacyAppLocalData;
        if (!Directory.Exists(appDataFolderPath))
        {
            return null;
        }

        // v3
        string? latestVersionFolderPath = GetOldUserConfigPathByFolderPrefix(appDataFolderPath, "ProtonVPN_Url_");
        if (string.IsNullOrEmpty(latestVersionFolderPath))
        {
            // v2
            latestVersionFolderPath = GetOldUserConfigPathByFolderPrefix(appDataFolderPath, "ProtonVPN.exe_Url_");
        }

        return string.IsNullOrEmpty(latestVersionFolderPath) ? null : Path.Combine(latestVersionFolderPath, "user.config");
    }

    public string? GetOldUserConfigPathByFolderPrefix(string appDataFolderPath, string prefix)
    {
        DateTime latestVersionTime = DateTime.MinValue;
        string? latestVersionFolderPath = null;

        foreach (string folderPath in Directory.GetDirectories(appDataFolderPath))
        {
            string? folderName = Path.GetFileName(folderPath);
            if (folderName != null && folderName.StartsWith(prefix))
            {
                foreach (string versionFolderPath in Directory.GetDirectories(folderPath))
                {
                    DateTime versionFolderLastWriteTime = Directory.GetLastWriteTime(versionFolderPath);
                    if (versionFolderLastWriteTime > latestVersionTime)
                    {
                        latestVersionTime = versionFolderLastWriteTime;
                        latestVersionFolderPath = versionFolderPath;
                    }
                }
            }
        }

        return latestVersionFolderPath;
    }

    private void MigrateSettingsFile(string filePath)
    {
        _xmlDocument.Value.Load(filePath);

        List<string> usernames = GetUsernames();
        Dictionary<string, Dictionary<string, string?>> userSettingsByUsername = new();
        foreach (string username in usernames)
        {
            userSettingsByUsername.Add(username.ToLowerInvariant(), MigrateUser(username));
            _logger.Info<AppLog>($"Migrated user '{username}' to temporary global setting.");
        }

        _globalSettings.LegacySettingsByUsername =
            userSettingsByUsername is not null && userSettingsByUsername.Count > 0 ? userSettingsByUsername : null;

        MigrateGlobalSettings();

        _logger.Info<AppLog>("Global migration completed.");
        _globalSettings.IsGlobalSettingsMigrationDone = true;
    }

    private List<string> GetUsernames()
    {
        string? jsonUserAccessTokens = GetSettingValue("UserAccessToken");
        if (!string.IsNullOrEmpty(jsonUserAccessTokens))
        {
            List<LegacyUserSetting>? userAccessTokens = _jsonSerializer.DeserializeFromString<List<LegacyUserSetting>>(jsonUserAccessTokens);
            if (userAccessTokens is not null)
            {
                return userAccessTokens
                    .Where(uat => !string.IsNullOrEmpty(uat.User))
                    // Ensure the user with access token is the last, so it remains as the currently logged in user
                    .OrderBy(uat => !string.IsNullOrEmpty(uat.Value?.Decrypt()))
                    .Select(uat => uat.User!.ToLower())
                    .ToList();
            }
        }

        return [];
    }

    private string? GetSettingValue(string name)
    {
        string xPath = $"/configuration/userSettings/ProtonVPN.Properties.Settings/setting[@name='{name}']/value";
        return _xmlDocument.Value.SelectSingleNode(xPath)?.InnerText;
    }

    private Dictionary<string, string?> MigrateUser(string username)
    {
        Dictionary<string, string?> userSettings = new()
        {
            { nameof(IUserSettings.IsNotificationEnabled), GetSettingValue("ShowNotifications") },
            { nameof(IUserSettings.IsVpnAcceleratorEnabled), GetSettingValue("VpnAcceleratorEnabled") },
            { nameof(IUserSettings.IsIpv6LeakProtectionEnabled), GetSettingValue("Ipv6LeakProtection") },
            { nameof(IUserSettings.IsNetShieldEnabled), GetUserSetting(username, "UserNetShieldEnabled") },
            { nameof(IUserSettings.NetShieldMode), MigrateNetShieldMode(username) },

            { nameof(IUserSettings.ConnectionKeyPair), GetConnectionKeyPair(username) },
            { nameof(IUserSettings.ConnectionCertificate), GetConnectionCertificate(username) },

            { nameof(IUserSettings.IsAutoConnectEnabled), GetSettingValue("ConnectOnAppStart") },

            { nameof(IUserSettings.NatType), GetUserSetting(username, "UserModerateNat") },

            { nameof(IUserSettings.IsCustomDnsServersEnabled), GetUserSetting(username, "UserCustomDnsEnabled") },
            { nameof(IUserSettings.CustomDnsServersList), MigrateCustomDns(username) },

            { nameof(IUserSettings.IsPortForwardingNotificationEnabled), GetSettingValue("PortForwardingNotificationsEnabled") },
            { nameof(IUserSettings.IsPortForwardingEnabled), GetUserSetting(username, "UserPortForwardingEnabled") },

            { nameof(IUserSettings.IsSplitTunnelingEnabled), GetSettingValue("SplitTunnelingEnabled") },
            { nameof(IUserSettings.SplitTunnelingInverseAppsList), MigrateSplitTunnelingApps("SplitTunnelingAllowApps") },
            { nameof(IUserSettings.SplitTunnelingStandardAppsList), MigrateSplitTunnelingApps("SplitTunnelingBlockApps") },
            { nameof(IUserSettings.SplitTunnelingInverseIpAddressesList), MigrateSplitTunnelingIps("SplitTunnelIncludeIps") },
            { nameof(IUserSettings.SplitTunnelingStandardIpAddressesList), MigrateSplitTunnelingIps("SplitTunnelExcludeIps") },

            { nameof(IUserSettings.OpenVpnAdapter), MigrateOpenVpnNetworkAdapter() },
            { nameof(IUserSettings.VpnProtocol), MigrateVpnProtocol() },

            { QUICK_CONNECT_PROFILE_ID_SETTING_KEY, GetUserSetting(username, "UserQuickConnect") },
            { PROFILES_SETTING_KEY, MigrateProfilesForLaterMigration(username) },
        };

        MigrateUserAuthData(username);

        return userSettings;
    }

    private string? GetUserSetting(string username, string setting)
    {
        return GetUserSetting<LegacyUserSetting>(username, setting)?.Value;
    }

    private T? GetUserSetting<T>(string username, string setting) where T : LegacyUserSettingBase
    {
        try
        {
            string? userSetting = GetSettingValue(setting);
            if (string.IsNullOrEmpty(userSetting))
            {
                return default;
            }

            return _jsonSerializer.DeserializeFromString<List<T>>(userSetting)?
                .Where(s => s.User != null && s.User.EqualsIgnoringCase(username))
                .FirstOrDefault();
        }
        catch
        {
            return default;
        }
    }

    private string? GetConnectionKeyPair(string username)
    {
        string? publicKey = GetUserSetting(username, "UserAuthenticationPublicKey")?.Decrypt();
        string? secretKey = GetUserSetting(username, "UserAuthenticationSecretKey")?.Decrypt();

        if (publicKey is null || secretKey is null)
        {
            return null;
        }

        ConnectionAsymmetricKeyPair connectionKeyPair = new()
        {
            PublicKey = publicKey,
            SecretKey = secretKey,
        };

        return _jsonSerializer.SerializeToString(connectionKeyPair).Encrypt();
    }

    private string? GetConnectionCertificate(string username)
    {
        try
        {
            string? userAuthenticationCertificatePem = GetUserSetting(username, "UserAuthenticationCertificatePem");
            return string.IsNullOrWhiteSpace(userAuthenticationCertificatePem) ? null : userAuthenticationCertificatePem;
        }
        catch
        {
            return null;
        }
    }

    private string? MigrateCustomDns(string username)
    {
        try
        {
            LegacyUserCustomDnsSetting? customDnsIpsJson = GetUserSetting<LegacyUserCustomDnsSetting>(username, "UserCustomDnsIps");
            if (customDnsIpsJson is null || customDnsIpsJson.Value?.Count == 0)
            {
                return null;
            }

            List<CustomDnsServer>? customDnsServersList = customDnsIpsJson.Value?
                .Where(lsip => !string.IsNullOrEmpty(lsip.Ip))
                .Select(lsip => new CustomDnsServer(lsip.Ip!, lsip.Enabled))
                .ToList();

            return customDnsServersList is null ? null : _jsonSerializer.SerializeToString(customDnsServersList);
        }
        catch
        {
            return null;
        }
    }

    private string? MigrateSplitTunnelingApps(string oldSplitTunnelingAppListName)
    {
        try
        {
            string? splitTunnelingAllowAppsJson = GetSettingValue(oldSplitTunnelingAppListName);
            if (string.IsNullOrEmpty(splitTunnelingAllowAppsJson))
            {
                return null;
            }

            List<LegacySplitTunnelApp>? splitTunnelingAllowApps = _jsonSerializer.DeserializeFromString<List<LegacySplitTunnelApp>>(splitTunnelingAllowAppsJson);
            if (splitTunnelingAllowApps is null)
            {
                return null;
            }

            List<SplitTunnelingApp>? splitTunnelingApps = splitTunnelingAllowApps?
                .Where(sta => !string.IsNullOrEmpty(sta.Path))
                .Select(sta => new SplitTunnelingApp(sta.Path!, sta.Enabled))
                .ToList();

            return splitTunnelingApps is null ? null : _jsonSerializer.SerializeToString(splitTunnelingApps);
        }
        catch
        {
            return null;
        }
    }

    private string? MigrateSplitTunnelingIps(string oldSplitTunnelingIpsListName)
    {
        try
        {
            string? splitTunnelingAllowIpsJson = GetSettingValue(oldSplitTunnelingIpsListName);
            if (string.IsNullOrEmpty(splitTunnelingAllowIpsJson))
            {
                return null;
            }

            List<LegacySettingIpAddress>? splitTunnelingAllowIps = _jsonSerializer.DeserializeFromString<List<LegacySettingIpAddress>>(splitTunnelingAllowIpsJson);
            if (splitTunnelingAllowIps is null)
            {
                return null;
            }

            List<SplitTunnelingIpAddress>? splitTunnelingIpAddresses = splitTunnelingAllowIps?
                .Where(sti => !string.IsNullOrEmpty(sti.Ip))
                .Select(sti => new SplitTunnelingIpAddress(sti.Ip!, sti.Enabled))
                .ToList();

            return splitTunnelingIpAddresses is null ? null : _jsonSerializer.SerializeToString(splitTunnelingIpAddresses);
        }
        catch
        {
            return null;
        }
    }

    private string? MigrateOpenVpnNetworkAdapter()
    {
        return GetSettingValue("NetworkAdapterType") switch
        {
            "0" => nameof(OpenVpnAdapter.Tap),
            "1" => nameof(OpenVpnAdapter.Tun),
            _ => nameof(OpenVpnAdapter.Tun),
        };
    }

    private string? MigrateVpnProtocol()
    {
        return GetSettingValue("OvpnProtocol") switch
        {
            "auto" => nameof(VpnProtocol.Smart),
            "udp" => nameof(VpnProtocol.OpenVpnUdp),
            "tcp" => nameof(VpnProtocol.OpenVpnTcp),
            "wireguard" => nameof(VpnProtocol.WireGuardUdp),
            "wireguard_tcp" => nameof(VpnProtocol.WireGuardTcp),
            "stealth" => nameof(VpnProtocol.WireGuardTls),
            _ => (string?)null,
        };
    }

    private string? MigrateNetShieldMode(string username)
    {
        return GetUserSetting(username, "UserNetShieldMode") switch
        {
            "1" => nameof(NetShieldMode.BlockMalwareOnly),
            "2" => nameof(NetShieldMode.BlockAdsMalwareTrackers),
            _ => nameof(NetShieldMode.BlockMalwareOnly),
        };
    }

    private string? MigrateProfilesForLaterMigration(string username)
    {
        try
        {
            List<LegacyProfile>? legacyProfiles = GetUserSetting<LegacyUserProfilesSetting>(username, "UserProfiles")?.Value?.Local;
            if (legacyProfiles is not null && legacyProfiles.Count == 0)
            {
                legacyProfiles = null;
            }
            return legacyProfiles is null ? null : _jsonSerializer.SerializeToString(legacyProfiles);
        }
        catch
        {
            return null;
        }
    }

    private void MigrateUserAuthData(string username)
    {
        string? accessToken = GetUserSetting(username, "UserAccessToken")?.Decrypt();
        string? refreshToken = GetUserSetting(username, "UserRefreshToken")?.Decrypt();
        string? uniqueSessionId = GetUserSetting(username, "UserUid")?.Decrypt();

        if (string.IsNullOrWhiteSpace(accessToken) ||
            string.IsNullOrWhiteSpace(refreshToken) ||
            string.IsNullOrWhiteSpace(uniqueSessionId))
        {
            _globalSettings.AccessToken = null;
            _globalSettings.RefreshToken = null;
            _globalSettings.UniqueSessionId = null;
        }
        else
        {
            _globalSettings.AccessToken = accessToken;
            _globalSettings.RefreshToken = refreshToken;
            _globalSettings.UniqueSessionId = uniqueSessionId;
        }
    }

    private void MigrateGlobalSettings()
    {
        bool? isAutoLaunchEnabled = IsGlobalSettingEnabled("StartOnBoot");
        if (isAutoLaunchEnabled.HasValue)
        {
            _globalSettings.IsAutoLaunchEnabled = isAutoLaunchEnabled.Value;
        }

        string? language = GetSettingValue("Language");
        if (!string.IsNullOrEmpty(language))
        {
            _globalSettings.Language = language;
        }

        bool? isBetaAccessEnabled = IsGlobalSettingEnabled("EarlyAccess");
        if (isBetaAccessEnabled.HasValue)
        {
            _globalSettings.IsBetaAccessEnabled = isBetaAccessEnabled.Value;
        }

        bool? areAutomaticUpdatesEnabled = IsGlobalSettingEnabled("IsToAutoUpdate");
        if (areAutomaticUpdatesEnabled.HasValue)
        {
            _globalSettings.AreAutomaticUpdatesEnabled = areAutomaticUpdatesEnabled.Value;
        }

        bool? isAlternativeRoutingEnabled = IsGlobalSettingEnabled("DoHEnabled");
        if (isAlternativeRoutingEnabled.HasValue)
        {
            _globalSettings.IsAlternativeRoutingEnabled = isAlternativeRoutingEnabled.Value;
        }

        MigrateKillSwitch();
        MigrateAutoLaunchMode();
    }

    private bool? IsGlobalSettingEnabled(string settingName)
    {
        return GetSettingValue(settingName)?.EqualsIgnoringCase("true");
    }

    private void MigrateKillSwitch()
    {
        string? oldKillSwitchMode = GetSettingValue("KillSwitchMode");
        if (string.IsNullOrEmpty(oldKillSwitchMode))
        {
            return;
        }

        KillSwitchMode newKillSwitchMode = oldKillSwitchMode switch
        {
            "1" => KillSwitchMode.Standard,
            "2" => KillSwitchMode.Advanced,
            _ => DefaultSettings.KillSwitchMode,
        };

        _globalSettings.IsKillSwitchEnabled = oldKillSwitchMode is "1" or "2";
        _globalSettings.KillSwitchMode = newKillSwitchMode;
    }

    private void MigrateAutoLaunchMode()
    {
        _globalSettings.AutoLaunchMode = GetSettingValue("StartMinimized") switch
        {
            "0" => AutoLaunchMode.OpenOnDesktop,
            // "1" => AutoLaunchMode.MinimizeToTaskbar (not yet supported)
            "2" => AutoLaunchMode.MinimizeToSystemTray,
            _ => DefaultSettings.AutoLaunchMode,
        };
    }
}