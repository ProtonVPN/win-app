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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;

namespace ProtonVPN.Client.UI.Dialogs.Troubleshooting;

public partial class TroubleshootingShellViewModel : ShellViewModelBase<ITroubleshootingWindowActivator>
{
    private readonly IUrlsBrowser _urlsBrowser;

    public TroubleshootingShellViewModel(
        IUrlsBrowser urlsBrowser,
        ITroubleshootingWindowActivator windowActivator,
        IViewModelHelper viewModelHelper)
        : base(windowActivator, viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
    }

    [RelayCommand]
    private void OpenStatusPage()
    {
        _urlsBrowser.BrowseTo(_urlsBrowser.ProtonStatusPage);
    }
}