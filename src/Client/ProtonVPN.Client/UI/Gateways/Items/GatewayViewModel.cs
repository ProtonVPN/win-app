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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Countries;
using ProtonVPN.Client.UI.Dialogs.Overlays;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Gateways.Items;

public partial class GatewayViewModel : LocationViewModelBase, IComparable
{
    private readonly IOverlayActivator _overlayActivator;

    public string GatewayName { get; init; } = string.Empty;

    public List<B2BServerViewModel> Servers { get; init; } = new();

    public bool IsUnderMaintenance { get; init; }

    public string ConnectButtonAutomationId => $"Connect_to_{GatewayName}";
    public string ShowServersButtonAutomationId => $"Show_servers_{GatewayName}";
    public string ActiveConnectionAutomationId => $"Active_connection_{GatewayName}";

    public override bool IsActiveConnection 
        => ConnectionDetails is not null 
        && ConnectionDetails.IsGateway
        && ConnectionDetails.GatewayName == GatewayName;

    protected override ConnectionIntent ConnectionIntent => new(new GatewayLocationIntent(GatewayName), new B2BFeatureIntent());

    protected override ModalSources UpsellModalSources => ModalSources.Undefined;

    public GatewayViewModel(
        ILocalizationProvider localizationProvider,
        IMainViewNavigator mainViewNavigator,
        IConnectionManager connectionManager,
        IOverlayActivator overlayActivator,
        ILogger logger,
        IIssueReporter issueReporter,
        IWebAuthenticator webAuthenticator,
        ISettings settings,
        IUrls urls) :
        base(
            localizationProvider,
            mainViewNavigator,
            connectionManager,
            logger,
            issueReporter,
            webAuthenticator,
            settings,
            urls)
    {
        _overlayActivator = overlayActivator;
    }

    [RelayCommand]
    public async Task ShowServerLoadOverlayAsync()
    {
        await _overlayActivator.ShowOverlayAsync<ServerLoadOverlayViewModel>();
    }

    public int CompareTo(object? obj)
    {
        if (obj is not GatewayViewModel gateway)
        {
            return 0;
        }

        if (string.IsNullOrEmpty(GatewayName) && !string.IsNullOrEmpty(gateway.GatewayName))
        {
            return -1;
        }

        if (string.IsNullOrEmpty(gateway.GatewayName) && !string.IsNullOrEmpty(GatewayName))
        {
            return 1;
        }

        return string.Compare(GatewayName, gateway.GatewayName, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (ReferenceEquals(obj, null))
        {
            return false;
        }

        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(GatewayViewModel left, GatewayViewModel right)
    {
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator !=(GatewayViewModel left, GatewayViewModel right)
    {
        return !(left == right);
    }

    public static bool operator <(GatewayViewModel left, GatewayViewModel right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(GatewayViewModel left, GatewayViewModel right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(GatewayViewModel left, GatewayViewModel right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(GatewayViewModel left, GatewayViewModel right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
}
