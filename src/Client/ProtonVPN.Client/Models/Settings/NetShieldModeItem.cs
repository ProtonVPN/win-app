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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Core.Bases.Models;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Settings.Contracts.Enums;

namespace ProtonVPN.Client.Models.Settings;

public class NetShieldModeItem : ModelBase
{
    public bool IsEnabled { get; }

    public NetShieldMode Mode { get; }

    public string ShortHeader => Localizer.GetToggleValue(IsEnabled);

    public string Header => Localizer.GetNetShieldMode(IsEnabled, Mode);

    public ImageSource IllustrationSource { get; }

    public bool IsStandardNetShieldEnabled => IsEnabled && Mode == NetShieldMode.BlockMalwareOnly;

    public bool IsAdvancedNetShieldEnabled => IsEnabled && Mode == NetShieldMode.BlockAdsMalwareTrackers;

    public NetShieldModeItem(
        ILocalizationProvider localizer,
        IApplicationThemeSelector themeSelector,
        bool isEnabled,
        NetShieldMode netShieldMode)
        : base(localizer)
    {
        IsEnabled = isEnabled;
        Mode = netShieldMode;

        ElementTheme theme = themeSelector.GetTheme();

        IllustrationSource = IsEnabled
            ? Mode switch
            {
                NetShieldMode.BlockMalwareOnly => ResourceHelper.GetIllustration("NetShieldOnLevel1IllustrationSource", theme),
                NetShieldMode.BlockAdsMalwareTrackers => ResourceHelper.GetIllustration("NetShieldOnLevel2IllustrationSource", theme),
            }
            : ResourceHelper.GetIllustration("NetShieldOffIllustrationSource", theme) ;
    }
}