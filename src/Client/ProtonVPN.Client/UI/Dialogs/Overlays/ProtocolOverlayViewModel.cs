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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;

namespace ProtonVPN.Client.UI.Dialogs.Overlays;

public partial class ProtocolOverlayViewModel : OverlayViewModelBase, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IPageNavigator _pageNavigator;
    private readonly ISettings _settings;
    private readonly IUrls _urls;

    public string LearnMoreUrl => _urls.ProtocolChangeLearnMore;

    public string CurrentProtocol => Localizer.Get($"Settings_SelectedProtocol_{_settings.VpnProtocol}");

    public ProtocolOverlayViewModel(ILocalizationProvider localizationProvider,
        IDialogActivator dialogActivator,
        IPageNavigator pageNavigator,
        ISettings settings,
        IUrls urls)
        : base(localizationProvider, dialogActivator)
    {
        _pageNavigator = pageNavigator;
        _settings = settings;
        _urls = urls;
    }

    [RelayCommand]
    public void NavigateTo(string pageKey)
    {
        CloseOverlay();

        _pageNavigator.NavigateTo(pageKey);
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.VpnProtocol))
        {
            OnPropertyChanged(nameof(CurrentProtocol));
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();
        OnPropertyChanged(nameof(CurrentProtocol));
    }
}