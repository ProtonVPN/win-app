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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.TeachingTips;
using ProtonVPN.Client.UI.Main.Widgets.Bases;
using ProtonVPN.Client.UI.Main.Widgets.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings;

public partial class SettingsWidgetViewModel : SideWidgetViewModelBase, ISideFooterWidget
{
    private readonly ITeachingTipService _teachingTipService;

    [ObservableProperty]
    private bool _isExcludedLocationsTeachingTipOpen;

    public override int SortIndex { get; } = 1;

    public override string Header => Localizer.Get("Settings_Page_Title");

    public SettingsWidgetViewModel(    
        ITeachingTipService teachingTipService,
        IMainViewNavigator mainViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(mainViewNavigator, viewModelHelper)
    {
        _teachingTipService = teachingTipService;
    }

    public override Task<bool> InvokeAsync()
    {
        return MainViewNavigator.NavigateToSettingsViewAsync();
    }

    protected override void InvalidateIsSelected()
    {
        IsSelected = MainViewNavigator.GetCurrentPageContext() is SettingsPageViewModel;

        // Dismiss the teaching tip if the user navigates away from the home page while it's open.
        if (MainViewNavigator.GetCurrentPageContext() is not null && IsExcludedLocationsTeachingTipOpen)
        {
            DismissTeachingTip();
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        _teachingTipService.Register(
            TeachingTipKey.ExcludedLocations,
            () => IsExcludedLocationsTeachingTipOpen = true,
            () => IsExcludedLocationsTeachingTipOpen = false);
    }
    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        // Dismiss the teaching tip when the widget is unloaded while it's open (eg. user sign out).
        if (IsExcludedLocationsTeachingTipOpen)
        {
            DismissTeachingTip();
        }

        _teachingTipService.Unregister(TeachingTipKey.ExcludedLocations);
    }

    [RelayCommand]
    private void InvokeTeachingTipAction()
    {
        _teachingTipService.InvokeAction(TeachingTipKey.ExcludedLocations);
    }

    [RelayCommand]
    private void DismissTeachingTip()
    {
        _teachingTipService.InvokeDismiss(TeachingTipKey.ExcludedLocations);
    }
}