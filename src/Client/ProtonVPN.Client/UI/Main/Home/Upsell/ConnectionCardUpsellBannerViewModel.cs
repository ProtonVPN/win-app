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
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.UI.Main.Home.Upsell;

public partial class ConnectionCardUpsellBannerViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<ChangeServerAttemptInvalidatedMessage>,
    IEventMessageReceiver<P2PWarningWindowClosedMessage>,
    IEventMessageReceiver<P2PTrafficDetectedMessage>
{
    private readonly IConnectionManager _connectionManager;
    private readonly IChangeServerModerator _changeServerModerator;
    private readonly ISettings _settings;
    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;
    private readonly IP2PDetectionWindowActivator _p2pDetectionWindowActivator;

    private bool _wasP2PtrafficDetected;

    private bool IsP2PWarningNotificationWithinCooldown =>
        (DateTime.UtcNow - _settings.LastP2PWarningNotificationUtcDate).TotalHours < RestrictionsHandler.NOTIFICATION_COOLDOWN_IN_HOURS;

    public bool IsP2PUpsellBannerVisible => _wasP2PtrafficDetected &&
                                            IsP2PWarningNotificationWithinCooldown &&
                                            _connectionManager.IsConnected &&
                                            !_settings.VpnPlan.IsPaid &&
                                            !_p2pDetectionWindowActivator.IsWindowVisible;

    public bool IsWrongCountryBannerVisible => !IsP2PUpsellBannerVisible &&
                                               _connectionManager.IsConnected &&
                                               !_settings.VpnPlan.IsPaid &&
                                               !_changeServerModerator.CanChangeServer();

    public bool IsBannerVisible => IsWrongCountryBannerVisible || IsP2PUpsellBannerVisible;

    public ConnectionCardUpsellBannerViewModel(
        IConnectionManager connectionManager,
        IChangeServerModerator changeServerModerator,
        ISettings settings,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IP2PDetectionWindowActivator p2pDetectionWindowActivator,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _connectionManager = connectionManager;
        _changeServerModerator = changeServerModerator;
        _settings = settings;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;
        _p2pDetectionWindowActivator = p2pDetectionWindowActivator;
    }

    public void Receive(ChangeServerAttemptInvalidatedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateBanner);
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (!_connectionManager.IsConnected)
        {
            _wasP2PtrafficDetected = false;
        }

        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateBanner);
        }
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateBanner);
        }
    }

    public void Receive(P2PWarningWindowClosedMessage message)
    {
        _wasP2PtrafficDetected = true;

        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateBanner);
        }
    }

    public void Receive(P2PTrafficDetectedMessage message)
    {
        _wasP2PtrafficDetected = true;

        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateBanner);
        }
    }

    [RelayCommand]
    public Task UpgradeAsync()
    {
        return _upsellCarouselWindowActivator.ActivateAsync(IsP2PUpsellBannerVisible
            ? UpsellFeatureType.P2P
            : UpsellFeatureType.WorldwideCoverage);
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateBanner();
    }

    private void InvalidateBanner()
    {
        OnPropertyChanged(nameof(IsBannerVisible));
        OnPropertyChanged(nameof(IsWrongCountryBannerVisible));
        OnPropertyChanged(nameof(IsP2PUpsellBannerVisible));
    }
}