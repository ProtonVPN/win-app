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
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Legacy.Models.Activation.Custom;
using ProtonVPN.Client.Legacy.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Legacy.UI.Connections.Profiles;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.UI.Home.ConnectionError;

public partial class ConnectionErrorViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionErrorMessage>,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<LoggingOutMessage>
{
    private readonly IUrls _urls;
    private readonly ISettings _settings;
    private readonly IReportIssueDialogActivator _reportIssueDialogActivator;
    private readonly IConnectionManager _connectionManager;
    private readonly IProfileEditor _profileEditor;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActionButtonTitle))]
    [NotifyPropertyChangedFor(nameof(ConnectionErrorMessage))]
    [NotifyPropertyChangedFor(nameof(IsConnectionErrorVisible))]
    [NotifyCanExecuteChangedFor(nameof(TriggerActionButtonCommand))]
    private VpnError _vpnError = VpnError.None;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TriggerActionButtonCommand))]
    [NotifyCanExecuteChangedFor(nameof(CloseErrorCommand))]
    private bool _isConnectionErrorVisible = false;

    public string ConnectionErrorTitle => Localizer.Get("Connection_Error_Title");

    public string ConnectionErrorMessage => Localizer.GetVpnErrorMessage(VpnError, _connectionManager.CurrentConnectionIntent, _settings.VpnPlan.IsPaid);

    public string ActionButtonTitle => Localizer.GetVpnErrorActionLabel(VpnError, _connectionManager.CurrentConnectionIntent);


    public ConnectionErrorViewModel(
        ILocalizationProvider localizationProvider,
        IUrls urls,
        ISettings settings,
        IReportIssueDialogActivator reportIssueDialogActivator,
        ILogger logger,
        IIssueReporter issueReporter,
        IConnectionManager connectionManager,
        IProfileEditor profileEditor)
        : base(localizationProvider, logger, issueReporter)
    {
        _urls = urls;
        _settings = settings;
        _reportIssueDialogActivator = reportIssueDialogActivator;
        _connectionManager = connectionManager;
        _profileEditor = profileEditor;
    }

    public void Receive(ConnectionErrorMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            VpnError = message.VpnError;
            IsConnectionErrorVisible = !string.IsNullOrEmpty(ConnectionErrorMessage);
        });
    }

    public void Receive(ConnectionStatusChanged message)
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

        switch (VpnError)
        {
            case VpnError.NoServers:
                if (_connectionManager.CurrentConnectionIntent is IConnectionProfile profile)
                {
                    await _profileEditor.EditProfileAsync(profile);
                }
                else
                {
                    goto default;
                }
                break;

            case VpnError.TlsCertificateError:
                await ReportAnIssueAsync();
                break;

            case VpnError.RpcServerUnavailable:
                _urls.NavigateTo(_urls.RpcServerProblem);
                break;

            case VpnError.NoTapAdaptersError:
            case VpnError.TapAdapterInUseError:
            case VpnError.TapRequiresUpdateError:
                _urls.NavigateTo(_urls.Troubleshooting);
                break;

            default:
                await ReportAnIssueAsync();
                break;
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

    private async Task ReportAnIssueAsync()
    {
        await _reportIssueDialogActivator.ShowDialogAsync();
    }
}