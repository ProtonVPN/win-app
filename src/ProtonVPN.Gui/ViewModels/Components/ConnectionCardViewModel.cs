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
using CommunityToolkit.Mvvm.Messaging;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Gui.Enums;
using ProtonVPN.Gui.Mappers;
using ProtonVPN.Gui.Messages;
using ProtonVPN.Gui.ViewModels.Pages;

namespace ProtonVPN.Gui.ViewModels.Components;

public partial class ConnectionCardViewModel : ObservableRecipient, IRecipient<VpnStateChangedMessage>
{
    private readonly HomeViewModel _homeViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelConnectionCommand))]
    [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowConnectionDetailsCommand))]
    private ConnectionStatus _currentConnectionStatus;

    [ObservableProperty]
    private string _header;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSubtitleVisible))]
    private string _subtitle;

    [ObservableProperty]
    private string _title;

    public ConnectionCardViewModel(HomeViewModel homeViewModel)
    {
        Messenger.RegisterAll(this);

        _header = "Last connected to";
        _title = "Switzerland";
        _subtitle = "Zurich";

        _homeViewModel = homeViewModel;
    }

    public bool IsSubtitleVisible => !string.IsNullOrEmpty(Subtitle);

    public void Receive(VpnStateChangedMessage message)
    {
        if (message?.Value is null)
        {
            return;
        }

        CurrentConnectionStatus = ConnectionStatusMapper.Map(message.Value.Status);
    }

    [RelayCommand(CanExecute = nameof(CanConnect), IncludeCancelCommand = true)]
    private async Task ConnectAsync(CancellationToken token)
    {
        try
        {
            Messenger.Send(new VpnStateChangedMessage(new VpnState(VpnStatus.Connecting)));

            await Task.Delay(TimeSpan.FromSeconds(2), token);

            Messenger.Send(new VpnStateChangedMessage(new VpnState(VpnStatus.Connected)));
        }
        catch (OperationCanceledException)
        {
            Messenger.Send(new VpnStateChangedMessage(new VpnState(VpnStatus.Disconnected)));
        }
    }

    private bool CanConnect()
    {
        return CurrentConnectionStatus == ConnectionStatus.Disconnected;
    }

    [RelayCommand(CanExecute = nameof(CanCancelConnection))]
    private void CancelConnection()
    {
        ConnectCommand.Cancel();
    }

    private bool CanCancelConnection()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connecting;
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private void Disconnect()
    {
        Messenger.Send(new VpnStateChangedMessage(new VpnState(VpnStatus.Disconnected)));
    }

    private bool CanDisconnect()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connected;
    }

    [RelayCommand(CanExecute = nameof(CanShowConnectionDetails))]
    private void ShowConnectionDetails()
    {
        _homeViewModel.ShowConnectionDetails();
    }

    private bool CanShowConnectionDetails()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connected;
    }
}