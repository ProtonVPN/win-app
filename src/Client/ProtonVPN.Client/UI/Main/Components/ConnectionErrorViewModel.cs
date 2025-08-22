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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;

namespace ProtonVPN.Client.UI.Main.Components;

public partial class ConnectionErrorViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionErrorMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<LoggingOutMessage>
{
    private readonly IConnectionErrorFactory _connectionErrorFactory;
    private readonly IApplicationIconSelector _applicationIconSelector;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TriggerActionButtonCommand))]
    [NotifyCanExecuteChangedFor(nameof(CloseErrorCommand))]
    private bool _isConnectionErrorVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActionButtonTitle))]
    [NotifyPropertyChangedFor(nameof(ConnectionErrorMessage))]
    [NotifyPropertyChangedFor(nameof(IsConnectionErrorVisible))]
    [NotifyCanExecuteChangedFor(nameof(TriggerActionButtonCommand))]
    private IConnectionError? _connectionError;

    public string ConnectionErrorMessage => ConnectionError?.Message ?? string.Empty;

    public string ActionButtonTitle => ConnectionError?.ActionLabel ?? string.Empty;

    public ConnectionErrorViewModel(
        IConnectionErrorFactory connectionErrorFactory,
        IApplicationIconSelector applicationIconSelector,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _connectionErrorFactory = connectionErrorFactory;
        _applicationIconSelector = applicationIconSelector;
    }

    public void Receive(ConnectionErrorMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            ConnectionError = _connectionErrorFactory.GetConnectionError(message.VpnError);
            IsConnectionErrorVisible = !string.IsNullOrEmpty(ConnectionErrorMessage);

            OnPropertyChanged(nameof(ConnectionErrorMessage));
        });
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            if (message.ConnectionStatus == ConnectionStatus.Connecting)
            {
                CloseError();
            }
        });
    }

    public void Receive(LoggingOutMessage message)
    {
        ExecuteOnUIThread(CloseError);
    }

    [RelayCommand(CanExecute = nameof(CanTriggerActionButton))]
    private async Task TriggerActionButtonAsync()
    {
        CloseError();

        if (ConnectionError is not null)
        {
            await ConnectionError.ExecuteActionAsync();
        }
    }

    private bool CanTriggerActionButton()
    {
        return IsConnectionErrorVisible && !string.IsNullOrEmpty(ActionButtonTitle);
    }

    [RelayCommand(CanExecute = nameof(CanCloseError))]
    private void CloseError()
    {
        IsConnectionErrorVisible = false;
    }

    private bool CanCloseError()
    {
        return IsConnectionErrorVisible;
    }
    
    partial void OnIsConnectionErrorVisibleChanged(bool value)
    {
        if (IsConnectionErrorVisible)
        {
            _applicationIconSelector.OnConnectionErrorTriggered();
        }
        else
        {
            _applicationIconSelector.OnConnectionErrorDismissed();
        }
    }
}