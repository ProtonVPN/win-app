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
using Microsoft.UI.Xaml;
using ProtonVPN.Client.Common.UI.Gallery.Pages;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;

namespace ProtonVPN.Client.UI.Gallery;

public partial class GalleryItemViewModel : PageViewModelBase
{
    [ObservableProperty]
    private string? _pageName;

    [ObservableProperty]
    private UIElement? _pageContent;

    public GalleryItemViewModel(IMainViewNavigator viewNavigator, ILocalizationProvider localizationProvider)
            : base(viewNavigator, localizationProvider)
    { }

    public override string? Title => PageName;

    public override void OnNavigatedTo(object parameter)
    {
        PageName = parameter?.ToString();

        PageContent = PageName switch
        {
            "Colors" => new ColorsPage(),
            "Inputs" => new InputsPage(),
            "Map controls" => new MapPage(),
            "Others" => new OtherControlsPage(),
            "Fields" => new TextFieldsPage(),
            "Typography" => new TypographyPage(),
            "Custom controls" => new VpnSpecificPage(),
            _ => null
        };

        InvalidateTitle();
    }
}