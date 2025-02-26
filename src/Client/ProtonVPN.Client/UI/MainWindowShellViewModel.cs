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

using ProtonVPN.Client.Common.Messages;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;

namespace ProtonVPN.Client.UI;

public class MainWindowShellViewModel : ShellViewModelBase<IMainWindowActivator, IMainWindowViewNavigator>
{
    private readonly IEventMessageSender _eventMessageSender;

    public MainWindowShellViewModel(
        IMainWindowActivator windowActivator,
        IMainWindowViewNavigator childViewNavigator,
        IEventMessageSender eventMessageSender,
        IViewModelHelper viewModelHelper)
        : base(windowActivator, childViewNavigator, viewModelHelper)
    {
        _eventMessageSender = eventMessageSender;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        _eventMessageSender.Send<ApplicationStartedMessage>();
    }
}