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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Navigation;

namespace ProtonVPN.Client.UI.Main;

public partial class MainPageViewModel : PageViewModelBase<IMainWindowViewNavigator, IMainViewNavigator>
{
    private const double EXPAND_SIDEBAR_WINDOW_WIDTH_THRESHOLD = 800;

    private readonly IMainWindowActivator _mainWindowActivator;

    [ObservableProperty]
    private bool _isSidebarExpanded;

    [ObservableProperty]
    private SplitViewDisplayMode _sidebarDisplayMode;

    public double WindowWidthThreshold { get; } = EXPAND_SIDEBAR_WINDOW_WIDTH_THRESHOLD;

    public MainPageViewModel(
        IMainWindowViewNavigator parentViewNavigator,
        IMainViewNavigator childViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainWindowActivator mainWindowActivator)
        : base(parentViewNavigator, childViewNavigator, localizer, logger, issueReporter)
    {
        _mainWindowActivator = mainWindowActivator;
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

    protected override void OnActivated()
    {
        base.OnActivated();

        IsSidebarExpanded = _mainWindowActivator.CurrentWindowSize.Width >= EXPAND_SIDEBAR_WINDOW_WIDTH_THRESHOLD;
        SidebarDisplayMode = SplitViewDisplayMode.CompactInline;
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        IsSidebarExpanded = false;
        SidebarDisplayMode = SplitViewDisplayMode.CompactOverlay;
    }
}