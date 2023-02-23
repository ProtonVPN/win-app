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

using ProtonVPN.Common.PortForwarding;
using ProtonVPN.Core.PortForwarding;
using ProtonVPN.Core.Settings;
using ProtonVPN.Notifications;
using ProtonVPN.Translations;

namespace ProtonVPN.PortForwarding
{
    public class PortForwardingNotifier : IPortForwardingNotifier, IPortForwardingStateAware
    {
        private readonly INotificationSender _notificationSender;
        private readonly IAppSettings _appSettings;

        private int _currentExternalPort;

        public PortForwardingNotifier(INotificationSender notificationSender, IAppSettings appSettings)
        {
            _notificationSender = notificationSender;
            _appSettings = appSettings;
        }

        public void OnPortForwardingStateChanged(PortForwardingState state)
        {
            if (state?.MappedPort?.MappedPort?.ExternalPort == null)
            {
                _currentExternalPort = 0;
            }
            else if (state.MappedPort.MappedPort.ExternalPort != _currentExternalPort)
            {
                _currentExternalPort = state.MappedPort.MappedPort.ExternalPort;
                SendNotification(state.MappedPort.MappedPort.ExternalPort);
            }
        }

        private void SendNotification(int port)
        {
            if (_appSettings.PortForwardingNotificationsEnabled)
            {
                string notificationTitle =
                    $"{Translation.Get("PortForwarding_lbl_ActivePort")} {port}";
                string notificationDescription = Translation.Get("PortForwarding_lbl_Info");
                _notificationSender.Send(notificationTitle, notificationDescription);
            }
        }
    }
}
