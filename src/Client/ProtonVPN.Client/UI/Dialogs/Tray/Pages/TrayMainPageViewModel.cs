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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Services.Upselling;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.UI.Dialogs.Tray.Pages;

public partial class TrayMainPageViewModel : PageViewModelBase<ITrayAppViewNavigator>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<ChangeServerAttemptInvalidatedMessage>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;
    private readonly IAccountUpgradeUrlLauncher _accountUpgradeUrlLauncher;
    private readonly IChangeServerModerator _changeServerModerator;
    private readonly IServerCountCache _serverCountCache;

    public bool IsPaidUser => _settings.VpnPlan.IsPaid;

    public bool IsUpsellBannerVisible => !IsPaidUser
                                     && (!_connectionManager.IsConnected || _changeServerModerator.CanChangeServer());

    public string UpsellBannerTagline
        => Localizer.GetFormat("Upsell_Carousel_WorldwideCoverage",
                Localizer.GetPluralFormat("Upsell_Carousel_WorldwideCoverage_Servers", _serverCountCache.GetServerCount()),
                Localizer.GetPluralFormat("Upsell_Carousel_WorldwideCoverage_Countries", _serverCountCache.GetCountryCount()));

    public TrayMainPageViewModel(
        ITrayAppViewNavigator parentViewNavigator,
        IViewModelHelper viewModelHelper,
        ISettings settings,
        IConnectionManager connectionManager,
        IAccountUpgradeUrlLauncher accountUpgradeUrlLauncher,
        IChangeServerModerator changeServerModerator,
        IServerCountCache serverCountCache)
        : base(parentViewNavigator,
               viewModelHelper)
    {
        _settings = settings;
        _connectionManager = connectionManager;
        _accountUpgradeUrlLauncher = accountUpgradeUrlLauncher;
        _changeServerModerator = changeServerModerator;
        _serverCountCache = serverCountCache;
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    public void Receive(ChangeServerAttemptInvalidatedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    public void Receive(ServerListChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateAllProperties();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(UpsellBannerTagline));
    }

    [RelayCommand]
    private async Task UpgradeAsync()
    {
        await _accountUpgradeUrlLauncher.OpenAsync(ModalSource.Tray);
    }
}