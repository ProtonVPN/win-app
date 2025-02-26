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
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;

namespace ProtonVPN.Client.UI.Main.Settings.Connection;

public partial class NetShieldPageViewModel : SettingsPageViewModelBase
{
    private readonly IUrlsBrowser _urlsBrowser;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NetShieldFeatureIconSource))]
    [NotifyPropertyChangedFor(nameof(IsNetShieldStatsPanelVisible))]
    private bool _isNetShieldEnabled;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.NetShieldMode))]
    [NotifyPropertyChangedFor(nameof(IsNetShieldLevelOne))]
    [NotifyPropertyChangedFor(nameof(IsNetShieldLevelTwo))]
    [NotifyPropertyChangedFor(nameof(NetShieldFeatureIconSource))]
    [NotifyPropertyChangedFor(nameof(IsNetShieldStatsPanelVisible))]
    private NetShieldMode _currentNetShieldMode;

    public bool IsNetShieldLevelOne
    {
        get => IsNetShieldMode(NetShieldMode.BlockMalwareOnly);
        set => SetNetShieldMode(value, NetShieldMode.BlockMalwareOnly);
    }

    public bool IsNetShieldLevelTwo
    {
        get => IsNetShieldMode(NetShieldMode.BlockAdsMalwareTrackers);
        set => SetNetShieldMode(value, NetShieldMode.BlockAdsMalwareTrackers);
    }

    public bool IsNetShieldStatsPanelVisible => ConnectionManager.IsConnected
                                             && IsNetShieldEnabled
                                             && Settings.IsNetShieldEnabled
                                             && CurrentNetShieldMode == NetShieldMode.BlockAdsMalwareTrackers
                                             && Settings.NetShieldMode == NetShieldMode.BlockAdsMalwareTrackers;

    public override string Title => Localizer.Get("Settings_Connection_NetShield");

    public ImageSource NetShieldFeatureIconSource => GetFeatureIconSource(IsNetShieldEnabled);

    public string LearnMoreUrl => _urlsBrowser.NetShieldLearnMore;

    public NetShieldPageViewModel(
        IUrlsBrowser urlsBrowser,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;

        PageSettings =
        [
            ChangedSettingArgs.Create(() => Settings.NetShieldMode, () => CurrentNetShieldMode),
            ChangedSettingArgs.Create(() => Settings.IsNetShieldEnabled, () => IsNetShieldEnabled)
        ];
    }

    public static ImageSource GetFeatureIconSource(bool isEnabled)
    {
        return isEnabled
            ? ResourceHelper.GetIllustration("NetShieldOnLevel1IllustrationSource")
            : ResourceHelper.GetIllustration("NetShieldOffIllustrationSource");
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        OnPropertyChanged(nameof(IsNetShieldStatsPanelVisible));
    }

    protected override void OnSaveSettings()
    {
        OnPropertyChanged(nameof(IsNetShieldStatsPanelVisible));
    }

    protected override void OnRetrieveSettings()
    {
        IsNetShieldEnabled = Settings.IsNetShieldEnabled;
        CurrentNetShieldMode = Settings.NetShieldMode;
    }

    private bool IsNetShieldMode(NetShieldMode netShieldMode)
    {
        return CurrentNetShieldMode == netShieldMode;
    }

    private void SetNetShieldMode(bool value, NetShieldMode netShieldMode)
    {
        if (value)
        {
            CurrentNetShieldMode = netShieldMode;
        }
    }

    protected override void OnConnectionStatusChanged(ConnectionStatus connectionStatus)
    {
        OnPropertyChanged(nameof(IsNetShieldStatsPanelVisible));
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        if (propertyName == nameof(ISettings.IsNetShieldEnabled) ||
            propertyName == nameof(ISettings.NetShieldMode))
        {
            OnPropertyChanged(nameof(IsNetShieldStatsPanelVisible));
        }
    }
}