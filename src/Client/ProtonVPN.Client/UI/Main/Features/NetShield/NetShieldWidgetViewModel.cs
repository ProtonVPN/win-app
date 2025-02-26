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
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Features.Bases;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Client.UI.Main.Settings.Connection;

namespace ProtonVPN.Client.UI.Main.Features.NetShield;

public partial class NetShieldWidgetViewModel : FeatureWidgetViewModelBase,
    IEventMessageReceiver<NetShieldStatsChangedMessage>
{
    private const int BADGE_MAXIMUM_NUMBER = 99;

    private readonly Lazy<List<ChangedSettingArgs>> _disableNetShieldSettings;
    private readonly Lazy<List<ChangedSettingArgs>> _enableStandardNetShieldSettings;
    private readonly Lazy<List<ChangedSettingArgs>> _enableAdvancedNetShieldSettings;

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

    public bool IsNetShieldEnabled => IsFeatureOverridden
        ? CurrentProfile!.Settings.IsNetShieldEnabled
        : Settings.IsNetShieldEnabled;

    public NetShieldMode NetShieldMode => IsFeatureOverridden
        ? CurrentProfile!.Settings.NetShieldMode
        : Settings.NetShieldMode;

    public bool IsInfoMessageVisible => !ConnectionManager.IsConnected
                                     || !IsNetShieldEnabled;

    public bool IsBlockMalwareOnlyMessageVisible => ConnectionManager.IsConnected
                                                 && IsNetShieldEnabled
                                                 && NetShieldMode == NetShieldMode.BlockMalwareOnly;

    public bool IsBlockAdsMalwareTrackersMessageVisible => ConnectionManager.IsConnected
                                                        && IsNetShieldEnabled
                                                        && NetShieldMode == NetShieldMode.BlockAdsMalwareTrackers;

    public bool IsNetShieldStatsPanelVisible => ConnectionManager.IsConnected
                                             && IsNetShieldEnabled
                                             && NetShieldMode == NetShieldMode.BlockAdsMalwareTrackers;

    public bool IsStandardNetShieldEnabled => IsNetShieldEnabled && NetShieldMode == NetShieldMode.BlockMalwareOnly;

    public bool IsAdvancedNetShieldEnabled => IsNetShieldEnabled && NetShieldMode == NetShieldMode.BlockAdsMalwareTrackers;

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
        IProfileEditor profileEditor)
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
        _disableNetShieldSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.IsNetShieldEnabled, () => false)
        ]);

        _enableStandardNetShieldSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.NetShieldMode, () => NetShieldMode.BlockMalwareOnly),
            ChangedSettingArgs.Create(() => Settings.IsNetShieldEnabled, () => true)
        ]);

        _enableAdvancedNetShieldSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.NetShieldMode, () => NetShieldMode.BlockAdsMalwareTrackers),
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
    }

    protected override void OnSettingsChanged()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsBlockMalwareOnlyMessageVisible));
        OnPropertyChanged(nameof(IsBlockAdsMalwareTrackersMessageVisible));
        OnPropertyChanged(nameof(IsNetShieldStatsPanelVisible));
        OnPropertyChanged(nameof(IsNetShieldEnabled));
        OnPropertyChanged(nameof(NetShieldMode));
        OnPropertyChanged(nameof(IsStandardNetShieldEnabled));
        OnPropertyChanged(nameof(IsAdvancedNetShieldEnabled));
    }

    protected override void OnConnectionStatusChanged()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsBlockMalwareOnlyMessageVisible));
        OnPropertyChanged(nameof(IsBlockAdsMalwareTrackersMessageVisible));
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
    private Task<bool> EnableStandardNetShieldAsync()
    {
        return TryChangeFeatureSettingsAsync(_enableStandardNetShieldSettings.Value);
    }

    [RelayCommand]
    private Task<bool> EnableAdvancedNetShieldAsync()
    {
        return TryChangeFeatureSettingsAsync(_enableAdvancedNetShieldSettings.Value);
    }
}