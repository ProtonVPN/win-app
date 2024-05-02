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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Common.UI.Extensions;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.UI.Home.Details;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Home;

public partial class HomeViewModel : NavigationPageViewModelBase,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<ThemeChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;
    private readonly IThemeSelector _themeSelector;
    private readonly ConnectionDetailsViewModel _connectionDetailsViewModel;

    private bool _reopenDetailsPaneAutomatically;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionDetailsPaneInline))]
    private bool _isConnectionDetailsPaneOpened;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionDetailsPaneInline))]
    private SplitViewDisplayMode _connectionDetailsPaneDisplayMode;

    [ObservableProperty]
    private double _connectionDetailsPaneWidth;

    [ObservableProperty]
    private string _activeCountryCode = string.Empty;

    public bool IsConnectionDetailsPaneInline => IsConnectionDetailsPaneOpened && ConnectionDetailsPaneDisplayMode.IsInline();

    public override string Title => Localizer.Get("Home_Page_Title");

    public override bool IsBackEnabled => false;

    public bool IsPaidUser => _settings.VpnPlan.IsPaid;

    public bool IsConnecting => _connectionManager.IsConnecting;

    public bool IsConnected => _connectionManager.IsConnected;

    public override IconElement Icon { get; } = new House();

    public ElementTheme Theme => _themeSelector.GetTheme().Theme;

    public HomeViewModel(IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IConnectionManager connectionManager,
        ISettings settings,
        IThemeSelector themeSelector,
        ILogger logger,
        IIssueReporter issueReporter,
        ConnectionDetailsViewModel connectionDetailsViewModel)
        : base(viewNavigator, localizationProvider, logger, issueReporter)
    {
        _connectionManager = connectionManager;
        _settings = settings;
        _themeSelector = themeSelector;
        _connectionDetailsViewModel = connectionDetailsViewModel;

        InvalidateActiveCountryCode();
    }

    [RelayCommand]
    private void CloseConnectionDetailsPane()
    {
        IsConnectionDetailsPaneOpened = false;
    }

    private void OpenConnectionDetailsPane()
    {
        // Opening connection details Pane, flag can be reset
        _reopenDetailsPaneAutomatically = false;

        IsConnectionDetailsPaneOpened = true;
    }

    public void ToggleConnectionDetailsPane()
    {
        if (IsConnectionDetailsPaneOpened)
        {
            CloseConnectionDetailsPane();
        }
        else
        {
            OpenConnectionDetailsPane();
        }
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(IsConnecting));
            OnPropertyChanged(nameof(IsConnected));

            switch (_connectionManager.ConnectionStatus)
            {
                case ConnectionStatus.Connected:
                case ConnectionStatus.Connecting:
                    if (_reopenDetailsPaneAutomatically &&
                    ConnectionDetailsPaneDisplayMode.IsInline() &&
                    !IsConnectionDetailsPaneOpened)
                    {
                        OpenConnectionDetailsPane();
                    }

                    if (_connectionManager.ConnectionStatus == ConnectionStatus.Connected)
                    {
                        string? countryCode = _connectionManager.CurrentConnectionDetails?.ExitCountryCode;
                        if (!string.IsNullOrEmpty(countryCode))
                        {
                            ActiveCountryCode = countryCode;
                        }
                    }

                    break;

                case ConnectionStatus.Disconnected:
                    if (IsConnectionDetailsPaneOpened)
                    {
                        // When details pane was closed due to disconnection, set flag to reopen it automatically on Connect
                        _reopenDetailsPaneAutomatically = true;

                        CloseConnectionDetailsPane();
                    }
                    InvalidateActiveCountryCode();
                    break;
            }
        });
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            if (message.PropertyName == nameof(ISettings.DeviceLocation))
            {
                InvalidateActiveCountryCode();
            }
        });
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(IsPaidUser));
        });
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(IsPaidUser));
            _reopenDetailsPaneAutomatically = _settings.IsConnectionDetailsPaneOpened;
            InvalidateActiveCountryCode();
        });
    }

    private void InvalidateActiveCountryCode()
    {
        if (_connectionManager.ConnectionStatus == ConnectionStatus.Disconnected)
        {
            ActiveCountryCode = _settings.DeviceLocation?.CountryCode ?? string.Empty;
        }
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();

        _connectionDetailsViewModel.IsActive = false;
    }

    public override void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

        _connectionDetailsViewModel.IsActive = IsConnectionDetailsPaneOpened;
    }

    partial void OnIsConnectionDetailsPaneOpenedChanged(bool value)
    {
        _connectionDetailsViewModel.IsActive = value;

        if (ConnectionDetailsPaneDisplayMode.IsInline())
        {
            _settings.IsConnectionDetailsPaneOpened = value || _reopenDetailsPaneAutomatically;
        }
    }

    public void Receive(ThemeChangedMessage message)
    {
        OnPropertyChanged(nameof(Theme));
    }
}