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
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.UI.Account;

public partial class AccountViewModel : ViewModelBase, IEventMessageReceiver<LoggedInMessage>
{
    private readonly ISettings _settings;
    private readonly IUserAuthenticator _userAuthenticator;

    public string? Username => _settings.Username;

    public string VpnPlan => Localizer.GetVpnPlanName(_settings.VpnPlanTitle);

    public AccountViewModel(
        ILocalizationProvider localizationProvider, 
        ISettings settings, 
        IUserAuthenticator userAuthenticator)
        : base(localizationProvider)
    {
        _settings = settings;
        _userAuthenticator = userAuthenticator;
    }

    public void Receive(LoggedInMessage message)
    {
        OnPropertyChanged(nameof(Username));
        OnPropertyChanged(nameof(VpnPlan));
    }

    [RelayCommand]
    public async Task SignOutAsync()
    {
        await _userAuthenticator.LogoutAsync();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(VpnPlan));
    }
}