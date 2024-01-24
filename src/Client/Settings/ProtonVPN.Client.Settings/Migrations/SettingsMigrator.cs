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
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Client.Settings.Migrations;

public class SettingsMigrator : ISettingsMigrator
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ISettings _settings;
    private readonly Lazy<XmlDocument> _xmlDocument;

    public SettingsMigrator(
        ILogger logger,
        IConfiguration configuration,
        IJsonSerializer jsonSerializer,
        ISettings settings)
    {
        _logger = logger;
        _configuration = configuration;
        _jsonSerializer = jsonSerializer;
        _settings = settings;

        _xmlDocument = new Lazy<XmlDocument>(() => new XmlDocument());
    }

    public void Migrate()
    {
        if (_settings.IsSettingsMigrationDone)
        {
            return;
        }

        string? legacyUserConfigFilePath = GetLegacyUserConfigFilePath();
        if (legacyUserConfigFilePath is null || !File.Exists(legacyUserConfigFilePath))
        {
            return;
        }

        try
        {
            MigrateSettingsFile(legacyUserConfigFilePath);

            if (Directory.Exists(_configuration.LegacyAppLocalData))
            {
                Directory.Delete(_configuration.LegacyAppLocalData, true);
            }
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Failed to migrate user settings.", e);
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
        foreach (string username in usernames)
        {
            MigrateUser(username);
        }

        MigrateGlobalSettings();

        _settings.IsSettingsMigrationDone = true;
    }

    private List<string> GetUsernames()
    {
        string? jsonUserAccessTokens = GetSettingValue("UserAccessToken");
        if (!string.IsNullOrEmpty(jsonUserAccessTokens))
        {
            List<LegacyUserSetting>? userAccessTokens = _jsonSerializer.Deserialize<List<LegacyUserSetting>>(jsonUserAccessTokens);
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

    private void MigrateUser(string username)
    {
        // It's important to first set the username, so that all the subsequent settings are migrated for that user
        _settings.Username = username;

        bool? isNotificationsEnabled = IsGlobalSettingEnabled("ShowNotifications");
        if (isNotificationsEnabled.HasValue)
        {
            _settings.IsNotificationEnabled = isNotificationsEnabled.Value;
        }

        bool? isVpnAcceleratorEnabled = IsGlobalSettingEnabled("VpnAcceleratorEnabled");
        if (isVpnAcceleratorEnabled.HasValue)
        {
            _settings.IsVpnAcceleratorEnabled = isVpnAcceleratorEnabled.Value;
        }

        bool? isAlternativeRoutingEnabled = IsGlobalSettingEnabled("DoHEnabled");
        if (isAlternativeRoutingEnabled.HasValue)
        {
            _settings.IsAlternativeRoutingEnabled = isAlternativeRoutingEnabled.Value;
        }

        bool? isIpv6LeakProtectionEnabled = IsGlobalSettingEnabled("Ipv6LeakProtection");
        if (isIpv6LeakProtectionEnabled.HasValue)
        {
            _settings.IsIpv6LeakProtectionEnabled = isIpv6LeakProtectionEnabled.Value;
        }

        bool? isNetShieldEnabled = IsUserSettingEnabled(username, "UserNetShieldEnabled");
        if (isNetShieldEnabled.HasValue)
        {
            _settings.IsNetShieldEnabled = isNetShieldEnabled.Value;
        }

        _settings.VpnPlanTitle = GetUserSetting(username, "UserVpnPlanName");

        MigrateUserAuthData(username);
        MigrateAuthCertificate(username);
        MigrateAutoConnect(username);
        MigrateModerateNat(username);
        MigrateCustomDns(username);
        MigratePortForwarding(username);

        MigrateKillSwitch();
        MigrateSplitTunnel();
        MigrateAutoLaunchMode();
        MigrateOpenVpnNetworkAdapter();
        MigrateVpnProtocol();

        StoreProfilesAndQuickConnectProfileForLaterMigration(username);
    }

    private bool? IsGlobalSettingEnabled(string settingName)
    {
        return GetSettingValue(settingName)?.EqualsIgnoringCase("true");
    }

    private bool? IsUserSettingEnabled(string username, string settingName)
    {
        return GetUserSetting(username, settingName)?.EqualsIgnoringCase("true");
    }

    private string? GetUserSetting(string username, string setting)
    {
        return GetUserSetting<LegacyUserSetting>(username, setting)?.Value;
    }

    private T? GetUserSetting<T>(string username, string setting) where T : LegacyUserSettingBase
    {
        string? userSetting = GetSettingValue(setting);
        if (string.IsNullOrEmpty(userSetting))
        {
            return default;
        }

        return _jsonSerializer.Deserialize<List<T>>(userSetting)?
            .Where(s => s.User != null && s.User.EqualsIgnoringCase(username))
            .FirstOrDefault();
    }

    private void StoreProfilesAndQuickConnectProfileForLaterMigration(string username)
    {
        _settings.LegacyQuickConnectProfileId = GetUserSetting(username, "UserQuickConnect");

        List<LegacyProfile>? legacyProfiles = GetUserSetting<LegacyUserProfilesSetting>(username, "UserProfiles")?.Value?.Local;
        if (legacyProfiles is not null && legacyProfiles.Count == 0)
        {
            legacyProfiles = null;
        }
        _settings.LegacyProfiles = legacyProfiles;
    }

    private void MigrateUserAuthData(string username)
    {
        _settings.AccessToken = GetUserSetting(username, "UserAccessToken")?.Decrypt();
        _settings.RefreshToken = GetUserSetting(username, "UserRefreshToken")?.Decrypt();
        _settings.UniqueSessionId = GetUserSetting(username, "UserUid")?.Decrypt();
    }

    private void MigrateAuthCertificate(string username)
    {
        _settings.AuthenticationPublicKey = GetUserSetting(username, "UserAuthenticationPublicKey")?.Decrypt();
        _settings.AuthenticationSecretKey = GetUserSetting(username, "UserAuthenticationSecretKey")?.Decrypt();
        _settings.AuthenticationCertificatePem = GetUserSetting(username, "UserAuthenticationCertificatePem")?.Decrypt();

        _settings.AuthenticationCertificateRequestUtcDate = GetDateTimeUserSetting(username, "UserAuthenticationCertificateRequestUtcDate");
        _settings.AuthenticationCertificateExpirationUtcDate = GetDateTimeUserSetting(username, "UserAuthenticationCertificateExpirationUtcDate");
        _settings.AuthenticationCertificateRefreshUtcDate = GetDateTimeUserSetting(username, "UserAuthenticationCertificateRefreshUtcDate");
    }

    private DateTimeOffset? GetDateTimeUserSetting(string username, string setting)
    {
        string? value = GetUserSetting<LegacyUserSetting>(username, setting)?.Value;
        if (!string.IsNullOrEmpty(value))
        {
            value = value.Replace("\"", "");
            if (DateTimeOffset.TryParse(value, out DateTimeOffset result))
            {
                return result;
            }
        }

        return null;
    }

    private void MigrateAutoConnect(string username)
    {
        _settings.AutoConnectMode = GetUserSetting(username, "UserQuickConnect") switch
        {
            "Fastest" => AutoConnectMode.FastestConnection,
            _ => DefaultSettings.AutoConnectMode,
        };

        bool? isAutoConnectEnabled = IsGlobalSettingEnabled("ConnectOnAppStart");
        if (isAutoConnectEnabled.HasValue)
        {
            _settings.IsAutoConnectEnabled = isAutoConnectEnabled.Value;
        }
    }

    private void MigrateModerateNat(string username)
    {
        _settings.NatType = GetUserSetting(username, "UserModerateNat") switch
        {
            "true" => NatType.Moderate,
            _ => DefaultSettings.NatType,
        };
    }

    private void MigrateCustomDns(string username)
    {
        bool? isCustomDnsServersEnabled = IsUserSettingEnabled(username, "UserCustomDnsEnabled");
        if (isCustomDnsServersEnabled.HasValue)
        {
            _settings.IsCustomDnsServersEnabled = isCustomDnsServersEnabled.Value;
        }

        LegacyUserCustomDnsSetting? customDnsIpsJson = GetUserSetting<LegacyUserCustomDnsSetting>(username, "UserCustomDnsIps");
        if (customDnsIpsJson is null || customDnsIpsJson.Value?.Count == 0)
        {
            return;
        }

        _settings.CustomDnsServersList = (from ip in customDnsIpsJson.Value
                                          where !string.IsNullOrEmpty(ip.Ip)
                                          select new CustomDnsServer(ip.Ip, ip.Enabled)).ToList();
    }

    private void MigratePortForwarding(string username)
    {
        bool? isPortForwardingNotificationEnabled = IsGlobalSettingEnabled("PortForwardingNotificationsEnabled");
        if (isPortForwardingNotificationEnabled.HasValue)
        {
            _settings.IsPortForwardingNotificationEnabled = isPortForwardingNotificationEnabled.Value;
        }

        bool? isPortForwardingEnabled = IsUserSettingEnabled(username, "UserPortForwardingEnabled");
        if (isPortForwardingEnabled.HasValue)
        {
            _settings.IsPortForwardingEnabled = isPortForwardingEnabled.Value;
        }
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

        _settings.IsKillSwitchEnabled = oldKillSwitchMode is "1" or "2";
        _settings.KillSwitchMode = newKillSwitchMode;
    }

    private void MigrateSplitTunnel()
    {
        bool? isSplitTunnelingEnabled = IsGlobalSettingEnabled("SplitTunnelingEnabled");
        if (isSplitTunnelingEnabled.HasValue)
        {
            _settings.IsSplitTunnelingEnabled = isSplitTunnelingEnabled.Value;
        }

        _settings.SplitTunnelingInverseAppsList = GetSplitTunnelingApps("SplitTunnelingAllowApps");
        _settings.SplitTunnelingStandardAppsList = GetSplitTunnelingApps("SplitTunnelingBlockApps");

        _settings.SplitTunnelingInverseIpAddressesList = GetSplitTunnelingIps("SplitTunnelIncludeIps");
        _settings.SplitTunnelingStandardIpAddressesList = GetSplitTunnelingIps("SplitTunnelExcludeIps");
    }

    private List<SplitTunnelingApp> GetSplitTunnelingApps(string oldSplitTunnelingAppListName)
    {
        string? splitTunnelingAllowAppsJson = GetSettingValue(oldSplitTunnelingAppListName);
        if (string.IsNullOrEmpty(splitTunnelingAllowAppsJson))
        {
            return [];
        }

        List<LegacySplitTunnelApp>? splitTunnelingAllowApps = _jsonSerializer.Deserialize<List<LegacySplitTunnelApp>>(splitTunnelingAllowAppsJson);
        if (splitTunnelingAllowApps is null)
        {
            return [];
        }

        return (from app in splitTunnelingAllowApps
                where !string.IsNullOrEmpty(app.Path)
                select new SplitTunnelingApp(app.Path, app.Enabled)).ToList();
    }

    private List<SplitTunnelingIpAddress> GetSplitTunnelingIps(string oldSplitTunnelingIpsListName)
    {
        string? splitTunnelingAllowIpsJson = GetSettingValue(oldSplitTunnelingIpsListName);
        if (string.IsNullOrEmpty(splitTunnelingAllowIpsJson))
        {
            return [];
        }

        List<LegacySettingIpAddress>? splitTunnelingAllowIps = _jsonSerializer.Deserialize<List<LegacySettingIpAddress>>(splitTunnelingAllowIpsJson);
        if (splitTunnelingAllowIps is null)
        {
            return [];
        }

        return (from ip in splitTunnelingAllowIps
                where !string.IsNullOrEmpty(ip.Ip)
                select new SplitTunnelingIpAddress(ip.Ip, ip.Enabled)).ToList();
    }

    private void MigrateAutoLaunchMode()
    {
        _settings.AutoLaunchMode = GetSettingValue("StartMinimized") switch
        {
            "0" => AutoLaunchMode.OpenOnDesktop,
            "1" => AutoLaunchMode.MinimizeToTaskbar,
            "2" => AutoLaunchMode.MinimizeToSystemTray,
            _ => DefaultSettings.AutoLaunchMode,
        };
    }

    private void MigrateOpenVpnNetworkAdapter()
    {
        _settings.OpenVpnAdapter = GetSettingValue("NetworkAdapterType") switch
        {
            "0" => OpenVpnAdapter.Tap,
            "1" => OpenVpnAdapter.Tun,
            _ => OpenVpnAdapter.Tun,
        };
    }

    private void MigrateVpnProtocol()
    {
        _settings.VpnProtocol = GetSettingValue("OvpnProtocol") switch
        {
            "auto" => VpnProtocol.Smart,
            "udp" => VpnProtocol.OpenVpnUdp,
            "tcp" => VpnProtocol.OpenVpnTcp,
            "wireguard" => VpnProtocol.WireGuardUdp,
            _ => DefaultSettings.VpnProtocol,
        };
    }

    private void MigrateGlobalSettings()
    {
        bool? isAutoLaunchEnabled = IsGlobalSettingEnabled("StartOnBoot");
        if (isAutoLaunchEnabled.HasValue)
        {
            _settings.IsAutoLaunchEnabled = isAutoLaunchEnabled.Value;
        }

        string? language = GetSettingValue("Language");
        if (!string.IsNullOrEmpty(language))
        {
            _settings.Language = language;
        }

        bool? isBetaAccessEnabled = IsGlobalSettingEnabled("EarlyAccess");
        if (isBetaAccessEnabled.HasValue)
        {
            _settings.IsBetaAccessEnabled = isBetaAccessEnabled.Value;
        }

        bool? areAutomaticUpdatesEnabled = IsGlobalSettingEnabled("IsToAutoUpdate");
        if (areAutomaticUpdatesEnabled.HasValue)
        {
            _settings.AreAutomaticUpdatesEnabled = areAutomaticUpdatesEnabled.Value;
        }
    }
}