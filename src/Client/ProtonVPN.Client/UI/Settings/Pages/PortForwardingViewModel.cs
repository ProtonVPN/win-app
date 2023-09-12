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
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Edition;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.UI.Settings.Pages;

public partial class PortForwardingViewModel : SettingsPageViewModelBase
{
    private readonly IUrls _urls;
    private readonly IClipboardEditor _clipboardEditor;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CopyPortNumberCommand))]
    private int? _activePortNumber;

    public override string? Title => Localizer.Get("Settings_Features_PortForwarding");

    public ImageSource PortForwardingFeatureIconSource => GetFeatureIconSource(Settings.IsPortForwardingEnabled);

    public string LearnMoreUrl => _urls.PortForwardingLearnMore;

    public bool IsPortForwardingEnabled
    {
        get => Settings.IsPortForwardingEnabled;
        set => Settings.IsPortForwardingEnabled = value;
    }

    public bool IsPortForwardingNotificationEnabled
    {
        get => Settings.IsPortForwardingNotificationEnabled;
        set => Settings.IsPortForwardingNotificationEnabled = value;
    }

    public PortForwardingViewModel(IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ISettings settings,
        IUrls urls,
        IClipboardEditor clipboardEditor)
        : base(viewNavigator, localizationProvider, settings)
    {
        _urls = urls;
        _clipboardEditor = clipboardEditor;

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
        string portNumber = ActivePortNumber.GetValueOrDefault().ToString();

        if (!string.IsNullOrEmpty(portNumber))
        {
            await _clipboardEditor.SetTextAsync(portNumber);
        }
    }

    public bool CanCopyPortNumber()
    {
        return IsPortForwardingEnabled && ActivePortNumber.HasValue;
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(ISettings.IsPortForwardingEnabled):
                OnPropertyChanged(nameof(IsPortForwardingEnabled));
                OnPropertyChanged(nameof(PortForwardingFeatureIconSource));
                InvalidateActivePortNumber();
                break;

            case nameof(ISettings.IsPortForwardingNotificationEnabled):
                OnPropertyChanged(nameof(IsPortForwardingNotificationEnabled));
                break;

            default:
                break;
        }
    }

    private void InvalidateActivePortNumber()
    {
        // TODO: Retrieve the actual port number for port forwarding
        ActivePortNumber = IsPortForwardingEnabled ? 12345 : null;
    }
}