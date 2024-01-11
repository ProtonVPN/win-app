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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Clipboards;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Settings.Pages.Entities;

namespace ProtonVPN.Client.UI.Settings.Pages;

public partial class PortForwardingViewModel : SettingsPageViewModelBase, IEventMessageReceiver<PortForwardingPortChanged>
{
    private readonly IUrls _urls;
    private readonly IClipboardEditor _clipboardEditor;
    private readonly IPortForwardingManager _portForwardingManager;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CopyPortNumberCommand))]
    [NotifyPropertyChangedFor(nameof(HasActivePortNumber))]
    private int? _activePortNumber;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PortForwardingFeatureIconSource))]
    [NotifyPropertyChangedFor(nameof(HasActivePortNumber))]
    private bool _isPortForwardingEnabled;

    [ObservableProperty]
    private bool _isPortForwardingNotificationEnabled;

    public bool HasActivePortNumber => ActivePortNumber.HasValue && IsPortForwardingEnabled && ConnectionManager.IsConnected;

    public override string? Title => Localizer.Get("Settings_Features_PortForwarding");

    public ImageSource PortForwardingFeatureIconSource => GetFeatureIconSource(IsPortForwardingEnabled);

    public string LearnMoreUrl => _urls.PortForwardingLearnMore;

    public PortForwardingViewModel(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IOverlayActivator overlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IUrls urls,
        IClipboardEditor clipboardEditor,
        IConnectionManager connectionManager,
        IPortForwardingManager portForwardingManager)
        : base(viewNavigator,
               localizationProvider,
               overlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager)
    {
        _urls = urls;
        _clipboardEditor = clipboardEditor;
        _portForwardingManager = portForwardingManager;

        InvalidateActivePortNumber();
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

    protected override void SaveSettings()
    {
        Settings.IsPortForwardingEnabled = IsPortForwardingEnabled;
        Settings.IsPortForwardingNotificationEnabled = IsPortForwardingNotificationEnabled;
    }

    protected override void RetrieveSettings()
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

    private void InvalidateActivePortNumber()
    {
        ActivePortNumber = _portForwardingManager.ActivePort;
    }

    public void Receive(PortForwardingPortChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            InvalidateActivePortNumber();
        });
    }

    public override void Receive(ConnectionStatusChanged message)
    {
        base.Receive(message);
        OnPropertyChanged(nameof(HasActivePortNumber));
    }
}