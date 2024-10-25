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
using ProtonVPN.Client.Contracts.Bases.Models;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Extensions;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Servers.Contracts;

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

    public ModalSources UpsellModalSources => GroupType.GetUpsellModalSources();

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

    public string ActiveConnectionAutomationId => $"Active_connection_{AutomationName}";

    protected virtual string AutomationName => Header;

    protected ConnectionItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator)
        : base(localizer)
    {
        ServersLoader = serversLoader;
        ConnectionManager = connectionManager;
        UpsellCarouselWindowActivator = upsellCarouselWindowActivator;
    }

    public abstract IConnectionIntent GetConnectionIntent();

    public virtual void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        IsActiveConnection = MatchesActiveConnection(currentConnectionDetails);
    }

    public virtual void InvalidateIsRestricted(bool isPaidUser)
    {
        IsRestricted = !isPaidUser;
    }

    protected abstract bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails);

    [RelayCommand(CanExecute = nameof(CanToggleConnection))]
    private async Task ToggleConnectionAsync()
    {
        if (IsRestricted)
        {
            UpsellCarouselWindowActivator.Activate();
            // TODO Navigate to a specific page using the UpsellModalSource;
            return;
        }

        if (IsActiveConnection)
        {
            await ConnectionManager.DisconnectAsync();
        }
        else
        {
            await ConnectionManager.ConnectAsync(GetConnectionIntent());
        }
    }

    private bool CanToggleConnection()
    {
        return !IsUnderMaintenance
            || IsActiveConnection
            || IsRestricted;
    }
}