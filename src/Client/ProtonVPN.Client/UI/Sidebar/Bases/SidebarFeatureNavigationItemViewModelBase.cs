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

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Models.Activation.Custom;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Sidebar.Bases;

public abstract class SidebarFeatureNavigationItemViewModelBase : SidebarNavigationItemViewModelBase
{
    protected readonly IUpsellCarouselDialogActivator UpsellCarouselDialogActivator;

    private readonly ImageIcon _featureIcon = new();

    public override IconElement? Icon => GetFeatureIcon();

    public override string Status => IsRestricted ? string.Empty : GetFeatureStatus();

    protected abstract ModalSources UpsellModalSource { get; }

    protected SidebarFeatureNavigationItemViewModelBase(
        IMainViewNavigator mainViewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IUpsellCarouselDialogActivator upsellCarouselDialogActivator)
        : base(mainViewNavigator,
               localizationProvider,
               logger,
               issueReporter,
               settings)
    {
        UpsellCarouselDialogActivator = upsellCarouselDialogActivator;
    }

    protected abstract string GetFeatureStatus();

    protected abstract ImageSource GetFeatureIconSource();

    private ImageIcon GetFeatureIcon()
    {
        _featureIcon.Source = GetFeatureIconSource();
        return _featureIcon;
    }
}

public abstract class SidebarFeatureNavigationItemViewModelBase<TPageViewModelBase> : SidebarFeatureNavigationItemViewModelBase
    where TPageViewModelBase : PageViewModelBase<IMainViewNavigator>
{
    protected SidebarFeatureNavigationItemViewModelBase(
        IMainViewNavigator mainViewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IUpsellCarouselDialogActivator upsellCarouselDialogActivator)
        : base(mainViewNavigator,
               localizationProvider,
               logger,
               issueReporter,
               settings,
               upsellCarouselDialogActivator)
    { }

    public override async Task<bool> InvokeAsync()
    {
        if (IsRestricted)
        {
            // Clicking on the sidebar brings the main window forward. 
            // Introduce small delay to avoid the dialog to appear behind the main window.
            await Task.Delay(200);

            UpsellCarouselDialogActivator.ShowDialog(UpsellModalSource);
            return true;
        }

        return await MainViewNavigator.NavigateToAsync<TPageViewModelBase>();
    }

    public override bool IsHostFor(PageViewModelBase? page)
    {
        return IsHostFor<TPageViewModelBase>(page);
    }
}