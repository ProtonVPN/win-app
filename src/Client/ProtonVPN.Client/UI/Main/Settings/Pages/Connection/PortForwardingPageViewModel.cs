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
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings.Connection;

public partial class PortForwardingPageViewModel : SettingsPageViewModelBase
{
    private readonly IUrlsBrowser _urlsBrowser;

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

    public string LearnMoreUrl => _urlsBrowser.PortForwardingLearnMore;

    public PortForwardingPageViewModel(
        IUrlsBrowser urlsBrowser,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               localizer,
               logger,
               issueReporter,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager)
    {
        _urlsBrowser = urlsBrowser;
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
    }

    protected override void OnSaveSettings()
    {
        Settings.IsPortForwardingEnabled = IsPortForwardingEnabled;
        Settings.IsPortForwardingNotificationEnabled = IsPortForwardingNotificationEnabled;

        OnPropertyChanged(nameof(IsExpanded));
    }

    protected override void OnRetrieveSettings()
    {
        IsPortForwardingEnabled = Settings.IsPortForwardingEnabled;
        IsPortForwardingNotificationEnabled = Settings.IsPortForwardingNotificationEnabled;
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        yield return new(nameof(ISettings.IsPortForwardingEnabled), IsPortForwardingEnabled,
            Settings.IsPortForwardingEnabled != IsPortForwardingEnabled);

        yield return new(nameof(ISettings.IsPortForwardingNotificationEnabled), IsPortForwardingNotificationEnabled,
            Settings.IsPortForwardingNotificationEnabled != IsPortForwardingNotificationEnabled);
    }

    protected override void OnConnectionStatusChanged(ConnectionStatus connectionStatus)
    {
        OnPropertyChanged(nameof(IsExpanded));
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        if (propertyName == nameof(ISettings.IsPortForwardingEnabled))
        {
            OnPropertyChanged(nameof(IsExpanded));
        }
    }
}