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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.UI.Main.FeatureIcons;

public class PortForwardingIconViewModel : FeatureIconViewModelBase,
    IEventMessageReceiver<PortForwardingPortChangedMessage>,
    IEventMessageReceiver<PortForwardingStatusChangedMessage>
{
    private readonly IPortForwardingManager _portForwardingManager;

    public override bool IsDimmed => IsFeatureEnabled 
                                  && (!ConnectionManager.IsConnected || ConnectionManager.CurrentConnectionDetails?.IsP2P != true);

    protected override bool IsFeatureEnabled => ConnectionManager.IsConnected && CurrentProfile != null
        ? CurrentProfile.Settings.IsPortForwardingEnabled
        : Settings.IsPortForwardingEnabled;

    public bool IsConnectedWithPortForwardingError => ConnectionManager.IsConnected
                                                   && IsFeatureEnabled
                                                   && (!ConnectionManager.IsP2PServerConnection() || _portForwardingManager.HasError);

    public PortForwardingIconViewModel(
        IConnectionManager connectionManager,
        ISettings settings,
        IApplicationThemeSelector themeSelector,
        IViewModelHelper viewModelHelper,
        IPortForwardingManager portForwardingManager)
        : base(connectionManager, settings, themeSelector, viewModelHelper)
    {
        _portForwardingManager = portForwardingManager;
    }

    protected override ImageSource GetImageSource()
    {
        return ResourceHelper.GetIllustration(
            IsFeatureEnabled
                ? "PortForwardingOnIllustrationSource"
                : "PortForwardingOffIllustrationSource",
            ThemeSelector.GetTheme());
    }

    protected override IEnumerable<string> GetSettingsChangedForIconUpdate()
    {
        yield return nameof(ISettings.IsPortForwardingEnabled);
    }

    public void Receive(PortForwardingPortChangedMessage message)
    {
        ExecuteOnUIThread(OnPortForwardingChange);
    }

    private void OnPortForwardingChange()
    {
        OnPropertyChanged(nameof(IsConnectedWithPortForwardingError));
    }

    public void Receive(PortForwardingStatusChangedMessage message)
    {
        ExecuteOnUIThread(OnPortForwardingChange);
    }
}