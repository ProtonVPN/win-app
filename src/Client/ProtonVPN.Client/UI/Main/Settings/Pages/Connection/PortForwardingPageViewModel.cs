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
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Contracts.Helpers;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Models.Clipboards;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings.Connection;

public partial class PortForwardingPageViewModel : SettingsPageViewModelBase,
    IEventMessageReceiver<PortForwardingStatusChangedMessage>,
    IEventMessageReceiver<PortForwardingPortChangedMessage>
{
    public override string Title => Localizer.Get("Settings_Connection_PortForwarding");

    private readonly IUrls _urls;
    private readonly IClipboardEditor _clipboardEditor;
    private readonly IPortForwardingManager _portForwardingManager;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CopyPortNumberCommand))]
    [NotifyPropertyChangedFor(nameof(HasActivePortNumber))]
    [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
    [NotifyPropertyChangedFor(nameof(IsExpanded))]
    [NotifyPropertyChangedFor(nameof(ActivePortNumberOrStatusMessage))]
    private int? _activePortNumber;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PortForwardingFeatureIconSource))]
    [NotifyPropertyChangedFor(nameof(IsExpanded))]
    private bool _isPortForwardingEnabled;

    [ObservableProperty] private bool _isPortForwardingNotificationEnabled;

    public bool IsExpanded => IsPortForwardingEnabled && ConnectionManager.IsConnected &&
                              (HasActivePortNumber || HasStatusMessage);

    public bool HasActivePortNumber => ActivePortNumber.HasValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
    [NotifyPropertyChangedFor(nameof(IsExpanded))]
    [NotifyPropertyChangedFor(nameof(ActivePortNumberOrStatusMessage))]
    private string? _statusMessage;

    public bool HasStatusMessage => !HasActivePortNumber && StatusMessage is not null;

    public ImageSource PortForwardingFeatureIconSource => GetFeatureIconSource(IsPortForwardingEnabled);

    public string LearnMoreUrl => _urls.PortForwardingLearnMore;

    public string? ActivePortNumberOrStatusMessage => ActivePortNumber?.ToString() ?? StatusMessage;

    public PortForwardingPageViewModel(
        IUrls urls,
        IClipboardEditor clipboardEditor,
        IPortForwardingManager portForwardingManager,
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
        _urls = urls;
        _clipboardEditor = clipboardEditor;
        _portForwardingManager = portForwardingManager;
    }

    public static ImageSource GetFeatureIconSource(bool isEnabled)
    {
        return isEnabled
            ? ResourceHelper.GetIllustration("PortForwardingOnIllustrationSource")
            : ResourceHelper.GetIllustration("PortForwardingOffIllustrationSource");
    }

    [RelayCommand(CanExecute = nameof(CanCopyPortNumber))]
    public async Task CopyPortNumberAsync()
    {
        int? activePortNumber = ActivePortNumber;
        if (activePortNumber is not null)
        {
            await _clipboardEditor.SetTextAsync($"{activePortNumber}");
        }
    }

    public bool CanCopyPortNumber()
    {
        return HasActivePortNumber;
    }

    protected override void OnSaveSettings()
    {
        Settings.IsPortForwardingEnabled = IsPortForwardingEnabled;
        Settings.IsPortForwardingNotificationEnabled = IsPortForwardingNotificationEnabled;
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

    public void Receive(PortForwardingStatusChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateStatusMessageAndActivePortNumber);
    }

    public void Receive(PortForwardingPortChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateStatusMessageAndActivePortNumber);
    }

    private void InvalidateStatusMessageAndActivePortNumber()
    {
        int? activePort = _portForwardingManager.ActivePort;

        ActivePortNumber = activePort;

        StatusMessage = activePort is null && _portForwardingManager.IsFetchingPort
            ? Localizer.Get("Settings_Connection_PortForwarding_Loading")
            : null;
    }

    protected override void OnConnectionStatusChanged(ConnectionStatus connectionStatus)
    {
        OnPropertyChanged(nameof(IsExpanded));
    }
}