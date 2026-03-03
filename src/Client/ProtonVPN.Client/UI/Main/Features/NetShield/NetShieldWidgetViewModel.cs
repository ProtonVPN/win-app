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
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.Profiles;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Features.Bases;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Client.UI.Main.Settings.Connection;

namespace ProtonVPN.Client.UI.Main.Features.NetShield;

public partial class NetShieldWidgetViewModel : FeatureWidgetViewModelBase,
    IEventMessageReceiver<NetShieldStatsChangedMessage>,
    IEventMessageReceiver<FeatureFlagsChangedMessage>
{
    private const int BADGE_MAXIMUM_NUMBER = 99;

    private readonly IFeatureFlagsObserver _featureFlagsObserver;

    private readonly Lazy<List<ChangedSettingArgs>> _disableNetShieldSettings;
    private readonly Lazy<List<ChangedSettingArgs>> _enableNetShieldLevelOneSettings;
    private readonly Lazy<List<ChangedSettingArgs>> _enableNetShieldLevelTwoSettings;  
    private readonly Lazy<List<ChangedSettingArgs>> _enableNetShieldLevelThreeSettings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalAdsAndTrackersBlocked))]
    [NotifyPropertyChangedFor(nameof(FormattedTotalAdsAndTrackersBlocked))]
    private long _numberOfTrackersStopped;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalAdsAndTrackersBlocked))]
    [NotifyPropertyChangedFor(nameof(FormattedTotalAdsAndTrackersBlocked))]
    private long _numberOfAdsBlocked;

    public int TotalAdsAndTrackersBlocked => Convert.ToInt32(NumberOfTrackersStopped + NumberOfAdsBlocked);

    public string FormattedTotalAdsAndTrackersBlocked =>
        TotalAdsAndTrackersBlocked > BADGE_MAXIMUM_NUMBER
            ? $"{BADGE_MAXIMUM_NUMBER}+"
            : $"{TotalAdsAndTrackersBlocked}";

    public override string Header => Localizer.Get("Settings_Connection_NetShield");

    public string InfoMessage => Localizer.Get("Flyouts_NetShield_Info");

    public string BlockMalwareOnlyMessage => Localizer.Get("Flyouts_NetShield_MalwareOnly_Success");

    public string BlockAdsMalwareTrackersMessage => Localizer.Get("Flyouts_NetShield_AdsMalwareTrackers_Success");    

    public string BlockAdsMalwareTrackersAdultContentMessage => Localizer.Get("Flyouts_NetShield_AdsMalwareTrackersAdultContent_Success");

    public bool IsNetShieldEnabled => IsFeatureOverridden
        ? CurrentProfile!.Settings.IsNetShieldEnabled
        : Settings.IsNetShieldEnabled;

    public NetShieldMode NetShieldMode => IsFeatureOverridden
        ? CurrentProfile!.Settings.NetShieldMode
        : Settings.NetShieldMode;

    public bool IsInfoMessageVisible => !ConnectionManager.IsConnected
                                     || !IsNetShieldEnabled;

    public bool IsBlockMalwareOnlyMessageVisible => ConnectionManager.IsConnected
                                                 && IsNetShieldLevelOneEnabled;

    public bool IsBlockAdsMalwareTrackersMessageVisible => ConnectionManager.IsConnected
                                                        && IsNetShieldLevelTwoEnabled;   

    public bool IsBlockAdsMalwareTrackersAdultContentMessageVisible => ConnectionManager.IsConnected
                                                                    && IsNetShieldLevelThreeEnabled;

    public bool IsNetShieldStatsPanelVisible => ConnectionManager.IsConnected
                                             && IsNetShieldEnabled
                                             && NetShieldMode >= NetShieldMode.BlockAdsMalwareTrackers;

    public bool IsNetShieldLevelOneEnabled => IsNetShieldEnabled && NetShieldMode == NetShieldMode.BlockMalwareOnly;

    public bool IsNetShieldLevelTwoEnabled => IsNetShieldEnabled && (NetShieldMode == NetShieldMode.BlockAdsMalwareTrackers || (NetShieldMode == NetShieldMode.BlockAdsMalwareTrackersAdultContent && !IsNetShieldLevelThreeAvailable));

    public bool IsNetShieldLevelThreeEnabled => IsNetShieldLevelThreeAvailable && IsNetShieldEnabled && NetShieldMode == NetShieldMode.BlockAdsMalwareTrackersAdultContent;

    public bool IsNetShieldLevelThreeAvailable => _featureFlagsObserver.IsNetShieldLevelThreeEnabled;

    protected override UpsellFeatureType? UpsellFeature { get; } = UpsellFeatureType.NetShield;

    public override bool IsFeatureOverridden => ConnectionManager.IsConnected 
                                             && CurrentProfile != null;

    public NetShieldWidgetViewModel(
        IViewModelHelper viewModelHelper,
        ISettings settings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        ISettingsConflictResolver settingsConflictResolver,
        IProfileEditor profileEditor,
        IFeatureFlagsObserver featureFlagsObserver)
        : base(viewModelHelper,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               connectionManager,
               upsellCarouselWindowActivator,
               requiredReconnectionSettings,
               settingsConflictResolver,
               profileEditor,
               ConnectionFeature.NetShield)
    {
        _featureFlagsObserver = featureFlagsObserver;

        _disableNetShieldSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.IsNetShieldEnabled, () => false)
        ]);

        _enableNetShieldLevelOneSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.NetShieldMode, () => NetShieldMode.BlockMalwareOnly),
            ChangedSettingArgs.Create(() => Settings.IsNetShieldEnabled, () => true)
        ]);

        _enableNetShieldLevelTwoSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.NetShieldMode, () => NetShieldMode.BlockAdsMalwareTrackers),
            ChangedSettingArgs.Create(() => Settings.IsNetShieldEnabled, () => true)
        ]);

        _enableNetShieldLevelThreeSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.NetShieldMode, () => NetShieldMode.BlockAdsMalwareTrackersAdultContent),
            ChangedSettingArgs.Create(() => Settings.IsNetShieldEnabled, () => true)
        ]);
    }

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

    public void Receive(FeatureFlagsChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(IsNetShieldLevelThreeAvailable));
            OnPropertyChanged(nameof(IsNetShieldLevelTwoEnabled));
            OnPropertyChanged(nameof(IsNetShieldLevelThreeEnabled));
            OnPropertyChanged(nameof(IsBlockAdsMalwareTrackersMessageVisible));
            OnPropertyChanged(nameof(IsBlockAdsMalwareTrackersAdultContentMessageVisible));
        });
    }

    protected override IEnumerable<string> GetSettingsChangedForUpdate()
    {
        yield return nameof(ISettings.NetShieldMode);
        yield return nameof(ISettings.IsNetShieldEnabled);
    }

    protected override string GetFeatureStatus()
    {
        return Localizer.GetToggleValue(IsNetShieldEnabled);
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(BlockMalwareOnlyMessage));
        OnPropertyChanged(nameof(BlockAdsMalwareTrackersMessage)); 
        OnPropertyChanged(nameof(BlockAdsMalwareTrackersAdultContentMessage));
    }

    protected override void OnSettingsChanged()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsBlockMalwareOnlyMessageVisible));
        OnPropertyChanged(nameof(IsBlockAdsMalwareTrackersMessageVisible));
        OnPropertyChanged(nameof(IsBlockAdsMalwareTrackersAdultContentMessageVisible));
        OnPropertyChanged(nameof(IsNetShieldStatsPanelVisible));
        OnPropertyChanged(nameof(IsNetShieldEnabled));
        OnPropertyChanged(nameof(NetShieldMode));
        OnPropertyChanged(nameof(IsNetShieldLevelOneEnabled));
        OnPropertyChanged(nameof(IsNetShieldLevelTwoEnabled));
        OnPropertyChanged(nameof(IsNetShieldLevelThreeEnabled));
    }

    protected override void OnConnectionStatusChanged()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsBlockMalwareOnlyMessageVisible));
        OnPropertyChanged(nameof(IsBlockAdsMalwareTrackersMessageVisible));
        OnPropertyChanged(nameof(IsBlockAdsMalwareTrackersAdultContentMessageVisible));
        OnPropertyChanged(nameof(IsNetShieldStatsPanelVisible));
        OnPropertyChanged(nameof(IsFeatureOverridden));
        OnPropertyChanged(nameof(IsNetShieldEnabled));
        OnPropertyChanged(nameof(NetShieldMode));
        OnPropertyChanged(nameof(CurrentProfile));

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

    [RelayCommand]
    private Task<bool> DisableNetShieldAsync()
    {
        return TryChangeFeatureSettingsAsync(_disableNetShieldSettings.Value);
    }

    [RelayCommand]
    private Task<bool> EnableNetShieldLevelOneAsync()
    {
        return TryChangeFeatureSettingsAsync(_enableNetShieldLevelOneSettings.Value);
    }

    [RelayCommand]
    private Task<bool> EnableNetShieldLevelTwoAsync()
    {
        return TryChangeFeatureSettingsAsync(_enableNetShieldLevelTwoSettings.Value);
    }

    [RelayCommand]
    private Task<bool> EnableNetShieldLevelThreeAsync()
    {
        return TryChangeFeatureSettingsAsync(_enableNetShieldLevelThreeSettings.Value);
    }
}