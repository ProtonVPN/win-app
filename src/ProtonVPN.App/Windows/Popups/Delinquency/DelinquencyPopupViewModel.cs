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

using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Servers.Reconnections;
using ProtonVPN.Sidebar;

namespace ProtonVPN.Windows.Popups.Delinquency
{
    public class DelinquencyPopupViewModel : BasePopupViewModel, IDelinquencyPopupViewModel
    {
        private readonly IActiveUrls _urls;
        private readonly Lazy<ConnectionStatusViewModel> _connectionStatusViewModel;

        public ReconnectionData ReconnectionData { get; private set; }

        public DelinquencyPopupViewModel(IActiveUrls urls,
            Lazy<ConnectionStatusViewModel> connectionStatusViewModel,
            AppWindow appWindow)
            : base(appWindow)
        {
            _urls = urls;
            _connectionStatusViewModel = connectionStatusViewModel;

            GoToBillingCommand = new RelayCommand(GoToBillingAction);
        }

        protected override void OnViewAttached(object view, object context)
        {
            CloseVpnAcceleratorReconnectionPopup();
            base.OnViewAttached(view, context);
        }

        private void CloseVpnAcceleratorReconnectionPopup()
        {
            _connectionStatusViewModel.Value.CloseVpnAcceleratorReconnectionPopupAction();
        }

        protected override void OnViewReady(object view)
        {
            CloseVpnAcceleratorReconnectionPopup();
            base.OnViewReady(view);
        }

        protected override void OnViewLoaded(object view)
        {
            CloseVpnAcceleratorReconnectionPopup();
            base.OnViewLoaded(view);
        }

        public ICommand GoToBillingCommand { get; set; }

        protected virtual void GoToBillingAction()
        {
            _urls.InvoicesUrl.Open();
            TryClose();
        }

        public void SetNoReconnectionData()
        {
            ReconnectionData = new ReconnectionData();
        }

        public void SetReconnectionData(Server previousServer, Server currentServer)
        {
            ReconnectionData = new ReconnectionData(previousServer, currentServer);
        }
    }
}