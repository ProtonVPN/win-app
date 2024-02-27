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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.ReportIssue;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Home.ConnectionError;

public partial class ConnectionErrorViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionErrorMessage>,
    IEventMessageReceiver<ConnectionStatusChanged>
{
    private readonly IUrls _urls;
    private readonly ISettings _settings;
    private readonly IDialogActivator _dialogActivator;
    private readonly IReportIssueViewNavigator _reportIssueViewNavigator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConnectionError))]
    private string _connectionErrorMessage = string.Empty;

    [ObservableProperty]
    private string _actionButtonTitle = string.Empty;

    private VpnError _vpnError = VpnError.None;

    public bool HasConnectionError => !string.IsNullOrEmpty(ConnectionErrorMessage);

    public ConnectionErrorViewModel(
        ILocalizationProvider localizationProvider,
        IUrls urls,
        ISettings settings,
        IDialogActivator dialogActivator,
        IReportIssueViewNavigator reportIssueViewNavigator,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizationProvider, logger, issueReporter)
    {
        _urls = urls;
        _settings = settings;
        _dialogActivator = dialogActivator;
        _reportIssueViewNavigator = reportIssueViewNavigator;
    }

    public void Receive(ConnectionErrorMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            _vpnError = message.VpnError;

            ConnectionErrorMessage = Localizer.GetVpnError(message.VpnError, _settings.IsPaid);
            ActionButtonTitle = Localizer.GetDisconnectErrorActionButtonTitle(message.VpnError);
        });
    }

    [RelayCommand]
    public async Task TriggerActionButtonAsync()
    {
        switch (_vpnError)
        {
            case VpnError.TlsCertificateError:
                await ReportAnIssueAsync();
                break;
            case VpnError.RpcServerUnavailable:
                _urls.NavigateTo(_urls.RpcServerProblemUrl);
                break;
            case VpnError.NoTapAdaptersError:
            case VpnError.TapAdapterInUseError:
            case VpnError.TapRequiresUpdateError:
                _urls.NavigateTo(_urls.TroubleShootingUrl);
                break;
            default:
                await ReportAnIssueAsync();
                break;
        }

        CloseError();
    }

    private async Task ReportAnIssueAsync()
    {
        _dialogActivator.ShowDialog<ReportIssueShellViewModel>();
        await _reportIssueViewNavigator.NavigateToCategorySelectionAsync();
    }

    [RelayCommand]
    public void CloseError()
    {
        ConnectionErrorMessage = string.Empty;
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
}