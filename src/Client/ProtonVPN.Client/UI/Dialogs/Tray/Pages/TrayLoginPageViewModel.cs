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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;

namespace ProtonVPN.Client.UI.Dialogs.Tray.Pages;

public partial class TrayLoginPageViewModel : PageViewModelBase<ITrayAppViewNavigator>
{
    private readonly IMainWindowActivator _mainWindowActivator;

    public TrayLoginPageViewModel(
        ITrayAppViewNavigator parentViewNavigator,
        IViewModelHelper viewModelHelper,
        IMainWindowActivator mainWindowActivator)
        : base(parentViewNavigator,
               viewModelHelper)
    {
        _mainWindowActivator = mainWindowActivator;
    }

    [RelayCommand]
    public void ShowApplication()
    {
        _mainWindowActivator.Activate();
    }
}