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

using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;

namespace ProtonVPN.Client.UI.Settings.Pages;

public class AdvancedSettingsViewModel : SettingsPageViewModelBase
{
    private readonly IUrls _urls;

    public override string? Title => Localizer.Get("Settings_Connection_AdvancedSettings");

    public string CustomDnsServersSettingsState => Localizer.GetToggleValue(Settings.IsCustomDnsServersEnabled);

    public string NatTypeLearnMoreUrl => _urls.NatTypeLearnMore;

    public bool IsAlternativeRoutingEnabled
    {
        get => Settings.IsAlternativeRoutingEnabled;
        set => Settings.IsAlternativeRoutingEnabled = value;
    }

    public bool IsStrictNatType
    {
        get => IsNatType(NatType.Strict);
        set => SetNatType(value, NatType.Strict);
    }

    public bool IsModerateNatType
    {
        get => IsNatType(NatType.Moderate);
        set => SetNatType(value, NatType.Moderate);
    }

    public AdvancedSettingsViewModel(IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ISettings settings,
        IUrls urls)
        : base(viewNavigator, localizationProvider, settings)
    {
        _urls = urls;
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(ISettings.IsAlternativeRoutingEnabled):
                OnPropertyChanged(nameof(IsAlternativeRoutingEnabled));
                break;

            case nameof(ISettings.NatType):
                OnPropertyChanged(nameof(IsStrictNatType));
                OnPropertyChanged(nameof(IsModerateNatType));
                break;

            case nameof(ISettings.IsCustomDnsServersEnabled):
                OnPropertyChanged(nameof(CustomDnsServersSettingsState));
                break;
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(CustomDnsServersSettingsState));
    }

    private bool IsNatType(NatType natType)
    {
        return Settings.NatType == natType;
    }

    private void SetNatType(bool value, NatType natType)
    {
        if (value)
        {
            Settings.NatType = natType;
        }
    }
}