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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.UI.Dialogs.Upsell.Bases;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Dialogs.Upsell;

public partial class UpsellCarouselShellViewModel : ShellViewModelBase<IUpsellCarouselWindowActivator, IUpsellCarouselViewNavigator>
{
    private static readonly int _firstUpsellFeatureIndex = 0;
    private static readonly int _lastUpsellFeatureIndex = Enum.GetValues<UpsellFeatureType>().Length - 1;

    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IWebAuthenticator _webAuthenticator;

    [ObservableProperty]
    private IUpsellFeaturePage? _selectedUpsellFeaturePage;

    public ObservableCollection<IUpsellFeaturePage> UpsellFeaturePages { get; }

    public override string Title => Localizer.Get("Upsell_Carousel_Title");

    public UpsellCarouselShellViewModel(
        IUpsellCarouselWindowActivator windowActivator,
        IUpsellCarouselViewNavigator childViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IUrlsBrowser urlsBrowser,
        IWebAuthenticator webAuthenticator,
        IEnumerable<IUpsellFeaturePage> upsellFeaturePages)
        : base(windowActivator,
               childViewNavigator,
               localizer,
               logger,
               issueReporter)
    {
        _urlsBrowser = urlsBrowser;
        _webAuthenticator = webAuthenticator;

        UpsellFeaturePages = new(upsellFeaturePages.OrderBy(p => p.SortIndex));
    }

    protected override void OnChildNavigation(NavigationEventArgs e)
    {
        base.OnChildNavigation(e);

        SelectedUpsellFeaturePage = ChildViewNavigator.GetCurrentPageContext() as IUpsellFeaturePage;
    }

    [RelayCommand]
    private void MoveToNextFeature()
    {
        int currentFeatureIndex = (int)(SelectedUpsellFeaturePage?.Feature ?? 0);
        int nextFeatureIndex = currentFeatureIndex + 1;
        if (nextFeatureIndex > _lastUpsellFeatureIndex)
        {
            nextFeatureIndex = _firstUpsellFeatureIndex;
        }
        UpsellFeatureType nextFeature = (UpsellFeatureType)nextFeatureIndex;

        MoveToFeature(nextFeature);
    }

    [RelayCommand]
    private void MoveToPreviousFeature()
    {
        int currentFeatureIndex = (int)(SelectedUpsellFeaturePage?.Feature ?? 0);
        int previousFeatureIndex = currentFeatureIndex - 1;
        if (previousFeatureIndex < _firstUpsellFeatureIndex)
        {
            previousFeatureIndex = _lastUpsellFeatureIndex;
        }
        UpsellFeatureType previousFeature = (UpsellFeatureType)previousFeatureIndex;

        MoveToFeature(previousFeature);
    }

    [RelayCommand]
    private async Task UpgradeAsync()
    {
        _urlsBrowser.BrowseTo(await _webAuthenticator.GetUpgradeAccountUrlAsync(WindowActivator.ModalSources));

        WindowActivator.Exit();
    }

    private void MoveToFeature(UpsellFeatureType feature)
    {
        SelectedUpsellFeaturePage = UpsellFeaturePages.First(f => f.Feature == feature);
    }

    partial void OnSelectedUpsellFeaturePageChanged(IUpsellFeaturePage? value)
    {
        if (value != null && !value.IsActive)
        {
            value.InvokeAsync();
        }
    }
}