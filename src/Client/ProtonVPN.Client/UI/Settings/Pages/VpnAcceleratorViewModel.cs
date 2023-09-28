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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.UI.Settings.Pages;

public partial class VpnAcceleratorViewModel : SettingsPageViewModelBase
{
    private readonly IUrls _urls;

    [ObservableProperty]
    private bool _isVpnAcceleratorEnabled;

    public override string? Title => Localizer.Get("Settings_Connection_VpnAccelerator");

    public string LearnMoreUrl => _urls.VpnAcceleratorLearnMore;

    public VpnAcceleratorViewModel(IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IUrls urls)
        : base(viewNavigator, localizationProvider, settings, settingsConflictResolver)
    {
        _urls = urls;
    }

    protected override bool HasConfigurationChanged()
    {
        return Settings.IsVpnAcceleratorEnabled != IsVpnAcceleratorEnabled;
    }

    protected override void SaveSettings()
    {
        Settings.IsVpnAcceleratorEnabled = IsVpnAcceleratorEnabled;
    }

    protected override void RetrieveSettings()
    {
        IsVpnAcceleratorEnabled = Settings.IsVpnAcceleratorEnabled;
    }
}