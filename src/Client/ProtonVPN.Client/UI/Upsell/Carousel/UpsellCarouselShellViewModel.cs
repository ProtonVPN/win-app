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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.UI.Upsell.Carousel.Features.Base;
using ProtonVPN.Client.UI.Upsell.Carousel.Models;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Upsell.Carousel;

public partial class UpsellCarouselShellViewModel : ShellViewModelBase<IUpsellCarouselViewNavigator>
{
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IUrls _urls;

    [ObservableProperty]
    private UpsellFeatureViewModelBase? _selectedFeature;

    public ObservableCollection<UpsellFeatureViewModelBase> Features { get; }

    public override string Title => Localizer.Get("Upsell_Carousel_Title");

    public ModalSources OriginalModalSources { get; set; } = ModalSources.Undefined;

    public UpsellCarouselShellViewModel(
        IUpsellCarouselViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        IWebAuthenticator webAuthenticator,
        IUrls urls,
        IList<UpsellFeatureViewModelBase> features)
        : base(viewNavigator,
               localizationProvider,
               logger,
               issueReporter)
    {
        _webAuthenticator = webAuthenticator;
        _urls = urls;

        Features = new(features);
    }

    [RelayCommand]
    public void MoveToNextFeature()
    {
        int currentFeatureIndex = (int)(SelectedFeature?.Feature ?? 0);
        int nextFeatureIndex = currentFeatureIndex + 1;
        if (nextFeatureIndex > GetLastUpsellFeatureIndex())
        {
            nextFeatureIndex = 0;
        }
        UpsellFeature nextFeature = (UpsellFeature)nextFeatureIndex;

        MoveToFeature(nextFeature);
    }

    [RelayCommand]
    public void MoveToPreviousFeature()
    {
        int currentFeatureIndex = (int)(SelectedFeature?.Feature ?? 0);
        int previousFeatureIndex = currentFeatureIndex - 1;
        if (previousFeatureIndex < 0)
        {
            previousFeatureIndex = GetLastUpsellFeatureIndex();
        }
        UpsellFeature previousFeature = (UpsellFeature)previousFeatureIndex;

        MoveToFeature(previousFeature);
    }

    [RelayCommand]
    public async Task UpgradeAsync()
    {
        _urls.NavigateTo(await _webAuthenticator.GetUpgradeAccountUrlAsync(OriginalModalSources));

        ViewNavigator.CloseCurrentWindow();
    }

    public void MoveToFeature(UpsellFeature feature)
    {
        SelectedFeature = Features.First(f => f.Feature == feature);
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        InvalidateWindowTitle();
    }

    protected override void OnViewNavigatorInitialized()
    {
        base.OnViewNavigatorInitialized();

        InvalidateWindowTitle();
    }

    private int GetLastUpsellFeatureIndex()
    {
        return Enum.GetValues<UpsellFeature>().Length - 1;
    }

    private void InvalidateWindowTitle()
    {
        if (ViewNavigator.Window.AppWindow != null)
        {
            ViewNavigator.Window.Title = Title;
            ViewNavigator.Window.AppWindow.Title = Title;
        }
    }

    partial void OnSelectedFeatureChanged(UpsellFeatureViewModelBase? value)
    {
        ViewNavigator.NavigateToFeatureAsync(value?.Feature ?? UpsellFeature.WorldwideCoverage);
    }
}