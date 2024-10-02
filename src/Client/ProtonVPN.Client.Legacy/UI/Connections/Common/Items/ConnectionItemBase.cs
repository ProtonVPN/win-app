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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Legacy.Models.Activation.Custom;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.UI.Home;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.Legacy.UI.Connections.Common.Items;

public abstract partial class ConnectionItemBase : ObservableObject
{
    protected readonly IServersLoader ServersLoader;
    protected readonly IConnectionManager ConnectionManager;
    protected readonly IMainViewNavigator MainViewNavigator;
    protected readonly IUpsellCarouselDialogActivator UpsellCarouselActivator;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleConnectionCommand))]
    [NotifyPropertyChangedFor(nameof(IsEnabled))]
    [NotifyPropertyChangedFor(nameof(ToolTip))]
    private bool _isUnderMaintenance;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleConnectionCommand))]
    [NotifyPropertyChangedFor(nameof(PrimaryActionLabel))]
    [NotifyPropertyChangedFor(nameof(PrimaryCommandAutomationId))]
    private bool _isActiveConnection;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleConnectionCommand))]
    [NotifyPropertyChangedFor(nameof(IsEnabled))]
    [NotifyPropertyChangedFor(nameof(ToolTip))]
    private bool _isRestricted;

    public ILocalizationProvider Localizer { get; }

    public bool IsEnabled => !IsRestricted && !IsUnderMaintenance;

    public string Header { get; }

    public string SearchableHeader { get; }

    public abstract string? ToolTip { get; }

    public virtual bool IsCounted => true;

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

    public abstract ModalSources UpsellModalSource { get; }

    protected virtual string AutomationName => Header;

    protected ConnectionItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IUpsellCarouselDialogActivator upsellCarouselActivator,
        string header)
    {
        Localizer = localizer;
        ServersLoader = serversLoader;
        ConnectionManager = connectionManager;
        MainViewNavigator = mainViewNavigator;
        UpsellCarouselActivator = upsellCarouselActivator;

        Header = header;
        SearchableHeader = header.RemoveDiacritics();
    }

    public abstract IConnectionIntent GetConnectionIntent();

    public abstract void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails);

    public virtual void InvalidateIsRestricted(bool isPaidUser)
    {
        IsRestricted = !isPaidUser;
    }

    public abstract void InvalidateIsUnderMaintenance();

    [RelayCommand(CanExecute = nameof(CanToggleConnection))]
    protected async Task ToggleConnectionAsync()
    {
        if (IsRestricted)
        {
            UpsellCarouselActivator.ShowDialog(UpsellModalSource);
            return;
        }

        await MainViewNavigator.NavigateToAsync<HomeViewModel>();

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
