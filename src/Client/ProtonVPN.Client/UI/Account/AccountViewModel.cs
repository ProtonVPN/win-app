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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Login;

namespace ProtonVPN.Client.UI.Account;

public partial class AccountViewModel : ViewModelBase, IEventMessageReceiver<LoginSuccessMessage>
{
    private readonly ISettings _settings;
    private readonly IMainViewNavigator _viewNavigator;

    public string? Username => _settings.Username;

    public string VpnPlan => Localizer.GetVpnPlanName(_settings.VpnPlanTitle);

    public AccountViewModel(ILocalizationProvider localizationProvider, ISettings settings, IMainViewNavigator viewNavigator)
        : base(localizationProvider)
    {
        _settings = settings;
        _viewNavigator = viewNavigator;
    }

    public void Receive(LoginSuccessMessage message)
    {
        OnPropertyChanged(nameof(Username));
        OnPropertyChanged(nameof(VpnPlan));
    }

    [RelayCommand]
    public async Task SignOutAsync()
    {
        await _viewNavigator.NavigateToAsync<LoginViewModel>();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(VpnPlan));
    }
}