﻿/*
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Common.Messages;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main;

public partial class MainPageViewModel : PageViewModelBase<IMainWindowViewNavigator, IMainViewNavigator>,
    IEventMessageReceiver<ApplicationStoppedMessage>
{
    private const double SIDEBAR_COMPACT_WIDTH = 62.0;
    private const double WIDGETBAR_MIN_WIDTH = 72.0;
    private const double WIDGETBAR_MAX_WIDTH = 90.0;

    private const double EXPAND_SIDEBAR_WINDOW_WIDTH_THRESHOLD = 885;
    private const double EXPAND_WIDGETBAR_WINDOW_WIDTH_THRESHOLD = 1000;

    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly ISettings _settings;
    private readonly ISettingsViewNavigator _settingsViewNavigator;
    private readonly IUserAuthenticator _userAuthenticator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EffectiveSidebarWidth))]
    private bool _isSidebarExpanded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EffectiveSidebarWidth))]
    private double _sidebarWidth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EffectiveSidebarWidth))]
    private SplitViewDisplayMode _sidebarDisplayMode = SplitViewDisplayMode.CompactInline;

    [ObservableProperty]
    private double _widgetBarWidth;

    public double EffectiveSidebarWidth => SidebarDisplayMode switch
    {
        SplitViewDisplayMode.Inline when !IsSidebarExpanded => 0,
        SplitViewDisplayMode.Overlay => 0,
        SplitViewDisplayMode.CompactInline when !IsSidebarExpanded => SIDEBAR_COMPACT_WIDTH,
        SplitViewDisplayMode.CompactOverlay => SIDEBAR_COMPACT_WIDTH,
        _ => SidebarWidth
    };

    public bool IsHomePageDisplayed => ChildViewNavigator.GetCurrentPageContext() is null;

    public MainPageViewModel(
        IMainWindowViewNavigator parentViewNavigator,
        IMainViewNavigator childViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainWindowActivator mainWindowActivator,
        ISettings settings,
        ISettingsViewNavigator settingsViewNavigator,
        IUserAuthenticator userAuthenticator)
        : base(parentViewNavigator, childViewNavigator, localizer, logger, issueReporter)
    {
        _mainWindowActivator = mainWindowActivator;
        _settings = settings;
        _settingsViewNavigator = settingsViewNavigator;
        _userAuthenticator = userAuthenticator;
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

    public void Receive(ApplicationStoppedMessage message)
    {
        SaveSidebarWidth();
    }

    public void OnSidebarInteractionStarted()
    {
        if (SidebarDisplayMode == SplitViewDisplayMode.CompactOverlay)
        {
            IsSidebarExpanded = true;
        }
    }

    public void OnSidebarInteractionEnded()
    {
        if (SidebarDisplayMode == SplitViewDisplayMode.CompactOverlay)
        {
            IsSidebarExpanded = false;
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateSidebarDisplayMode();
        InvalidateSidebarWidth();
        InvalidateWidgetBarWidth();

        if (_mainWindowActivator.Window != null)
        {
            _mainWindowActivator.Window.SizeChanged += OnMainWindowSizeChanged;
        }
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        SaveSidebarWidth();

        if (_mainWindowActivator.Window != null)
        {
            _mainWindowActivator.Window.SizeChanged -= OnMainWindowSizeChanged;
        }
    }

    protected override void OnChildNavigation(NavigationEventArgs e)
    {
        base.OnChildNavigation(e);

        OnPropertyChanged(nameof(IsHomePageDisplayed));
    }

    private void OnMainWindowSizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        InvalidateSidebarDisplayMode();
        InvalidateWidgetBarWidth();
    }

    private void InvalidateSidebarDisplayMode()
    {
        if (!IsActive || !_userAuthenticator.IsLoggedIn)
        {
            return;
        }

        bool isAboveThreshold = _mainWindowActivator.CurrentWindowSize.Width >= EXPAND_SIDEBAR_WINDOW_WIDTH_THRESHOLD;

        SidebarDisplayMode = isAboveThreshold
            ? SplitViewDisplayMode.CompactInline
            : SplitViewDisplayMode.CompactOverlay;
        IsSidebarExpanded = isAboveThreshold;
    }

    private void InvalidateWidgetBarWidth()
    {
        bool isAboveThreshold = _mainWindowActivator.CurrentWindowSize.Width >= EXPAND_WIDGETBAR_WINDOW_WIDTH_THRESHOLD;

        WidgetBarWidth = isAboveThreshold
            ? WIDGETBAR_MAX_WIDTH
            : WIDGETBAR_MIN_WIDTH;
    }

    private void InvalidateSidebarWidth()
    {
        SidebarWidth = _settings.SidebarWidth;
    }

    private void SaveSidebarWidth()
    {
        _settings.SidebarWidth = (int)SidebarWidth;
    }
}