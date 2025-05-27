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

using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.UI.Main.FeatureIcons;

public class VpnAcceleratorIconViewModel : FeatureIconViewModelBase
{
    protected override bool IsFeatureEnabled => Settings.VpnPlan.IsPaid && Settings.IsVpnAcceleratorEnabled;

    public VpnAcceleratorIconViewModel(
        IConnectionManager connectionManager,
        ISettings settings,
        IApplicationThemeSelector themeSelector,
        IViewModelHelper viewModelHelper)
        : base(connectionManager, settings, themeSelector, viewModelHelper)
    { }

    protected override ImageSource GetImageSource()
    {
        return ResourceHelper.GetIllustration(
            IsFeatureEnabled
                ? "VpnAcceleratorOnIllustrationSource"
                : "VpnAcceleratorOffIllustrationSource",
            ThemeSelector.GetTheme());
    }

    protected override IEnumerable<string> GetSettingsChangedForIconUpdate()
    {
        yield return nameof(ISettings.IsVpnAcceleratorEnabled);
    }
}