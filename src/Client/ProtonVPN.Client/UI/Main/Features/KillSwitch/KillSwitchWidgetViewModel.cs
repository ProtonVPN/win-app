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

using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Contracts.Services.Selection;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.UI.Main.Features.Bases;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Features.KillSwitch;

public class KillSwitchWidgetViewModel : FeatureWidgetViewModelBase
{
    private readonly IApplicationThemeSelector _applicationThemeSelector;

    public override string Header => Localizer.Get("Settings_Connection_KillSwitch");

    public KillSwitchWidgetViewModel(
        IApplicationThemeSelector applicationThemeSelector,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IMainViewNavigator mainViewNavigator)
        : base(localizer, logger, issueReporter, mainViewNavigator, settings, ConnectionFeature.KillSwitch)
    {
        _applicationThemeSelector = applicationThemeSelector;
    }

    protected override string GetFeatureStatus()
    {
        return Localizer.Get(
            Settings.IsKillSwitchEnabled
                ? Settings.KillSwitchMode switch
                {
                    KillSwitchMode.Standard => "Settings_Connection_KillSwitch_Standard",
                    KillSwitchMode.Advanced => "Settings_Connection_KillSwitch_Advanced",
                    _ => throw new ArgumentOutOfRangeException(nameof(ISettings.KillSwitchMode))
                }
                : "Common_States_Off");
    }
}