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
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Core.Models;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Client.Logic.Feedback.Diagnostics.Logs;

public class UserSettingsLog : LogBase
{
    private readonly ISettings _settings;
    private readonly IUserAuthenticator _userAuthenticator;

    public UserSettingsLog(IConfiguration config, ISettings settings, IUserAuthenticator userAuthenticator)
        : base(config.DiagnosticsLogFolder, "Settings.txt")
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

        foreach(KeyValuePair<string, dynamic?> property in GetProperties())
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
        yield return new(nameof(ISettings.VpnPlanTitle), _settings.VpnPlanTitle);
        yield return new(nameof(ISettings.AuthenticationCertificateRequestUtcDate), _settings.AuthenticationCertificateRequestUtcDate);
        yield return new(nameof(ISettings.AuthenticationCertificateExpirationUtcDate), _settings.AuthenticationCertificateExpirationUtcDate);
        yield return new(nameof(ISettings.AuthenticationCertificateRefreshUtcDate), _settings.AuthenticationCertificateRefreshUtcDate);
        yield return new(nameof(ISettings.NatType), _settings.NatType);
        yield return new(nameof(ISettings.IsVpnAcceleratorEnabled), _settings.IsVpnAcceleratorEnabled);
        yield return new(nameof(ISettings.WindowWidth), _settings.WindowWidth);
        yield return new(nameof(ISettings.WindowHeight), _settings.WindowHeight);
        yield return new(nameof(ISettings.WindowXPosition), _settings.WindowXPosition);
        yield return new(nameof(ISettings.WindowYPosition), _settings.WindowYPosition);
        yield return new(nameof(ISettings.IsWindowMaximized), _settings.IsWindowMaximized);
        yield return new(nameof(ISettings.IsNotificationEnabled), _settings.IsNotificationEnabled);
        yield return new(nameof(ISettings.IsBetaAccessEnabled), _settings.IsBetaAccessEnabled);
        yield return new(nameof(ISettings.IsHardwareAccelerationEnabled), _settings.IsHardwareAccelerationEnabled);
        yield return new(nameof(ISettings.IsShareStatisticsEnabled), _settings.IsShareStatisticsEnabled);
        yield return new(nameof(ISettings.IsShareCrashReportsEnabled), _settings.IsShareCrashReportsEnabled);
        yield return new(nameof(ISettings.IsAlternativeRoutingEnabled), _settings.IsAlternativeRoutingEnabled);
        yield return new(nameof(ISettings.IsCustomDnsServersEnabled), _settings.IsCustomDnsServersEnabled);
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