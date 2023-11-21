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

using Microsoft.Toolkit.Uwp.Notifications;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Notifications.Contracts;

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
            .AddText($"{_localizer.Get("Settings_Features_PortForwarding_ActivePort")} {activePort}")
            .AddText(_localizer.Get("Notifications_PortForwarding_Description"))
            .AddAppLogoOverride(new Uri(Environment.CurrentDirectory + "\\Assets\\Illustrations\\large-port-forwarding-on.png"))
            .Show();
    }
}