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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.ConnectionErrors;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.ConnectionErrors;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.OperatingSystems.Network.Contracts.NetworkInterfaces;

namespace ProtonVPN.Client.UI.Main.Components;

public partial class ConnectionErrorViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionErrorMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<LoggingOutMessage>
{
    private readonly IConfiguration _config;
    private readonly IConnectionErrorFactory _connectionErrorFactory;
    private readonly IApplicationIconSelector _applicationIconSelector;
    private readonly IConflictingNetworkInterfacesProvider _conflictingNetworkInterfacesProvider;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TriggerActionButtonCommand))]
    [NotifyCanExecuteChangedFor(nameof(CloseErrorCommand))]
    private bool _isConnectionErrorVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActionButtonTitle))]
    [NotifyPropertyChangedFor(nameof(ConnectionErrorSeverity))]
    [NotifyPropertyChangedFor(nameof(ConnectionErrorTitle))]
    [NotifyPropertyChangedFor(nameof(ConnectionErrorMessage))]
    [NotifyPropertyChangedFor(nameof(IsConnectionErrorVisible))]
    [NotifyCanExecuteChangedFor(nameof(TriggerActionButtonCommand))]
    private IConnectionError? _connectionError;

    private ConnectionStatus _lastStatus = ConnectionStatus.Disconnected;

    public Severity ConnectionErrorSeverity => ConnectionError?.Severity ?? Severity.None;

    public string ConnectionErrorTitle => ConnectionError?.Title ?? string.Empty;

    public string ConnectionErrorMessage => ConnectionError?.Message ?? string.Empty;

    public string ActionButtonTitle => ConnectionError?.ActionLabel ?? string.Empty;

    private readonly Lazy<Debouncer<ConnectionStatus>> _conflictingAdapterCheckerDebouncer;

    public ConnectionErrorViewModel(
        IConfiguration config,
        IConnectionErrorFactory connectionErrorFactory,
        IApplicationIconSelector applicationIconSelector,
        IViewModelHelper viewModelHelper,
        IConflictingNetworkInterfacesProvider conflictingNetworkInterfacesProvider)
        : base(viewModelHelper)
    {
        _config = config;
        _connectionErrorFactory = connectionErrorFactory;
        _applicationIconSelector = applicationIconSelector;
        _conflictingNetworkInterfacesProvider = conflictingNetworkInterfacesProvider;

        _conflictingAdapterCheckerDebouncer = new(() => new(_config.ConflictingAdapterCheckerDelay,
            input => TriggerConflictingAdapterCheckAsync(input)));
    }

    private async Task TriggerConflictingAdapterCheckAsync(ConnectionStatus connectionStatus)
    {
        if (connectionStatus is not ConnectionStatus.Connecting)
        {
            return;
        }

        IReadOnlyList<NetworkInterfaceInfo> conflictingAdapters = _conflictingNetworkInterfacesProvider.Get();
        if (conflictingAdapters.Any())
        {
            ExecuteOnUIThread(() =>
            {
                SetConflictingAdapterError(conflictingAdapters, connectionStatus);
            });
        }
    }

    public void Receive(ConnectionErrorMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            IConnectionError connectionError = _connectionErrorFactory.GetConnectionError(message.VpnError);
            if (connectionError is WireGuardAdapterInUseConnectionError or TapAdapterInUseConnectionError or UnknownConnectionError)
            {
                if (IsConnectionErrorVisible && ConnectionError is IConflictingAdapterConnectionError)
                {
                    Logger.Info<AppLog>($"Not showing {connectionError.GetType().Name} because there is already a ConflictingAdapterConnectionError being shown.");
                }
                else
                {
                    IReadOnlyList<NetworkInterfaceInfo> conflictingAdapters = _conflictingNetworkInterfacesProvider.Get();
                    if (conflictingAdapters.Any())
                    {
                        SetConflictingAdapterError(conflictingAdapters, _lastStatus);
                    }
                    else
                    {
                        SetConnectionError(connectionError);
                    }
                }
            }
            else
            {
                SetConnectionError(connectionError);
            }
        });
    }

    private void SetConnectionError(IConnectionError connectionError)
    {
        ConnectionError = connectionError;
        IsConnectionErrorVisible = !string.IsNullOrEmpty(ConnectionErrorTitle) && !string.IsNullOrEmpty(ConnectionErrorMessage);

        OnPropertyChanged(nameof(ConnectionErrorMessage));
        OnPropertyChanged(nameof(ConnectionErrorSeverity));
        _applicationIconSelector.OnConnectionErrorTriggered(ConnectionErrorSeverity);
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (_lastStatus != message.ConnectionStatus)
        {
            _conflictingAdapterCheckerDebouncer.Value.Call(message.ConnectionStatus);
        }
        _lastStatus = message.ConnectionStatus;

        ExecuteOnUIThread(() =>
        {
            if (message.ConnectionStatus == ConnectionStatus.Connecting && IsToCloseErrorOnConnecting())
            {
                CloseError();
            }
            else if (message.ConnectionStatus == ConnectionStatus.Disconnected && IsToCloseErrorOnDisconnect())
            {
                CloseError();
            }
            else if (message.ConnectionStatus == ConnectionStatus.Connected)
            {
                CloseError();
            }
            else if (IsConnectionErrorVisible && ConnectionError is IConflictingAdapterConnectionError conflictingAdapterConnectionError)
            {
                conflictingAdapterConnectionError.SetConnectionStatus(message.ConnectionStatus);
                OnPropertyChanged(nameof(ConnectionErrorSeverity));
                _applicationIconSelector.OnConnectionErrorTriggered(ConnectionErrorSeverity);
            }
        });
    }

    private void SetConflictingAdapterError(IReadOnlyList<NetworkInterfaceInfo> conflictingAdapters, ConnectionStatus connectionStatus)
    {
        IConflictingAdapterConnectionError conflictingAdapterConnectionError = _connectionErrorFactory.GetConflictingAdapterConnectionError();
        conflictingAdapterConnectionError.SetConflictingAdapters(conflictingAdapters);
        conflictingAdapterConnectionError.SetConnectionStatus(connectionStatus);
        SetConnectionError(conflictingAdapterConnectionError);
    }

    private bool IsToCloseErrorOnConnecting()
    {
        return ConnectionError?.IsToCloseErrorOnConnecting ?? true;
    }

    private bool IsToCloseErrorOnDisconnect()
    {
        return ConnectionError?.IsToCloseErrorOnDisconnect ?? false;
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
            _applicationIconSelector.OnConnectionErrorTriggered(ConnectionErrorSeverity);
        }
        else
        {
            _applicationIconSelector.OnConnectionErrorDismissed();
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(ConnectionErrorTitle));
        OnPropertyChanged(nameof(ConnectionErrorMessage));
        OnPropertyChanged(nameof(ActionButtonTitle));
    }
}