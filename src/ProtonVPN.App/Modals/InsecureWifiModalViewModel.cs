/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Resources;
using ProtonVPN.Vpn.Connectors;

namespace ProtonVPN.Modals
{
    public class InsecureWifiModalViewModel : BaseModalViewModel
    {
        private readonly QuickConnector _quickConnector;

        public InsecureWifiModalViewModel(QuickConnector quickConnector)
        {
            _quickConnector = quickConnector;
            ConnectCommand = new RelayCommand(ConnectAction);
        }

        public ICommand ConnectCommand { get; }

        public string Name { get; set; }

        public string Message => StringResources.Format("Dialogs_InsecureWiFi_msg_Detected", Name);

        public override void BeforeOpenModal(dynamic options)
        {
            if (options?.Name == null)
            {
                return;
            }

            Name = options.Name;
        }

        private async void ConnectAction()
        {
            await _quickConnector.Connect();
        }
    }
}
