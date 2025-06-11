/*
 * Copyright (c) 2025 Proton AG
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

using Microsoft.Toolkit.Uwp.Notifications;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Notifications.Contracts;
using ProtonVPN.Client.Notifications.Contracts.Arguments;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Notifications;

public class PortForwardingNewPortNotificationSender : NotificationSenderBase, IPortForwardingNewPortNotificationSender
{
    private readonly ILocalizationProvider _localizer;
    private readonly IConfiguration _configuration;

    public PortForwardingNewPortNotificationSender(
        ILogger logger,
        ILocalizationProvider localizationProvider,
        IConfiguration configuration)
        : base(logger)
    {
        _localizer = localizationProvider;
        _configuration = configuration;
    }

    public void Send(int activePort)
    {
        Send(new ToastContentBuilder()
            .AddText($"{_localizer.Get("Settings_Connection_PortForwarding_ActivePort")} {activePort}")
            .AddText(_localizer.Get("Notifications_PortForwarding_Description"))
            // No need to use current theme for this icon, it was design team's decision to use light icon in this case
            .AddAppLogoOverride(new Uri(Path.Combine(_configuration.AssetsFolder, "Illustrations", "Light", "port-forwarding-on.png")))
            .AddButton(_localizer.Get("Notifications_PortForwarding_CopyPortNumber"), ToastActivationType.Foreground, NotificationArguments.COPY_PORT_FORWARDING_PORT_TO_CLIPBOARD));
    }
}