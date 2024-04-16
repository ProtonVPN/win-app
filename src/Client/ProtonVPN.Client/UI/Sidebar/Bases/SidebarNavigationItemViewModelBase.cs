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

using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Sidebar.Bases;

public abstract class SidebarNavigationItemViewModelBase : SidebarInteractiveItemViewModelBase
{
    protected readonly IMainViewNavigator MainViewNavigator;

    protected SidebarNavigationItemViewModelBase(
        IMainViewNavigator mainViewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings)
        : base(localizationProvider,
               logger,
               issueReporter,
               settings)
    {
        MainViewNavigator = mainViewNavigator;
    }

    protected static bool IsHostFor<TPageViewModelBase>(PageViewModelBase? page)
        where TPageViewModelBase : PageViewModelBase<IMainViewNavigator>
    {
        if (page == null)
        {
            return false;
        }

        string? pageNamespace = page.GetType().Namespace;
        string? hostNamespace = typeof(TPageViewModelBase).Namespace;

        if (string.IsNullOrEmpty(pageNamespace) || string.IsNullOrEmpty(hostNamespace))
        {
            return false;
        }

        return pageNamespace.Contains(hostNamespace);
    }

    public abstract bool IsHostFor(PageViewModelBase? page);
}

public abstract class SidebarNavigationItemViewModelBase<TPageViewModelBase> : SidebarNavigationItemViewModelBase
    where TPageViewModelBase : PageViewModelBase<IMainViewNavigator>
{
    protected SidebarNavigationItemViewModelBase(
        IMainViewNavigator mainViewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings)
        : base(mainViewNavigator,
               localizationProvider,
               logger,
               issueReporter,
               settings)
    { }

    public override async Task<bool> InvokeAsync()
    {
        return await MainViewNavigator.NavigateToAsync<TPageViewModelBase>();
    }

    public override bool IsHostFor(PageViewModelBase? page)
    {
        return IsHostFor<TPageViewModelBase>(page);
    }
}