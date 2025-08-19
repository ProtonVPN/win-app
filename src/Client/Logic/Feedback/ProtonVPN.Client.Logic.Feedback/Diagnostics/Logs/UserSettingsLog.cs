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

using System.Collections;
using System.Text;
using Newtonsoft.Json;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Client.Logic.Feedback.Diagnostics.Logs;

public class UserSettingsLog : LogBase
{
    private readonly ISettings _settings;
    private readonly IUserAuthenticator _userAuthenticator;

    public UserSettingsLog(IStaticConfiguration config, ISettings settings, IUserAuthenticator userAuthenticator)
        : base(config.DiagnosticLogsFolder, "Settings.txt")
    {
        _settings = settings;
        _userAuthenticator = userAuthenticator;
    }

    public override void Write()
    {
        File.WriteAllText(Path, GenerateContent());
    }

    private string GenerateContent()
    {
        StringBuilder stringBuilder = new();

        stringBuilder
            .AppendLine("Global Settings")
            .AppendLine();

        foreach (KeyValuePair<string, dynamic?> property in GetGlobalSettings())
        {
            stringBuilder.AppendLine($"{property.Key}: {Serialize(property.Value)}");
        }

        stringBuilder
            .AppendLine()
            .AppendLine("User Settings")
            .AppendLine();

        if (_userAuthenticator.IsLoggedIn)
        {
            foreach (KeyValuePair<string, dynamic?> property in GetUserSettings())
            {
                stringBuilder.AppendLine($"{property.Key}: {Serialize(property.Value)}");
            }
        }
        else
        {
            stringBuilder.AppendLine("The user is not logged in.");
        }

        return stringBuilder.ToString();
    }

    private IEnumerable<KeyValuePair<string, dynamic?>> GetGlobalSettings()
    {
        yield return new(nameof(IGlobalSettings.Language), _settings.Language);

        yield return new($"{nameof(IGlobalSettings.DeviceLocation)}.{nameof(DeviceLocation.CountryCode)}", _settings.DeviceLocation?.CountryCode);

        yield return new(nameof(IGlobalSettings.IsAlternativeRoutingEnabled), _settings.IsAlternativeRoutingEnabled);

        yield return new(nameof(IGlobalSettings.IsGlobalSettingsMigrationDone), _settings.IsGlobalSettingsMigrationDone);

        yield return new(nameof(IGlobalSettings.AreAutomaticUpdatesEnabled), _settings.AreAutomaticUpdatesEnabled);
        yield return new(nameof(IGlobalSettings.IsBetaAccessEnabled), _settings.IsBetaAccessEnabled);

        yield return new(nameof(IGlobalSettings.IsShareCrashReportsEnabled), _settings.IsShareCrashReportsEnabled);

        yield return new(nameof(IGlobalSettings.IsAutoLaunchEnabled), _settings.IsAutoLaunchEnabled);
        yield return new(nameof(IGlobalSettings.AutoLaunchMode), _settings.AutoLaunchMode);

        yield return new(nameof(IGlobalSettings.IsKillSwitchEnabled), _settings.IsKillSwitchEnabled);
        yield return new(nameof(IGlobalSettings.KillSwitchMode), _settings.KillSwitchMode);

        yield return new(nameof(IGlobalSettings.WireGuardConnectionTimeout), _settings.WireGuardConnectionTimeout);
    }

    private IEnumerable<KeyValuePair<string, dynamic?>> GetUserSettings()
    {
        yield return new(nameof(IUserSettings.Theme), _settings.Theme);
        yield return new(nameof(IUserSettings.VpnProtocol), _settings.VpnProtocol);
        yield return new(nameof(IUserSettings.Username), _settings.Username);
        yield return new($"{nameof(IUserSettings.VpnPlan)}.{nameof(VpnPlan.IsPaid)}", _settings.VpnPlan.IsPaid);
        yield return new($"{nameof(IUserSettings.VpnPlan)}.{nameof(VpnPlan.Title)}", _settings.VpnPlan.Title);

        yield return new($"{nameof(IUserSettings.ConnectionCertificate)}.{nameof(ConnectionCertificate.RequestUtcDate)}", _settings.ConnectionCertificate?.RequestUtcDate);
        yield return new($"{nameof(IUserSettings.ConnectionCertificate)}.{nameof(ConnectionCertificate.RefreshUtcDate)}", _settings.ConnectionCertificate?.RefreshUtcDate);
        yield return new($"{nameof(IUserSettings.ConnectionCertificate)}.{nameof(ConnectionCertificate.ExpirationUtcDate)}", _settings.ConnectionCertificate?.ExpirationUtcDate);

        yield return new($"{nameof(IUserSettings.ChangeServerAttempts)}.{nameof(ChangeServerAttempts.AttemptsCount)}", _settings.ChangeServerAttempts.AttemptsCount);
        yield return new($"{nameof(IUserSettings.ChangeServerAttempts)}.{nameof(ChangeServerAttempts.LastAttemptUtcDate)}", _settings.ChangeServerAttempts.LastAttemptUtcDate);

        yield return new(nameof(IUserSettings.IsNotificationEnabled), _settings.IsNotificationEnabled);
        yield return new(nameof(IUserSettings.OpenVpnAdapter), _settings.OpenVpnAdapter);
        yield return new(nameof(IUserSettings.IsIpv6Enabled), _settings.IsIpv6Enabled);
        yield return new(nameof(IUserSettings.IsIpv6LeakProtectionEnabled), _settings.IsIpv6LeakProtectionEnabled);
        yield return new(nameof(IUserSettings.IsSmartReconnectEnabled), _settings.IsSmartReconnectEnabled);
        yield return new(nameof(IUserSettings.NatType), _settings.NatType);
        yield return new(nameof(IUserSettings.IsVpnAcceleratorEnabled), _settings.IsVpnAcceleratorEnabled);
        yield return new(nameof(IUserSettings.LogicalsLastModifiedDate), _settings.LogicalsLastModifiedDate);

        yield return new(nameof(IUserSettings.IsUserSettingsMigrationDone), _settings.IsUserSettingsMigrationDone);

        yield return new(nameof(IUserSettings.IsShareStatisticsEnabled), _settings.IsShareStatisticsEnabled);

        yield return new(nameof(IUserSettings.IsCustomDnsServersEnabled), _settings.IsCustomDnsServersEnabled);
        yield return new(nameof(IUserSettings.CustomDnsServersList), _settings.CustomDnsServersList);

        yield return new(nameof(IUserSettings.IsAutoConnectEnabled), _settings.IsAutoConnectEnabled);

        yield return new(nameof(IUserSettings.IsNetShieldEnabled), _settings.IsNetShieldEnabled);
        yield return new(nameof(IUserSettings.NetShieldMode), _settings.NetShieldMode);

        yield return new(nameof(IUserSettings.IsPortForwardingEnabled), _settings.IsPortForwardingEnabled);
        yield return new(nameof(IUserSettings.IsPortForwardingNotificationEnabled), _settings.IsPortForwardingNotificationEnabled);

        yield return new(nameof(IUserSettings.IsSplitTunnelingEnabled), _settings.IsSplitTunnelingEnabled);
        yield return new(nameof(IUserSettings.SplitTunnelingMode), _settings.SplitTunnelingMode);
        yield return new(nameof(IUserSettings.SplitTunnelingStandardAppsList), _settings.SplitTunnelingStandardAppsList);
        yield return new(nameof(IUserSettings.SplitTunnelingStandardIpAddressesList), _settings.SplitTunnelingStandardIpAddressesList);
        yield return new(nameof(IUserSettings.SplitTunnelingInverseAppsList), _settings.SplitTunnelingInverseAppsList);
        yield return new(nameof(IUserSettings.SplitTunnelingInverseIpAddressesList), _settings.SplitTunnelingInverseIpAddressesList);

        yield return new(nameof(IUserSettings.SidebarWidth), _settings.SidebarWidth);
        yield return new(nameof(IUserSettings.IsRecentsPaneOpened), _settings.IsRecentsPaneOpened);
        yield return new(nameof(IUserSettings.IsNavigationPaneOpened), _settings.IsNavigationPaneOpened);
        yield return new(nameof(IUserSettings.IsConnectionDetailsPaneOpened), _settings.IsConnectionDetailsPaneOpened);

        yield return new(nameof(IUserSettings.WindowWidth), _settings.WindowWidth);
        yield return new(nameof(IUserSettings.WindowHeight), _settings.WindowHeight);
        yield return new(nameof(IUserSettings.WindowXPosition), _settings.WindowXPosition);
        yield return new(nameof(IUserSettings.WindowYPosition), _settings.WindowYPosition);
        yield return new(nameof(IUserSettings.IsWindowMaximized), _settings.IsWindowMaximized);

        yield return new(nameof(IUserSettings.WasWelcomeOverlayDisplayed), _settings.WasWelcomeOverlayDisplayed);
        yield return new(nameof(IUserSettings.WasWelcomePlusOverlayDisplayed), _settings.WasWelcomePlusOverlayDisplayed);
        yield return new(nameof(IUserSettings.WasWelcomeUnlimitedOverlayDisplayed), _settings.WasWelcomeUnlimitedOverlayDisplayed);
        yield return new(nameof(IUserSettings.WasWelcomeB2BOverlayDisplayed), _settings.WasWelcomeB2BOverlayDisplayed);

        yield return new(nameof(IUserSettings.IsP2PInfoBannerDismissed), _settings.IsP2PInfoBannerDismissed);
        yield return new(nameof(IUserSettings.IsSecureCoreInfoBannerDismissed), _settings.IsSecureCoreInfoBannerDismissed);
        yield return new(nameof(IUserSettings.IsTorInfoBannerDismissed), _settings.IsTorInfoBannerDismissed);
        yield return new(nameof(IUserSettings.IsGatewayInfoBannerDismissed), _settings.IsGatewayInfoBannerDismissed);

        yield return new(nameof(IUserSettings.LastSeenWhatsNewOverlayVersion), _settings.LastSeenWhatsNewOverlayVersion);
    }

    private string Serialize(dynamic? value)
    {
        if (value == null ||
            (value is string stringValue && string.IsNullOrEmpty(stringValue)))
        {
            value = "Not set";
        }

        if (value is ICollection)
        {
            value = JsonConvert.SerializeObject(value);
        }

        return value is string result
            ? result
            : value?.ToString() ?? string.Empty;
    }
}