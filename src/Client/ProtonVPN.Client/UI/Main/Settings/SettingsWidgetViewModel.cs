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

using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.UI.Main.Widgets.Bases;
using ProtonVPN.Client.UI.Main.Widgets.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings;

public class SettingsWidgetViewModel : SideWidgetViewModelBase, ISideFooterWidget
{
    public override int SortIndex { get; } = 1;

    public override string Header => Localizer.Get("Settings_Page_Title");

    public SettingsWidgetViewModel(
        IMainViewNavigator mainViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(mainViewNavigator, viewModelHelper)
    { }

    public override Task<bool> InvokeAsync()
    {
        return MainViewNavigator.NavigateToSettingsViewAsync();
    }

    protected override void InvalidateIsSelected()
    {
        IsSelected = MainViewNavigator.GetCurrentPageContext() is SettingsPageViewModel;
    }
}