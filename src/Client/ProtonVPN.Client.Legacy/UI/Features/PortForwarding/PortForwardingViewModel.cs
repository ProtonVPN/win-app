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
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Legacy.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Legacy.Models.Activation;
using ProtonVPN.Client.Legacy.Models.Clipboards;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Legacy.UI.Settings.Pages.Entities;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.UI.Features.PortForwarding;

public partial class PortForwardingViewModel : SettingsPageViewModelBase,
    IEventMessageReceiver<PortForwardingPortChanged>,
    IEventMessageReceiver<PortForwardingStatusChanged>
{
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

    [ObservableProperty]
    private bool _isPortForwardingNotificationEnabled;

    public bool IsExpanded => IsPortForwardingEnabled && ConnectionManager.IsConnected && (HasActivePortNumber || HasStatusMessage);

    public bool HasActivePortNumber => ActivePortNumber.HasValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
    [NotifyPropertyChangedFor(nameof(IsExpanded))]
    [NotifyPropertyChangedFor(nameof(ActivePortNumberOrStatusMessage))]
    private string? _statusMessage;

    public bool HasStatusMessage => !HasActivePortNumber && StatusMessage is not null;

    public override bool IsBackEnabled => false;

    public override string? Title => Localizer.Get("Settings_Connection_PortForwarding");

    public ImageSource PortForwardingFeatureIconSource => GetFeatureIconSource(IsPortForwardingEnabled);

    public string LearnMoreUrl => _urls.PortForwardingLearnMore;

    public string? ActivePortNumberOrStatusMessage => ActivePortNumber?.ToString() ?? StatusMessage;

    public PortForwardingViewModel(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IOverlayActivator overlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IUrls urls,
        IClipboardEditor clipboardEditor,
        IConnectionManager connectionManager,
        IPortForwardingManager portForwardingManager,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator,
               localizationProvider,
               overlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               logger,
               issueReporter)
    {
        _urls = urls;
        _clipboardEditor = clipboardEditor;
        _portForwardingManager = portForwardingManager;

        InvalidateStatusMessageAndActivePortNumber();
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

    public void Receive(PortForwardingStatusChanged message)
    {
        ExecuteOnUIThread(InvalidateStatusMessageAndActivePortNumber);
    }

    public void Receive(PortForwardingPortChanged message)
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