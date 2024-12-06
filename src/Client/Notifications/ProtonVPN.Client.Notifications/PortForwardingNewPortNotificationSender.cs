/*
 * Copyright (c) 2024 Proton AG
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
using ProtonVPN.Client.Common.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Notifications.Contracts;
using ProtonVPN.Client.Notifications.Contracts.Arguments;

namespace ProtonVPN.Client.Notifications;

public class PortForwardingNewPortNotificationSender : IPortForwardingNewPortNotificationSender
{
    private readonly ILocalizationProvider _localizer;

    public PortForwardingNewPortNotificationSender(ILocalizationProvider localizationProvider)
    {
        _localizer = localizationProvider;
    }

    public void Send(int activePort)
    {
        new ToastContentBuilder()
            .AddText($"{_localizer.Get("Settings_Connection_PortForwarding_ActivePort")} {activePort}")
            .AddText(_localizer.Get("Notifications_PortForwarding_Description"))
            // No need to use current theme for this icon, it was design team's decision to use light icon in this case
            .AddAppLogoOverride(new Uri(AssetPathHelper.GetAbsoluteAssetPath("Illustrations", "Light", "port-forwarding-on.png")))
            .AddButton(_localizer.Get("Notifications_PortForwarding_CopyPortNumber"), ToastActivationType.Foreground, NotificationArguments.COPY_PORT_FORWARDING_PORT_TO_CLIPBOARD)
            .Show();
    }
}