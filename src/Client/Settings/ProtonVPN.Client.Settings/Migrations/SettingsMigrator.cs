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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Migrations.Contracts;
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
    private const int RANDOM_PROFILE_TYPE = 2;
    private const string FASTEST_PROFILE_NAME = "Fastest";
    private const string RANDOM_PROFILE_NAME = "Random";

    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ISettings _settings;
    private readonly IRecentConnectionsProvider _recentConnectionsProvider;
    private readonly IServersLoader _serversLoader;
    private readonly IServersUpdater _serversUpdater;
    private readonly Lazy<XmlDocument> _xmlDocument;

    public SettingsMigrator(
        ILogger logger,
        IConfiguration configuration,
        IJsonSerializer jsonSerializer,
        ISettings settings,
        IRecentConnectionsProvider recentConnectionsProvider,
        IServersLoader serversLoader,
        IServersUpdater serversUpdater)
    {
        _logger = logger;
        _configuration = configuration;
        _jsonSerializer = jsonSerializer;
        _settings = settings;
        _recentConnectionsProvider = recentConnectionsProvider;
        _serversLoader = serversLoader;
        _serversUpdater = serversUpdater;

        _xmlDocument = new Lazy<XmlDocument>(() => new XmlDocument());
    }

    public async Task MigrateSettingsAsync()
    {
        if (_settings.IsSettingsMigrationDone || !File.Exists(_configuration.LegacyUserConfigFilePath))
        {
            return;
        }

        try
        {
            await MigrateSettingsFileAsync(_configuration.LegacyUserConfigFilePath);

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

    private async Task MigrateSettingsFileAsync(string filePath)
    {
        _xmlDocument.Value.Load(filePath);

        List<string> usernames = GetUsernames();
        foreach (string username in usernames)
        {
            await MigrateUserAsync(username);
        }

        MigrateGlobalSettings();

        _settings.IsSettingsMigrationDone = true;
    }

    private List<string> GetUsernames()
    {
        string? setting = GetSettingValue("UserAccessToken");
        if (!string.IsNullOrEmpty(setting))
        {
            List<LegacyUserSetting>? legacyUserSettings = _jsonSerializer.Deserialize<List<LegacyUserSetting>>(setting);
            if (legacyUserSettings is not null)
            {
                List<string> list = legacyUserSettings
                    .Where(us => !string.IsNullOrEmpty(us.User))
                    .Select(us => us.User!.ToLower())
                    .ToList();

                string? loggedInUsername = GetSettingValue("Username")?.Decrypt().ToLower();
                if (string.IsNullOrEmpty(loggedInUsername))
                {
                    return list;
                }

                // Make sure the currently logged-in user is migrated the last
                list = list.Where(u => u != loggedInUsername).ToList();
                list.Add(loggedInUsername);

                return list;
            }
        }

        return [];
    }

    private string? GetSettingValue(string name)
    {
        string xPath = $"/configuration/userSettings/ProtonVPN.Properties.Settings/setting[@name='{name}']/value";
        return _xmlDocument.Value.SelectSingleNode(xPath)?.InnerText;
    }

    private async Task MigrateUserAsync(string username)
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

        await MigrateProfilesAsync(username, GetUserSetting(username, "UserQuickConnect"));
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

    private async Task MigrateProfilesAsync(string username, string? quickConnectProfile)
    {
        LegacyUserProfilesSetting? legacyUserProfilesSetting = GetUserSetting<LegacyUserProfilesSetting>(username, "UserProfiles");
        if (legacyUserProfilesSetting?.Value?.Local is null)
        {
            return;
        }

        Lazy<Task<List<Server>>> servers = new(GetServersAsync);
        List<IConnectionIntent> connectionIntents = await GetConnectionIntentsAsync(legacyUserProfilesSetting.Value.Local, servers);

        IConnectionIntent? recentConnectionIntent = null;
        if (!string.IsNullOrEmpty(quickConnectProfile) && quickConnectProfile != FASTEST_PROFILE_NAME &&
            quickConnectProfile != RANDOM_PROFILE_NAME)
        {
            LegacyProfile? profile = legacyUserProfilesSetting.Value.Local.FirstOrDefault(p => p.Id == quickConnectProfile);
            if (profile is not null)
            {
                recentConnectionIntent = await GetConnectionIntentAsync(profile, servers);
            }
        }

        _recentConnectionsProvider.SaveRecentConnections(connectionIntents, recentConnectionIntent);
    }

    private async Task<List<Server>> GetServersAsync()
    {
        await _serversUpdater.UpdateAsync();
        return _serversLoader.GetServers().ToList();
    }

    private async Task<List<IConnectionIntent>> GetConnectionIntentsAsync(List<LegacyProfile> profiles, Lazy<Task<List<Server>>> servers)
    {
        List<IConnectionIntent> connectionIntents = [];

        foreach (LegacyProfile profile in profiles)
        {
            if (profile.ProfileType == RANDOM_PROFILE_TYPE)
            {
                continue;
            }

            IConnectionIntent connectionIntent = await GetConnectionIntentAsync(profile, servers);
            connectionIntents.Add(connectionIntent);
        }

        return connectionIntents;
    }

    private async Task<IConnectionIntent> GetConnectionIntentAsync(LegacyProfile profile, Lazy<Task<List<Server>>> servers)
    {
        ILocationIntent? locationIntent = null;
        IFeatureIntent? featureIntent = null;

        if (string.IsNullOrEmpty(profile.ServerId))
        {
            if (!string.IsNullOrEmpty(profile.CountryCode))
            {
                locationIntent = new CountryLocationIntent(profile.CountryCode);
            }
            else if (!string.IsNullOrEmpty(profile.GatewayName))
            {
                locationIntent = new GatewayLocationIntent(profile.GatewayName);
            }

            if (profile.Features.IsSupported(ServerFeatures.SecureCore))
            {
                featureIntent = new SecureCoreFeatureIntent();
            }
        }
        else
        {
            Server? server = (await servers.Value).FirstOrDefault(s => s.Id == profile.ServerId);
            if (server is not null)
            {
                if (string.IsNullOrEmpty(server.GatewayName))
                {
                    locationIntent = new ServerLocationIntent(profile.ServerId, server.Name, server.ExitCountry,
                        server.City);
                }
                else
                {
                    locationIntent = new GatewayServerLocationIntent(profile.ServerId, server.Name, server.ExitCountry,
                        server.GatewayName);
                }

                if (profile.Features.IsSupported(ServerFeatures.SecureCore))
                {
                    featureIntent = new SecureCoreFeatureIntent(server.EntryCountry);
                }
            }
        }

        if (profile.Features.IsSupported(ServerFeatures.P2P))
        {
            featureIntent = new P2PFeatureIntent();
        }

        if (profile.Features.IsSupported(ServerFeatures.Tor))
        {
            featureIntent = new TorFeatureIntent();
        }

        if (profile.Features.IsSupported(ServerFeatures.B2B))
        {
            featureIntent = new B2BFeatureIntent();
        }

        return new ConnectionIntent(locationIntent ?? new CountryLocationIntent(), featureIntent);
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

        bool? isAutomaticUpdatesEnabled = IsGlobalSettingEnabled("IsToAutoUpdate");
        if (isAutomaticUpdatesEnabled.HasValue)
        {
            _settings.AreAutomaticUpdatesEnabled = isAutomaticUpdatesEnabled.Value;
        }
    }
}