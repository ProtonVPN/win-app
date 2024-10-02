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
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Helpers;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.UI.Main.Features.Bases;
using ProtonVPN.Client.UI.Settings.Pages.Entities;

namespace ProtonVPN.Client.UI.Main.Features.NetShield;

public partial class NetShieldPageViewModel : FeaturePageViewModelBase
{
    private readonly IUrls _urls;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NetShieldFeatureIconSource))]
    private bool _isNetShieldEnabled;

    public override string Header => Localizer.Get("Settings_Features_NetShield");

    public ImageSource NetShieldFeatureIconSource => GetFeatureIconSource(IsNetShieldEnabled);

    public string LearnMoreUrl => _urls.NetShieldLearnMore;

    public NetShieldPageViewModel(
        IMainViewNavigator parentViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IUrls urls,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager
    )
        : base(parentViewNavigator, localizer, logger, issueReporter, mainWindowOverlayActivator, settings,
            settingsConflictResolver, connectionManager, ConnectionFeature.NetShield)
    {
        _urls = urls;
    }

    public static ImageSource GetFeatureIconSource(bool isEnabled)
    {
        return isEnabled
            ? ResourceHelper.GetIllustration("NetShieldOnIllustrationSource")
            : ResourceHelper.GetIllustration("NetShieldOffIllustrationSource");
    }

    protected override void OnSaveSettings()
    {
        Settings.IsNetShieldEnabled = IsNetShieldEnabled;
    }

    protected override void OnRetrieveSettings()
    {
        IsNetShieldEnabled = Settings.IsNetShieldEnabled;
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        yield return new(nameof(ISettings.IsNetShieldEnabled), IsNetShieldEnabled, Settings.IsNetShieldEnabled != IsNetShieldEnabled);
    }
}