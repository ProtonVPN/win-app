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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Helpers;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.UI.Main.Features.Bases;

namespace ProtonVPN.Client.UI.Main.Features.NetShield;

public class NetShieldWidgetViewModel : FeatureWidgetViewModelBase
{
    public override string Header => Localizer.Get("Settings_Features_NetShield");

    public NetShieldWidgetViewModel(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IMainViewNavigator mainViewNavigator)
        : base(localizer, logger, issueReporter, mainViewNavigator, settings, ConnectionFeature.NetShield)
    { }

    protected override string GetFeatureStatus()
    {
        return Localizer.GetToggleValue(Settings.IsNetShieldEnabled);
    }

    protected override ImageSource GetFeatureIconSource()
    {
        return ResourceHelper.GetIllustration(
            Settings.IsNetShieldEnabled
                ? "NetShieldOnIllustrationSource"
                : "NetShieldOffIllustrationSource");
    }

    protected override string GetFeatureToggleSettingName()
    {
        return nameof(Settings.IsNetShieldEnabled);
    }
}