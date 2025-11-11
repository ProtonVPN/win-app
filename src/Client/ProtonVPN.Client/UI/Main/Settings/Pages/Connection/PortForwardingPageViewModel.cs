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
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;

namespace ProtonVPN.Client.UI.Main.Settings.Connection;

public partial class PortForwardingPageViewModel : SettingsPageViewModelBase,
    IEventMessageReceiver<PortForwardingPortChangedMessage>,
    IEventMessageReceiver<PortForwardingStatusChangedMessage>
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IPortForwardingManager _portForwardingManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PortForwardingFeatureIconSource))]
    [NotifyPropertyChangedFor(nameof(IsExpanded))]
    private bool _isPortForwardingEnabled;

    [ObservableProperty]
    private bool _isPortForwardingNotificationEnabled;

    public override string Title => Localizer.Get("Settings_Connection_PortForwarding");

    public bool IsExpanded => IsPortForwardingEnabled
                           && Settings.IsPortForwardingEnabled
                           && ConnectionManager.IsConnected;

    public ImageSource PortForwardingFeatureIconSource => GetFeatureIconSource(IsPortForwardingEnabled);

    public string WarningMessage => !ConnectionManager.IsP2PServerConnection()
        ? Localizer.Get("Flyouts_PortForwarding_Warning")
        : Localizer.Get("Flyouts_PortForwarding_ActivePort_Error");

    public bool IsWarningMessageVisible => ConnectionManager.IsConnected
                                        && IsPortForwardingEnabled
                                        && (!ConnectionManager.IsP2PServerConnection() || _portForwardingManager.HasError);

    public string LearnMoreUrl => _urlsBrowser.PortForwardingLearnMore;

    public PortForwardingPageViewModel(
        IUrlsBrowser urlsBrowser,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper,
        IPortForwardingManager portForwardingManager)
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
        _portForwardingManager = portForwardingManager;
        PageSettings =
        [
            ChangedSettingArgs.Create(() => Settings.IsPortForwardingNotificationEnabled, () => IsPortForwardingNotificationEnabled),
            ChangedSettingArgs.Create(() => Settings.IsPortForwardingEnabled, () => IsPortForwardingEnabled)
        ];
    }

    public static ImageSource GetFeatureIconSource(bool isEnabled)
    {
        return isEnabled
            ? ResourceHelper.GetIllustration("PortForwardingOnIllustrationSource")
            : ResourceHelper.GetIllustration("PortForwardingOffIllustrationSource");
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        OnPropertyChanged(nameof(IsExpanded));
        OnPropertyChanged(nameof(WarningMessage));
        OnPropertyChanged(nameof(IsWarningMessageVisible));
    }

    protected override void OnSaveSettings()
    {
        OnPropertyChanged(nameof(IsExpanded));
        OnPropertyChanged(nameof(IsWarningMessageVisible));
    }

    protected override void OnRetrieveSettings()
    {
        IsPortForwardingEnabled = Settings.IsPortForwardingEnabled;
        IsPortForwardingNotificationEnabled = Settings.IsPortForwardingNotificationEnabled;
    }

    protected override void OnConnectionStatusChanged(ConnectionStatus connectionStatus)
    {
        OnPropertyChanged(nameof(IsExpanded));
        OnPropertyChanged(nameof(WarningMessage));
        OnPropertyChanged(nameof(IsWarningMessageVisible));
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        if (propertyName == nameof(ISettings.IsPortForwardingEnabled))
        {
            OnPropertyChanged(nameof(IsExpanded));
            OnPropertyChanged(nameof(IsWarningMessageVisible));
        }
    }

    public void Receive(PortForwardingPortChangedMessage message)
    {
        ExecuteOnUIThread(OnPortForwardingChange);
    }

    private void OnPortForwardingChange()
    {
        OnPropertyChanged(nameof(IsWarningMessageVisible));
    }

    public void Receive(PortForwardingStatusChangedMessage message)
    {
        ExecuteOnUIThread(OnPortForwardingChange);
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(WarningMessage));
    }
}
