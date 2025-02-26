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

using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.UI.Main.Profiles.Contracts;

namespace ProtonVPN.Client.UI.Main.Profiles.Components;

public partial class ProfileIconSelectorViewModel : ViewModelBase, IProfileIconSelector
{
    private IProfileIcon _originalProfileIcon = ProfileIcon.Default;

    [ObservableProperty]
    private ProfileCategory _selectedCategory;

    [ObservableProperty]
    private ProfileColor _selectedColor;

    public ProfileIconSelectorViewModel(
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    { }

    public ProfileCategory GetProfileCategory()
    {
        return SelectedCategory;
    }

    public IProfileIcon GetProfileIcon()
    {
        return new ProfileIcon()
        {
            Category = SelectedCategory,
            Color = SelectedColor
        };
    }

    public void SetProfileIcon(IProfileIcon icon)
    {
        _originalProfileIcon = icon ?? ProfileIcon.Default;

        SelectedCategory = _originalProfileIcon.Category;
        SelectedColor = _originalProfileIcon.Color;
    }

    public bool HasChanged()
    {
        return _originalProfileIcon.Category != SelectedCategory
            || _originalProfileIcon.Color != SelectedColor;
    }

    public bool IsReconnectionRequired()
    {
        return false;
    }
}