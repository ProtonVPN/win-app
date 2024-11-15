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
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles.Controls;

public partial class ProfileIconSelectorViewModel : ViewModelBase, IProfileIconSelector
{
    [ObservableProperty]
    private ProfileCategory _selectedCategory;

    [ObservableProperty]
    private ProfileColor _selectedColor;

    public ProfileIconSelectorViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizationProvider,
               logger,
               issueReporter)
    { }

    public ProfileCategory GetProfileCategory()
    {
        return SelectedCategory;
    }

    public ProfileColor GetProfileColor()
    {
        return SelectedColor;
    }

    public void SetProfileIcon(ProfileCategory category, ProfileColor color)
    {
        SelectedCategory = category;
        SelectedColor = color;
    }
}