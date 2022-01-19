/*
 * Copyright (c) 2021 Proton Technologies AG
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Newtonsoft.Json;
using ProtonVPN.Account;
using ProtonVPN.BugReporting.Actions;
using ProtonVPN.BugReporting.FormElements;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.OS;
using ProtonVPN.Core.Settings;
using ProtonVPN.Servers;
using UserLocation = ProtonVPN.Core.User.UserLocation;

namespace ProtonVPN.BugReporting
{
    public class ReportFieldProvider : IReportFieldProvider
    {
        private readonly IUserStorage _userStorage;
        private readonly Common.Configuration.Config _config;
        private readonly ISystemState _systemState;
        private readonly IAppSettings _appSettings;

        public ReportFieldProvider(IUserStorage userStorage, Common.Configuration.Config config,
            ISystemState systemState, IAppSettings appSettings)
        {
            _config = config;
            _userStorage = userStorage;
            _systemState = systemState;
            _appSettings = appSettings;
        }

        public KeyValuePair<string, string>[] GetFields(SendReportAction message)
        {
            User user = _userStorage.User();
            UserLocation location = _userStorage.Location();
            string country = Countries.GetName(location.Country);
            string isp = location.Isp;

            return new[]
            {
                new KeyValuePair<string, string>("OS", "Windows"),
                new KeyValuePair<string, string>("OSVersion", Environment.OSVersion.ToString()),
                new KeyValuePair<string, string>("Client", "Windows app"),
                new KeyValuePair<string, string>("ClientVersion", _config.AppVersion),
                new KeyValuePair<string, string>("Title", "Windows app form"),
                new KeyValuePair<string, string>("Description", GetDescription(message)),
                new KeyValuePair<string, string>("Username", user.Username),
                new KeyValuePair<string, string>("Plan", VpnPlanHelper.GetPlanName(user.VpnPlan)),
                new KeyValuePair<string, string>("Email", GetEmail(message.FormElements)),
                new KeyValuePair<string, string>("Country", string.IsNullOrEmpty(country) ? "" : country),
                new KeyValuePair<string, string>("ISP", string.IsNullOrEmpty(isp) ? "" : isp),
                new KeyValuePair<string, string>("ClientType", "2")
            };
        }

        private string GetDescription(SendReportAction message)
        {
            StringBuilder stringBuilder = new();
            AppendCategoryToDescription(stringBuilder, message);
            AppendFormElementsToDescription(stringBuilder, message);
            AppendAdditionalInformationToDescription(stringBuilder);
            
            if (message.SendLogs)
            {
                AppendSettingsToDescription(stringBuilder);
            }

            return stringBuilder.ToString();
        }

        private void AppendCategoryToDescription(StringBuilder stringBuilder, SendReportAction message)
        {
            stringBuilder.AppendLine($"Category: {message.Category}").AppendLine();
        }

        private void AppendFormElementsToDescription(StringBuilder stringBuilder, SendReportAction message)
        {
            foreach (FormElement element in message.FormElements)
            {
                if (!element.Value.IsNullOrEmpty() && !element.IsEmailField())
                {
                    stringBuilder.AppendLine(element.SubmitLabel).AppendLine(element.Value).AppendLine();
                }
            }
        }

        private void AppendAdditionalInformationToDescription(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("Additional info")
                .AppendLine($"Pending reboot: {_systemState.PendingReboot().ToYesNoString()}")
                .AppendLine($"DeviceID: {_config.DeviceId}");
        }

        private void AppendSettingsToDescription(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine().AppendLine("Settings");
            IEnumerable<KeyValuePair<string, dynamic>> properties = GetProperties();
            foreach (KeyValuePair<string, dynamic> property in properties)
            {
                stringBuilder.AppendLine($"{property.Key}: {ConvertToReadableValue(property.Value)}");
            }
        }

        private IEnumerable<KeyValuePair<string, dynamic>> GetProperties()
        {
            yield return new(nameof(IAppSettings.ProfileChangesSyncedAt), _appSettings.ProfileChangesSyncedAt);
            yield return new(nameof(IAppSettings.OvpnProtocol), _appSettings.OvpnProtocol);
            yield return new(nameof(IAppSettings.Language), _appSettings.Language);
            yield return new(nameof(IAppSettings.AppFirstRun), _appSettings.AppFirstRun);
            yield return new(nameof(IAppSettings.ShowNotifications), _appSettings.ShowNotifications);
            yield return new(nameof(IAppSettings.AutoConnect)+"Enabled", !_appSettings.AutoConnect.IsNullOrEmpty());
            yield return new(nameof(IAppSettings.QuickConnect), _appSettings.QuickConnect);
            yield return new(nameof(IAppSettings.StartOnStartup), _appSettings.StartOnStartup);
            yield return new(nameof(IAppSettings.StartMinimized), _appSettings.StartMinimized);
            yield return new(nameof(IAppSettings.EarlyAccess), _appSettings.EarlyAccess);
            yield return new(nameof(IAppSettings.SecureCore), _appSettings.SecureCore);
            yield return new(nameof(IAppSettings.LastUpdate), _appSettings.LastUpdate);
            yield return new(nameof(IAppSettings.KillSwitchMode), _appSettings.KillSwitchMode);
            yield return new(nameof(IAppSettings.Ipv6LeakProtection), _appSettings.Ipv6LeakProtection);
            yield return new(nameof(IAppSettings.CustomDnsEnabled), _appSettings.CustomDnsEnabled);
            yield return new(nameof(IAppSettings.SidebarMode), _appSettings.SidebarMode);
            yield return new(nameof(IAppSettings.WelcomeModalShown), _appSettings.WelcomeModalShown);
            yield return new(nameof(IAppSettings.OnboardingStep), _appSettings.OnboardingStep);
            yield return new(nameof(IAppSettings.AppStartCounter), _appSettings.AppStartCounter);
            yield return new(nameof(IAppSettings.SidebarTab), _appSettings.SidebarTab);
            yield return new(nameof(IAppSettings.SplitTunnelingBlockApps), JsonConvert.SerializeObject(_appSettings.SplitTunnelingBlockApps));
            yield return new(nameof(IAppSettings.SplitTunnelingAllowApps), JsonConvert.SerializeObject(_appSettings.SplitTunnelingAllowApps));
            yield return new(nameof(IAppSettings.SplitTunnelExcludeIps), JsonConvert.SerializeObject(_appSettings.SplitTunnelExcludeIps));
            yield return new(nameof(IAppSettings.SplitTunnelIncludeIps), JsonConvert.SerializeObject(_appSettings.SplitTunnelIncludeIps));
            yield return new(nameof(IAppSettings.CustomDnsIps), JsonConvert.SerializeObject(_appSettings.CustomDnsIps));
            yield return new(nameof(IAppSettings.SplitTunnelingEnabled), _appSettings.SplitTunnelingEnabled);
            yield return new(nameof(IAppSettings.SplitTunnelMode), _appSettings.SplitTunnelMode);
            yield return new(nameof(IAppSettings.NetShieldEnabled), _appSettings.NetShieldEnabled);
            yield return new(nameof(IAppSettings.NetShieldMode), _appSettings.NetShieldMode);
            yield return new(nameof(IAppSettings.LastPrimaryApiFail), _appSettings.LastPrimaryApiFail);
            yield return new(nameof(IAppSettings.AlternativeApiBaseUrls), _appSettings.AlternativeApiBaseUrls);
            yield return new(nameof(IAppSettings.ActiveAlternativeApiBaseUrl), _appSettings.ActiveAlternativeApiBaseUrl);
            yield return new(nameof(IAppSettings.DoHEnabled), _appSettings.DoHEnabled);
            yield return new(nameof(IAppSettings.PortForwardingEnabled), _appSettings.PortForwardingEnabled);
            yield return new(nameof(IAppSettings.FeaturePortForwardingEnabled), _appSettings.FeaturePortForwardingEnabled);
            yield return new(nameof(IAppSettings.DoNotShowPortForwardingConfirmationDialog), _appSettings.DoNotShowPortForwardingConfirmationDialog);
            yield return new(nameof(IAppSettings.DoNotShowKillSwitchConfirmationDialog), _appSettings.DoNotShowKillSwitchConfirmationDialog);
            yield return new(nameof(IAppSettings.DoNotShowEnableSmartProtocolDialog), _appSettings.DoNotShowEnableSmartProtocolDialog);
            yield return new(nameof(IAppSettings.FeatureSmartProtocolWireGuardEnabled), _appSettings.FeatureSmartProtocolWireGuardEnabled);
            yield return new(nameof(IAppSettings.Announcements)+"Count", _appSettings.Announcements?.Count ?? 0);
            yield return new(nameof(IAppSettings.OpenVpnTcpPorts), JsonConvert.SerializeObject(_appSettings.OpenVpnTcpPorts));
            yield return new(nameof(IAppSettings.OpenVpnUdpPorts), JsonConvert.SerializeObject(_appSettings.OpenVpnUdpPorts));
            yield return new(nameof(IAppSettings.WireGuardPorts), JsonConvert.SerializeObject(_appSettings.WireGuardPorts));
            yield return new(nameof(IAppSettings.BlackHoleIps), JsonConvert.SerializeObject(_appSettings.BlackHoleIps));
            yield return new(nameof(IAppSettings.FeatureNetShieldEnabled), _appSettings.FeatureNetShieldEnabled);
            yield return new(nameof(IAppSettings.FeatureMaintenanceTrackerEnabled), _appSettings.FeatureMaintenanceTrackerEnabled);
            yield return new(nameof(IAppSettings.FeaturePollNotificationApiEnabled), _appSettings.FeaturePollNotificationApiEnabled);
            yield return new(nameof(IAppSettings.MaintenanceCheckInterval), _appSettings.MaintenanceCheckInterval);
            yield return new(nameof(IAppSettings.NetworkAdapterType), _appSettings.NetworkAdapterType);
            yield return new(nameof(IAppSettings.VpnAcceleratorEnabled), _appSettings.VpnAcceleratorEnabled);
            yield return new(nameof(IAppSettings.FeatureVpnAcceleratorEnabled), _appSettings.FeatureVpnAcceleratorEnabled);
            yield return new(nameof(IAppSettings.FeatureStreamingServicesLogosEnabled), _appSettings.FeatureStreamingServicesLogosEnabled);
            yield return new(nameof(IAppSettings.FeatureSmartReconnectEnabled), _appSettings.FeatureSmartReconnectEnabled);
            yield return new(nameof(IAppSettings.SmartReconnectEnabled), _appSettings.SmartReconnectEnabled);
            yield return new(nameof(IAppSettings.SmartReconnectNotificationsEnabled), _appSettings.SmartReconnectNotificationsEnabled);
            yield return new(nameof(IAppSettings.AuthenticationCertificateExpirationUtcDate), _appSettings.AuthenticationCertificateExpirationUtcDate);
            yield return new(nameof(IAppSettings.AuthenticationCertificateRefreshUtcDate), _appSettings.AuthenticationCertificateRefreshUtcDate);
            yield return new(nameof(IAppSettings.AuthenticationCertificateRequestUtcDate), _appSettings.AuthenticationCertificateRequestUtcDate);
            yield return new(nameof(IAppSettings.HardwareAccelerationEnabled), _appSettings.HardwareAccelerationEnabled);
        }

        private string ConvertToReadableValue(dynamic value)
        {
            if (value == null ||
                value is string stringValue && stringValue.IsNullOrEmpty())
            {
                value = "Not set";
            }

            if (value is StringCollection)
            {
                value = JsonConvert.SerializeObject(value);
            }

            return value is string result ? result : value.ToString();
        }

        private string GetEmail(IList<FormElement> formElements)
        {
            FormElement emailField = formElements.GetEmailField();
            return emailField != null ? emailField.Value : string.Empty;
        }
    }
}