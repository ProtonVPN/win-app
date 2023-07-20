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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;

namespace ProtonVPN.Client.Contracts.ViewModels;

public abstract partial class ShellViewModelBase : PageViewModelBase
{
    public override bool IsBackEnabled => ViewNavigator.CanGoBack;

    public PageViewModelBase? CurrentPage => ViewNavigator?.Frame?.GetPageViewModel() as PageViewModelBase;

    protected ShellViewModelBase(IViewNavigator viewNavigator, ILocalizationProvider localizationProvider)
                : base(viewNavigator, localizationProvider)
    {
        ViewNavigator.Navigated += OnNavigated;
    }

    public void InitializeViewNavigator(Window window, Frame frame)
    {
        ViewNavigator.Window = window;
        ViewNavigator.Frame = frame;
    }

    public void ResetViewNavigator()
    {
        ViewNavigator.Window = null;
        ViewNavigator.Frame = null;
    }

    protected virtual void OnNavigated(object sender, NavigationEventArgs e)
    {
        OnPropertyChanged(nameof(IsBackEnabled));
        OnPropertyChanged(nameof(CurrentPage));
    }
}