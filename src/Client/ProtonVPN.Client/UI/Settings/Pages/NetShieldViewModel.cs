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

using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;

namespace ProtonVPN.Client.UI.Settings.Pages;

public class NetShieldViewModel : PageViewModelBase<IMainViewNavigator>, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IUrls _urls;

    public override string? Title => Localizer.Get("Settings_Features_NetShield");

    public ImageSource NetShieldFeatureIconSource => GetFeatureIconSource(_settings.IsNetShieldEnabled);

    public string LearnMoreUrl => _urls.NetShieldLearnMore;

    public bool IsNetShieldEnabled
    {
        get => _settings.IsNetShieldEnabled;
        set => _settings.IsNetShieldEnabled = value;
    }

    public NetShieldViewModel(IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ISettings settings,
        IUrls urls)
        : base(viewNavigator, localizationProvider)
    {
        _settings = settings;
        _urls = urls;
    }

    public static ImageSource GetFeatureIconSource(bool isEnabled)
    {
        return isEnabled
            ? ResourceHelper.GetIllustration("NetShieldOnIllustrationSource")
            : ResourceHelper.GetIllustration("NetShieldOffIllustrationSource");
    }

    public void Receive(SettingChangedMessage message)
    {
        switch (message.PropertyName)
        {
            case nameof(ISettings.IsNetShieldEnabled):
                OnPropertyChanged(nameof(IsNetShieldEnabled));
                OnPropertyChanged(nameof(NetShieldFeatureIconSource));
                break;

            default:
                break;
        }
    }
}