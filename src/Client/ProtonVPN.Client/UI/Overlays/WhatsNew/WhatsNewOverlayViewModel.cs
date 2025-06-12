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

using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.UI.Overlays.WhatsNew;

public partial class WhatsNewOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly ISettings _settings;

    public bool IsSubscriptionBadgeVisible => !_settings.VpnPlan.IsPaid;

    public bool IsB2BSectionVisible => _settings.VpnPlan.IsB2B;

    public WhatsNewOverlayViewModel(
        ISettings settings,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IViewModelHelper viewModelHelper)
        : base(mainWindowOverlayActivator, viewModelHelper)
    {
        _settings = settings;
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(IsSubscriptionBadgeVisible));
            OnPropertyChanged(nameof(IsB2BSectionVisible));
        });
    }
}