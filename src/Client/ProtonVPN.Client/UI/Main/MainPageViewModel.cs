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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Common.Messages;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Main.Home;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main;

public partial class MainPageViewModel : PageViewModelBase<IMainWindowViewNavigator, IMainViewNavigator>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<ApplicationStoppedMessage>
{
    private const double EXPAND_SIDEBAR_WINDOW_WIDTH_THRESHOLD = 800;
    private const double EXPAND_WIDGETBAR_WINDOW_WIDTH_THRESHOLD = 1000;

    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;
    private readonly ISettingsViewNavigator _settingsViewNavigator;

    [ObservableProperty]
    private bool _isSidebarExpanded;

    [ObservableProperty]
    private SplitViewDisplayMode _sidebarDisplayMode;

    [ObservableProperty]
    private double _sidebarWidth;

    public double SidebarWindowWidthThreshold { get; } = EXPAND_SIDEBAR_WINDOW_WIDTH_THRESHOLD;

    public double WidgetsWindowWidthThreshold { get; } = EXPAND_WIDGETBAR_WINDOW_WIDTH_THRESHOLD;

    public bool IsConnected => _connectionManager.IsConnected;

    public bool IsConnecting => _connectionManager.IsConnecting;

    public bool IsDisconnected => _connectionManager.IsDisconnected;

    public bool IsHomePageDisplayed => ChildViewNavigator.GetCurrentPageContext() is HomePageViewModel;

    public MainPageViewModel(
        IMainWindowViewNavigator parentViewNavigator,
        IMainViewNavigator childViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainWindowActivator mainWindowActivator,
        IConnectionManager connectionManager,
        ISettings settings,
        ISettingsViewNavigator settingsViewNavigator)
        : base(parentViewNavigator, childViewNavigator, localizer, logger, issueReporter)
    {
        _mainWindowActivator = mainWindowActivator;
        _connectionManager = connectionManager;
        _settings = settings;
        _settingsViewNavigator = settingsViewNavigator;
    }

    public void OnSidebarInteractionStarted()
    {
        if (!IsSidebarExpanded)
        {
            SidebarDisplayMode = SplitViewDisplayMode.CompactOverlay;
            IsSidebarExpanded = true;
        }
    }

    public void OnSidebarInteractionEnded()
    {
        if (SidebarDisplayMode == SplitViewDisplayMode.CompactOverlay)
        {
            IsSidebarExpanded = false;
            SidebarDisplayMode = SplitViewDisplayMode.CompactInline;
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateCurrentConnectionStatus);
        }
    }

    public void Receive(ApplicationStoppedMessage message)
    {
        SaveSidebarWidth();
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        IsSidebarExpanded = _mainWindowActivator.CurrentWindowSize.Width >= EXPAND_SIDEBAR_WINDOW_WIDTH_THRESHOLD;
        SidebarDisplayMode = SplitViewDisplayMode.CompactInline;

        InvalidateCurrentConnectionStatus();
        InvalidateSidebarWidth();
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        IsSidebarExpanded = false;
        SidebarDisplayMode = SplitViewDisplayMode.CompactOverlay;

        SaveSidebarWidth();
    }

    protected override void OnChildNavigation(NavigationEventArgs e)
    {
        base.OnChildNavigation(e);

        OnPropertyChanged(nameof(IsHomePageDisplayed));
    }

    private void InvalidateCurrentConnectionStatus()
    {
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(IsConnecting));
        OnPropertyChanged(nameof(IsDisconnected));
    }

    private void InvalidateSidebarWidth()
    {
        SidebarWidth = _settings.SidebarWidth;
    }

    private void SaveSidebarWidth()
    {
        _settings.SidebarWidth = (int)SidebarWidth;
    }

    public async Task CloseCurrentSettingsPageAsync()
    {
        if (IsHomePageDisplayed) 
        {
            return;
        }

        if (_settingsViewNavigator.GetCurrentPageContext() is SettingsPageViewModelBase currentSettingsPage)
        {
            await currentSettingsPage.CloseAsync();
        }
    }
}