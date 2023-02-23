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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Common;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.SettingsLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Native.Structures;
using ProtonVPN.Core.OS.Crypto;
using ProtonVPN.Core.Profiles.Cached;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Settings.Contracts;
using ProtonVPN.Core.Storage;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Settings;

namespace ProtonVPN.Core
{
    internal class AppSettings : IAppSettings, INotifyPropertyChanged, ILoggedInAware
    {
        private readonly ILogger _logger;
        private readonly ISettingsStorage _storage;
        private readonly UserSettings _userSettings;
        private readonly IConfiguration _config;
        private readonly HashSet<string> _accessedPerUserProperties = new();

        public AppSettings(ILogger logger,
            ISettingsStorage storage, 
            UserSettings userSettings, 
            IConfiguration config)
        {
            _logger = logger;
            _storage = storage;
            _userSettings = userSettings;
            _config = config;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CachedProfileDataContract Profiles
        {
            get => GetPerUser<CachedProfileDataContract>() ?? new CachedProfileDataContract();
            set => SetPerUser(value);
        }

        public IReadOnlyList<Announcement> Announcements
        {
            get => GetPerUser<IReadOnlyList<Announcement>>() ?? new List<Announcement>();
            set => SetPerUser(value);
        }

        public List<IssueCategoryResponse> ReportAnIssueFormData
        {
            get => GetPerUser<List<IssueCategoryResponse>>() ?? new List<IssueCategoryResponse>();
            set => SetPerUser(value);
        }

        public DateTime ProfileChangesSyncedAt
        {
            get => GetPerUser<DateTime>();
            set => SetPerUser(value);
        }

        public string OvpnProtocol
        {
            get => Get<string>();
            set => Set(value);
        }

        public bool AppFirstRun
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool ShowNotifications
        {
            get => Get<bool>();
            set => Set(value);
        }

        public WindowPlacement WindowPlacement
        {
            get => Get<WindowPlacement>();
            set => Set(value);
        }

        public WindowPlacement SidebarWindowPlacement
        {
            get => Get<WindowPlacement>();
            set => Set(value);
        }

        public double Width
        {
            get => Get<double>();
            set => Set(value);
        }

        public string AutoConnect
        {
            get => GetPerUser<string>();
            set => SetPerUser(value);
        }

        public string QuickConnect
        {
            get => GetPerUser<string>();
            set => SetPerUser(value);
        }

        public bool StartOnBoot
        {
            get => Get<bool>();
            set => Set(value);
        }

        public StartMinimizedMode StartMinimized
        {
            get => Get<StartMinimizedMode>();
            set => Set(value);
        }

        public bool EarlyAccess
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool SecureCore
        {
            get => GetPerUser<bool>();
            set => SetPerUser(value);
        }

        public string LastUpdate
        {
            get => Get<string>();
            set => Set(value);
        }

        public string Language
        {
            get => Get<string>();
            set => Set(value);
        }

        public KillSwitchMode KillSwitchMode
        {
            get => Get<KillSwitchMode>();
            set => Set(value);
        }

        public bool Ipv6LeakProtection
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool CustomDnsEnabled
        {
            get => GetPerUser<bool>();
            set => SetPerUser(value);
        }

        [Obsolete(
            "Use this only for checking if the user enabled/disabled the feature." +
            "Use IsNetShieldEnabled() for checking if the NetShield is/should be enabled.")]
        public bool NetShieldEnabled
        {
            get => GetPerUser<bool>();
            set => SetPerUser(value);
        }

        [Obsolete(
            "Use this only for checking if the user enabled/disabled the feature." +
            "Use IsPortForwardingEnabled() for checking if Port Forwarding is/should be enabled.")]
        public bool PortForwardingEnabled
        {
            get => GetPerUser<bool>();
            set => SetPerUser(value);
        }

        public int NetShieldMode
        {
            get => GetPerUser<int>();
            set => SetPerUser(value);
        }

        public bool SidebarMode
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool ModerateNat
        {
            get => GetPerUser<bool>();
            set => SetPerUser(value);
        }

        public bool WelcomeModalShown
        {
            get => GetPerUser<bool>();
            set => SetPerUser(value);
        }

        public bool NetShieldModalShown
        {
            get => GetPerUser<bool>();
            set => SetPerUser(value);
        }

        public int OnboardingStep
        {
            get => GetPerUser<int>();
            set => SetPerUser(value);
        }

        public string LastEventId
        {
            get => GetPerUser<string>();
            set => SetPerUser(value);
        }

        public int AppStartCounter
        {
            get => Get<int>();
            set => Set(value);
        }

        public int SidebarTab
        {
            get => Get<int>();
            set => Set(value);
        }

        public DateTime LastPrimaryApiFailDateUtc
        {
            get => Get<DateTime>();
            set => Set(value);
        }

        public StringCollection AlternativeApiBaseUrls
        {
            get => Get<StringCollection>();
            set => Set(value);
        }

        public string ActiveAlternativeApiBaseUrl
        {
            get => Get<string>();
            set => Set(value);
        }

        public bool DoHEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public SplitTunnelingApp[] SplitTunnelingBlockApps
        {
            get => Get<SplitTunnelingApp[]>() ?? new SplitTunnelingApp[0];
            set => Set(value);
        }

        public SplitTunnelingApp[] SplitTunnelingAllowApps
        {
            get => Get<SplitTunnelingApp[]>() ?? new SplitTunnelingApp[0];
            set => Set(value);
        }

        public IpContract[] SplitTunnelExcludeIps
        {
            get => Get<IpContract[]>() ?? new IpContract[0];
            set => Set(value);
        }

        public IpContract[] SplitTunnelIncludeIps
        {
            get => Get<IpContract[]>() ?? new IpContract[0];
            set => Set(value);
        }

        public IpContract[] CustomDnsIps
        {
            get => GetPerUser<IpContract[]>() ?? new IpContract[0];
            set => SetPerUser(value);
        }

        public int SettingsSelectedTabIndex
        {
            get => Get<int>();
            set => Set(value);
        }

        public bool SplitTunnelingEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public SplitTunnelMode SplitTunnelMode
        {
            get => Get<SplitTunnelMode>();
            set => Set(value);
        }

        public int[] OpenVpnTcpPorts
        {
            get => Get<int[]>() ?? _config.DefaultOpenVpnTcpPorts;
            set => Set(value);
        }

        public int[] OpenVpnUdpPorts
        {
            get => Get<int[]>() ?? _config.DefaultOpenVpnUdpPorts;
            set => Set(value);
        }

        public int[] WireGuardPorts
        {
            get => Get<int[]>() ?? _config.DefaultWireGuardPorts;
            set => Set(value);
        }

        public OpenVpnAdapter NetworkAdapterType
        {
            get => Get<OpenVpnAdapter>();
            set => Set(value);
        }

        public bool FeatureNetShieldEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool FeatureMaintenanceTrackerEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool FeaturePollNotificationApiEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool FeaturePortForwardingEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool FeatureSmartProtocolWireGuardEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool DoNotShowPortForwardingConfirmationDialog
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool PortForwardingNotificationsEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool PortForwardingInQuickSettings
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool DoNotShowKillSwitchConfirmationDialog
        {
            get => GetPerUser<bool>();
            set => SetPerUser(value);
        }
        
        public bool DoNotShowEnableSmartProtocolDialog
        {
            get => GetPerUser<bool>();
            set => SetPerUser(value);
        }

        public bool DoNotShowDiscourageSecureCoreDialog
        {
            get => GetPerUser<bool>();
            set => SetPerUser(value);
        }

        [Obsolete(
            "Use this only for checking if the user enabled/disabled the feature." +
            "Use IsVpnAcceleratorEnabled() for checking if VPN Accelerator is/should be enabled.")]
        public bool VpnAcceleratorEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool FeatureVpnAcceleratorEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool FeatureStreamingServicesLogosEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool FeaturePromoCodeEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool FeatureSmartReconnectEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool ShowNonStandardPortsToFreeUsers
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool ConnectOnAppStart
        {
            get => Get<bool>();
            set => Set(value);
        }

        [Obsolete(
            "Use this only for checking if the user enabled/disabled the feature." +
            "Use IsSmartReconnectEnabled() for checking if Smart Reconnect is/should be enabled.")]
        public bool SmartReconnectEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool SmartReconnectNotificationsEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool AllowNonStandardPorts
        {
            get => GetPerUser<bool>();
            set => SetPerUser(value);
        }

        public string AuthenticationPublicKey
        {
            get => GetPerUserDecrypted();
            set => SetPerUserEncrypted(value);
        }

        public string AuthenticationSecretKey
        {
            get => GetPerUserDecrypted();
            set => SetPerUserEncrypted(value);
        }

        public string AuthenticationCertificatePem
        {
            get => GetPerUserDecrypted();
            set => SetPerUserEncrypted(value);
        }

        public DateTimeOffset? AuthenticationCertificateExpirationUtcDate
        {
            get => GetPerUser<string>().FromJsonDateTimeOffset();
            set => SetPerUser(value.ToJsonDateTimeOffset());
        }

        public DateTimeOffset? AuthenticationCertificateRefreshUtcDate
        {
            get => GetPerUser<string>().FromJsonDateTimeOffset();
            set => SetPerUser(value.ToJsonDateTimeOffset());
        }

        public DateTimeOffset? AuthenticationCertificateRequestUtcDate
        {
            get => GetPerUser<string>().FromJsonDateTimeOffset();
            set => SetPerUser(value.ToJsonDateTimeOffset());
        }

        public string CertificationServerPublicKey
        {
            get => GetPerUserDecrypted();
            set => SetPerUserEncrypted(value);
        }

        public string AccessToken
        {
            get => GetPerUserDecrypted();
            set => SetPerUserEncrypted(value);
        }

        public string RefreshToken
        {
            get => GetPerUserDecrypted();
            set => SetPerUserEncrypted(value);
        }

        public string Uid
        {
            get => GetPerUserDecrypted();
            set => SetPerUserEncrypted(value);
        }

        public bool HardwareAccelerationEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool IsToShowRebrandingPopup
        {
            get => Get<bool>();
            set => Set(value);
        }

        public ConcurrentDictionary<string, DnsResponse> DnsCache
        {
            get => Get<ConcurrentDictionary<string, DnsResponse>>();
            set => Set(value);
        }

        public TimeSpan MaintenanceCheckInterval
        {
            get
            {
                TimeSpan value = Get<TimeSpan>();
                if (value == TimeSpan.Zero)
                {
                    value = _config.MaintenanceCheckInterval;
                }

                return value;
            }
            set => Set(value);
        }

        public void OnUserLoggedIn()
        {
            List<string> properties = _accessedPerUserProperties.ToList();
            _accessedPerUserProperties.Clear();
            OnPropertiesChanged(properties);
        }

        public string[] GetSplitTunnelApps()
        {
            string[] apps = { };

            switch (SplitTunnelMode)
            {
                case SplitTunnelMode.Block:
                    apps = GetApps(SplitTunnelingBlockApps);
                    break;
                case SplitTunnelMode.Permit:
                    apps = GetApps(SplitTunnelingAllowApps);
                    break;
            }

            return apps;
        }

        public bool IsNetShieldEnabled()
        {
            return FeatureNetShieldEnabled && NetShieldEnabled;
        }

        public bool IsPortForwardingEnabled()
        {
            return FeaturePortForwardingEnabled && PortForwardingEnabled;
        }

        public bool IsVpnAcceleratorEnabled()
        {
            return FeatureVpnAcceleratorEnabled && VpnAcceleratorEnabled;
        }

        public bool IsSmartReconnectEnabled()
        {
            return FeatureSmartReconnectEnabled && SmartReconnectEnabled;
        }

        public bool IsSmartReconnectNotificationsEnabled()
        {
            return ShowNotifications && SmartReconnectNotificationsEnabled;
        }

        public VpnProtocol GetProtocol()
        {
            string protocolStr = OvpnProtocol;
            return protocolStr.EqualsIgnoringCase("udp") ? VpnProtocol.OpenVpnUdp :
                protocolStr.EqualsIgnoringCase("tcp") ? VpnProtocol.OpenVpnTcp :
                protocolStr.EqualsIgnoringCase("wireguard") ? VpnProtocol.WireGuard :
                VpnProtocol.Smart;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private T Get<T>([CallerMemberName] string propertyName = null)
        {
            return _storage.Get<T>(propertyName);
        }

        private void Set<T>(T value, [CallerMemberName] string propertyName = null)
        {
            T oldValue = default;
            Type toType = UnwrapNullable(typeof(T));
            if (toType.IsValueType || toType == typeof(string))
            {
                oldValue = _storage.Get<T>(propertyName);
                if (EqualityComparer<T>.Default.Equals(oldValue, value))
                {
                    return;
                }
            }

            _storage.Set(propertyName, value);
            OnPropertyChanged(propertyName);
            LogChange(propertyName, oldValue, value);
        }

        private void LogChange<T>(string propertyName, T oldValue, T newValue)
        {
            string oldValueJson = JsonConvert.SerializeObject(oldValue).GetLastChars(64);
            string newValueJson = JsonConvert.SerializeObject(newValue).GetLastChars(64);
            _logger.Info<SettingsChangeLog>($"Setting '{propertyName}' " +
                $"changed from '{oldValueJson}' to '{newValueJson}'.");
        }

        private T GetPerUser<T>([CallerMemberName] string propertyName = null)
        {
            _accessedPerUserProperties.Add(propertyName);

            return _userSettings.Get<T>(propertyName);
        }

        private string GetPerUserDecrypted([CallerMemberName] string propertyName = null)
        {
            _accessedPerUserProperties.Add(propertyName);

            return _userSettings.Get<string>(propertyName)?.Decrypt();
        }

        private void SetPerUserInner<T>(T value, string propertyName)
        {
            _accessedPerUserProperties.Add(propertyName);

            T oldValue = default;
            Type toType = UnwrapNullable(typeof(T));
            if (toType.IsValueType || toType == typeof(string))
            {
                oldValue = _userSettings.Get<T>(propertyName);
                if (EqualityComparer<T>.Default.Equals(oldValue, value))
                {
                    return;
                }
            }

            _userSettings.Set(propertyName, value);
            OnPropertyChanged(propertyName);
            LogChange(propertyName, oldValue, value);
        }

        private void SetPerUser<T>(T value, [CallerMemberName] string propertyName = null)
        {
            SetPerUserInner(value, propertyName);
        }

        private void SetPerUserEncrypted(string value, [CallerMemberName] string propertyName = null)
        {
            SetPerUserInner(value?.Encrypt(), propertyName);
        }

        private Type UnwrapNullable(Type type)
        {
            return IsNullableType(type) ? Nullable.GetUnderlyingType(type) : type;
        }

        private bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private void OnPropertiesChanged(IEnumerable<string> properties)
        {
            foreach (string property in properties)
            {
                OnPropertyChanged(property);
            }
        }

        private string[] GetApps(SplitTunnelingApp[] apps)
        {
            return apps == null
                ? new string[0]
                : apps.Where(a => a.Enabled)
                      .Select(a => a.Path)
                      .Union(apps.Where(a => a.Enabled)
                      .SelectMany(a => a.AdditionalPaths ?? new string[0]))
                      .ToArray();
        }
    }
}