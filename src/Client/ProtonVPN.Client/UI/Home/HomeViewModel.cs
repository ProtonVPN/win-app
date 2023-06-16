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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.Services;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;

namespace ProtonVPN.Client.UI.Home;

public partial class HomeViewModel : NavigationPageViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailsPaneInline))]
    private bool _isDetailsPaneOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailsPaneInline))]
    private SplitViewDisplayMode _detailsPaneDisplayMode;

    public bool IsDetailsPaneInline => IsDetailsPaneOpen && (DetailsPaneDisplayMode is SplitViewDisplayMode.Inline or SplitViewDisplayMode.CompactInline);

    public override string? Title => Localizer.Get("Home_Page_Title");

    public override bool IsBackEnabled => false;

    public override IconElement Icon { get; } = new House();

    public HomeViewModel(INavigationService navigationService, ILocalizationProvider localizationProvider)
        : base(navigationService, localizationProvider)
    {
    }

    [RelayCommand]
    public void CloseDetailsPane()
    {
        IsDetailsPaneOpen = false;
    }

    [RelayCommand]
    public void OpenDetailsPane()
    {
        IsDetailsPaneOpen = true;
    }

    public void ShowConnectionDetails()
    {
        IsDetailsPaneOpen = true;
    }
}