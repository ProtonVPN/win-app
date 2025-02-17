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

using Microsoft.UI.Xaml;
using ProtonVPN.Client.Core.Bases;

namespace ProtonVPN.Client.UI.Main.Profiles.Components;

public sealed partial class ProfileOptionsSelectorView : IContextAware
{
    public ProfileOptionsSelectorViewModel ViewModel { get; }

    public ProfileOptionsSelectorView()
    {
        ViewModel = App.GetService<ProfileOptionsSelectorViewModel>();

        InitializeComponent();
    }

    public event EventHandler? OptionsExpanded;

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnConnectAndGoExpanded(object sender, EventArgs e)
    {
        OptionsExpanded?.Invoke(this, EventArgs.Empty);
    }
}