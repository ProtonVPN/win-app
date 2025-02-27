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
using ProtonVPN.Client.Settings.Contracts;
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
        if (!_userAuthenticator.IsLoggedIn)
        {
            return "The user is not logged in.";
        }

        StringBuilder stringBuilder = new();

        stringBuilder
            .AppendLine("Settings")
            .AppendLine();

        foreach (KeyValuePair<string, dynamic?> property in GetProperties())
        {
            stringBuilder.AppendLine($"{property.Key}: {Serialize(property.Value)}");
        }

        return stringBuilder.ToString();
    }

    private IEnumerable<KeyValuePair<string, dynamic?>> GetProperties()
    {
        yield return new(nameof(ISettings.Theme), _settings.Theme);
        yield return new(nameof(ISettings.Language), _settings.Language);
        yield return new(nameof(ISettings.VpnProtocol), _settings.VpnProtocol);
        yield return new(nameof(ISettings.Username), _settings.Username);
        yield return new($"{nameof(ISettings.VpnPlan)}.{nameof(ISettings.VpnPlan.Title)}", _settings.VpnPlan.Title);
        yield return new($"{nameof(ISettings.ConnectionCertificate)}.{nameof(ConnectionCertificate.RequestUtcDate)}", _settings.ConnectionCertificate?.RequestUtcDate);
        yield return new($"{nameof(ISettings.ConnectionCertificate)}.{nameof(ConnectionCertificate.RefreshUtcDate)}", _settings.ConnectionCertificate?.RefreshUtcDate);
        yield return new($"{nameof(ISettings.ConnectionCertificate)}.{nameof(ConnectionCertificate.ExpirationUtcDate)}", _settings.ConnectionCertificate?.ExpirationUtcDate);
        yield return new($"{nameof(ISettings.ChangeServerAttempts)}.{nameof(ISettings.ChangeServerAttempts.AttemptsCount)}", _settings.ChangeServerAttempts.AttemptsCount);
        yield return new($"{nameof(ISettings.ChangeServerAttempts)}.{nameof(ISettings.ChangeServerAttempts.LastAttemptUtcDate)}", _settings.ChangeServerAttempts.LastAttemptUtcDate);
        yield return new(nameof(ISettings.NatType), _settings.NatType);
        yield return new(nameof(ISettings.IsVpnAcceleratorEnabled), _settings.IsVpnAcceleratorEnabled);
        yield return new(nameof(ISettings.WindowWidth), _settings.WindowWidth);
        yield return new(nameof(ISettings.WindowHeight), _settings.WindowHeight);
        yield return new(nameof(ISettings.WindowXPosition), _settings.WindowXPosition);
        yield return new(nameof(ISettings.WindowYPosition), _settings.WindowYPosition);
        yield return new(nameof(ISettings.IsWindowMaximized), _settings.IsWindowMaximized);
        yield return new(nameof(ISettings.IsNotificationEnabled), _settings.IsNotificationEnabled);
        yield return new(nameof(ISettings.IsBetaAccessEnabled), _settings.IsBetaAccessEnabled);
        yield return new(nameof(ISettings.IsShareStatisticsEnabled), _settings.IsShareStatisticsEnabled);
        yield return new(nameof(ISettings.IsShareCrashReportsEnabled), _settings.IsShareCrashReportsEnabled);
        yield return new(nameof(ISettings.IsAlternativeRoutingEnabled), _settings.IsAlternativeRoutingEnabled);
        yield return new(nameof(ISettings.IsCustomDnsServersEnabled), _settings.IsCustomDnsServersEnabled);
        yield return new(nameof(ISettings.IsNavigationPaneOpened), _settings.IsNavigationPaneOpened);
        yield return new(nameof(ISettings.SidebarWidth), _settings.SidebarWidth);
        yield return new(nameof(ISettings.IsRecentsPaneOpened), _settings.IsRecentsPaneOpened);
        yield return new(nameof(ISettings.IsConnectionDetailsPaneOpened), _settings.IsConnectionDetailsPaneOpened);
        yield return new(nameof(ISettings.OpenVpnAdapter), _settings.OpenVpnAdapter);
        yield return new(nameof(ISettings.IsIpv6LeakProtectionEnabled), _settings.IsIpv6LeakProtectionEnabled);
        yield return new(nameof(ISettings.IsAutoConnectEnabled), _settings.IsAutoConnectEnabled);
        yield return new(nameof(ISettings.IsNetShieldEnabled), _settings.IsNetShieldEnabled);
        yield return new(nameof(ISettings.NetShieldMode), _settings.NetShieldMode);
        yield return new(nameof(ISettings.IsPortForwardingEnabled), _settings.IsPortForwardingEnabled);
        yield return new(nameof(ISettings.IsPortForwardingNotificationEnabled), _settings.IsPortForwardingNotificationEnabled);
        yield return new(nameof(ISettings.IsSplitTunnelingEnabled), _settings.IsSplitTunnelingEnabled);
        yield return new(nameof(ISettings.IsSmartReconnectEnabled), _settings.IsSmartReconnectEnabled);
        yield return new(nameof(ISettings.IsUserSettingsMigrationDone), _settings.IsUserSettingsMigrationDone);
        yield return new(nameof(ISettings.SplitTunnelingMode), _settings.SplitTunnelingMode);
        yield return new(nameof(ISettings.WasWelcomeOverlayDisplayed), _settings.WasWelcomeOverlayDisplayed);
        yield return new(nameof(ISettings.WasWelcomePlusOverlayDisplayed), _settings.WasWelcomePlusOverlayDisplayed);
        yield return new(nameof(ISettings.WasWelcomeUnlimitedOverlayDisplayed), _settings.WasWelcomeUnlimitedOverlayDisplayed);
        yield return new(nameof(ISettings.WasWelcomeB2BOverlayDisplayed), _settings.WasWelcomeB2BOverlayDisplayed);
        yield return new(nameof(ISettings.LogicalsLastModifiedDate), _settings.LogicalsLastModifiedDate);
        yield return new(nameof(ISettings.IsP2PInfoBannerDismissed), _settings.IsP2PInfoBannerDismissed);
        yield return new(nameof(ISettings.IsSecureCoreInfoBannerDismissed), _settings.IsSecureCoreInfoBannerDismissed);
        yield return new(nameof(ISettings.IsTorInfoBannerDismissed), _settings.IsTorInfoBannerDismissed);
        yield return new(nameof(ISettings.IsGatewayInfoBannerDismissed), _settings.IsGatewayInfoBannerDismissed);
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