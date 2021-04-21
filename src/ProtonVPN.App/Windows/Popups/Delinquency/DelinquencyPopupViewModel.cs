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
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Servers.Models;

namespace ProtonVPN.Windows.Popups.Delinquency
{
    public class DelinquencyPopupViewModel : BasePopupViewModel, IDelinquencyPopupViewModel
    {
        private readonly IActiveUrls _urls;

        public bool IsReconnection { get; private set; }

        public Server FromServer { get; private set; }
        public bool IsFromServerSecureCore { get; private set; }

        public Server ToServer { get; private set; }
        public bool IsToServerSecureCore { get; private set; }

        public DelinquencyPopupViewModel(IActiveUrls urls, AppWindow appWindow)
            : base(appWindow)
        {
            _urls = urls;

            GoToBillingCommand = new RelayCommand(GoToBillingAction);
        }

        public ICommand GoToBillingCommand { get; set; }

        protected virtual void GoToBillingAction()
        {
            _urls.InvoicesUrl.Open();
            TryClose();
        }

        public void SetNoReconnectionData()
        {
            IsReconnection = false;
            FromServer = null;
            ToServer = null;
        }

        public void SetReconnectionData(Server previousServer, Server currentServer)
        {
            IsReconnection = true;

            FromServer = previousServer;
            IsFromServerSecureCore = previousServer.IsSecureCore();

            ToServer = currentServer;
            IsToServerSecureCore = currentServer.IsSecureCore();
        }
    }
}