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
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.UI.Main.Features.Bases;
using ProtonVPN.Client.UI.Main.Settings.Connection;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Features.NetShield;

public partial class NetShieldWidgetViewModel : FeatureWidgetViewModelBase,
    IEventMessageReceiver<NetShieldStatsChangedMessage>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalAdsAndTrackersBlocked))]
    private long _numberOfTrackersStopped;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalAdsAndTrackersBlocked))]
    private long _numberOfAdsBlocked;

    public int TotalAdsAndTrackersBlocked => Convert.ToInt32(NumberOfTrackersStopped + NumberOfAdsBlocked);

    public override string Header => Localizer.Get("Settings_Connection_NetShield");

    public string InfoMessage => Localizer.Get("Flyouts_NetShield_Info");

    public string BlockMalwareOnlyMessage => Localizer.Get("Flyouts_NetShield_MalwareOnly_Success");

    public string BlockAdsMalwareTrackersMessage => Localizer.Get("Flyouts_NetShield_AdsMalwareTrackers_Success");

    public bool IsInfoMessageVisible => !ConnectionManager.IsConnected
                                     || !Settings.IsNetShieldEnabled;

    public bool IsBlockMalwareOnlyMessageVisible => ConnectionManager.IsConnected
                                                 && Settings.IsNetShieldEnabled
                                                 && Settings.NetShieldMode == NetShieldMode.BlockMalwareOnly;

    public bool IsBlockAdsMalwareTrackersMessageVisible => ConnectionManager.IsConnected
                                                        && Settings.IsNetShieldEnabled
                                                        && Settings.NetShieldMode == NetShieldMode.BlockAdsMalwareTrackers;

    public bool IsNetShieldStatsPanelVisible => ConnectionManager.IsConnected
                                             && Settings.IsNetShieldEnabled
                                             && Settings.NetShieldMode == NetShieldMode.BlockAdsMalwareTrackers;

    protected override UpsellFeatureType? UpsellFeature { get; } = UpsellFeatureType.NetShield;

    public NetShieldWidgetViewModel(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator)
        : base(localizer,
               logger,
               issueReporter,
               mainViewNavigator,
               settingsViewNavigator,
               settings,
               connectionManager,
               upsellCarouselWindowActivator,
               ConnectionFeature.NetShield)
    { }

    public void Receive(NetShieldStatsChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            if (ConnectionManager.IsConnected)
            {
                SetNetShieldStats(message);
            }
            else
            {
                ClearNetShieldStats();
            }
        });
    }

    protected override IEnumerable<string> GetSettingsChangedForUpdate()
    {
        yield return nameof(ISettings.NetShieldMode);
        yield return nameof(ISettings.IsNetShieldEnabled);
    }

    protected override string GetFeatureStatus()
    {
        return Localizer.GetToggleValue(Settings.IsNetShieldEnabled);
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(BlockMalwareOnlyMessage));
        OnPropertyChanged(nameof(BlockAdsMalwareTrackersMessage));
    }

    protected override void OnSettingsChanged()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsBlockMalwareOnlyMessageVisible));
        OnPropertyChanged(nameof(IsBlockAdsMalwareTrackersMessageVisible));
        OnPropertyChanged(nameof(IsNetShieldStatsPanelVisible));
    }

    protected override void OnConnectionStatusChanged()
    {
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsBlockMalwareOnlyMessageVisible));
        OnPropertyChanged(nameof(IsBlockAdsMalwareTrackersMessageVisible));
        OnPropertyChanged(nameof(IsNetShieldStatsPanelVisible));

        if (!ConnectionManager.IsConnected)
        {
            ClearNetShieldStats();
        }
    }

    protected override bool IsOnFeaturePage(PageViewModelBase? currentPageContext)
    {
        return currentPageContext is NetShieldPageViewModel;
    }

    private void SetNetShieldStats(NetShieldStatsChangedMessage stats)
    {
        NumberOfAdsBlocked = stats.NumOfAdvertisementUrlsBlocked;
        NumberOfTrackersStopped = stats.NumOfTrackingUrlsBlocked;
    }

    private void ClearNetShieldStats()
    {
        NumberOfAdsBlocked = 0;
        NumberOfTrackersStopped = 0;
    }
}