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
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.UI.Settings.Pages;

public class CensorshipViewModel : SettingsPageViewModelBase
{
    private readonly IUrls _urls;

    public override string? Title => Localizer.Get("Settings_Improve_Censorship");

    public bool IsShareStatisticsEnabled
    {
        get => Settings.IsShareStatisticsEnabled;
        set => Settings.IsShareStatisticsEnabled = value;
    }

    public bool IsShareCrashReportsEnabled
    {
        get => Settings.IsShareCrashReportsEnabled;
        set => Settings.IsShareCrashReportsEnabled = value;
    }

    public string LearnMoreUrl => _urls.UsageStatisticsLearnMore;

    public CensorshipViewModel(IMainViewNavigator viewNavigator, 
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
            case nameof(IsShareStatisticsEnabled):
                OnPropertyChanged(nameof(IsShareStatisticsEnabled));
                break;

            case nameof(IsShareCrashReportsEnabled):
                OnPropertyChanged(nameof(IsShareCrashReportsEnabled));
                break;
        }
    }
}