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
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Core.Bases.Models;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.Models.Connections;

public abstract partial class ConnectionItemBase : ModelBase, IConnectionItem
{
    protected readonly IServersLoader ServersLoader;
    protected readonly IConnectionManager ConnectionManager;
    protected readonly IUpsellCarouselWindowActivator UpsellCarouselWindowActivator;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleConnectionCommand))]
    [NotifyPropertyChangedFor(nameof(IsAvailable))]
    [NotifyPropertyChangedFor(nameof(ToolTip))]
    private bool _isUnderMaintenance;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleConnectionCommand))]
    [NotifyPropertyChangedFor(nameof(PrimaryActionLabel))]
    [NotifyPropertyChangedFor(nameof(PrimaryCommandAutomationId))]
    private bool _isActiveConnection;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleConnectionCommand))]
    [NotifyPropertyChangedFor(nameof(IsAvailable))]
    [NotifyPropertyChangedFor(nameof(ToolTip))]
    private bool _isRestricted;

    public bool IsAvailable => !IsRestricted && !IsUnderMaintenance;

    public abstract ConnectionGroupType GroupType { get; }

    public abstract string Header { get; }

    public virtual string Description => string.Empty;

    public bool IsDescriptionVisible { get; protected set; } = true;

    public abstract string? ToolTip { get; }

    public virtual bool IsCounted => true;

    public virtual object FirstSortProperty => Header;

    public virtual object SecondSortProperty => Description;

    public string PrimaryActionLabel => Localizer.Get(
        IsActiveConnection
            ? "Common_Actions_Disconnect"
            : "Common_Actions_Connect");

    public string PrimaryCommandAutomationId =>
        IsActiveConnection
            ? $"Disconnect_from_{AutomationName}"
            : $"Connect_to_{AutomationName}";

    public abstract string SecondaryCommandAutomationId { get; }

    public abstract VpnTriggerDimension VpnTriggerDimension { get; }

    public string ActiveConnectionAutomationId => $"Active_connection_{AutomationName}";

    protected bool IsSearchItem { get; }

    protected virtual string AutomationName => Header;

    protected ConnectionItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        bool isSearchItem)
        : base(localizer)
    {        
        ServersLoader = serversLoader;
        ConnectionManager = connectionManager;
        UpsellCarouselWindowActivator = upsellCarouselWindowActivator;
        IsSearchItem = isSearchItem;
    }

    public abstract IConnectionIntent GetConnectionIntent();

    public virtual void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        IsActiveConnection = ConnectionManager.IsConnected
                          && MatchesActiveConnection(currentConnectionDetails);
    }

    public virtual void InvalidateIsRestricted(bool isPaidUser)
    {
        IsRestricted = !isPaidUser;
    }

    protected abstract bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails);

    [RelayCommand(CanExecute = nameof(CanToggleConnection))]
    private Task ToggleConnectionAsync()
    {
        IConnectionIntent connectionIntent = GetConnectionIntent();

        if (IsRestricted)
        {
            return UpsellCarouselWindowActivator.ActivateAsync(
                connectionIntent switch
                {
                    IConnectionProfile => UpsellFeatureType.Profiles,
                    _ => connectionIntent?.Feature switch
                    {
                        SecureCoreFeatureIntent => UpsellFeatureType.SecureCore,
                        P2PFeatureIntent => UpsellFeatureType.P2P,
                        TorFeatureIntent => UpsellFeatureType.Tor,
                        _ => UpsellFeatureType.WorldwideCoverage
                    }
                }
            );
        }

        return IsActiveConnection
            ? ConnectionManager.DisconnectAsync(VpnTriggerDimension)
            : ConnectionManager.ConnectAsync(VpnTriggerDimension, connectionIntent);
    }

    private bool CanToggleConnection()
    {
        return !IsUnderMaintenance
            || IsActiveConnection
            || IsRestricted;
    }
}