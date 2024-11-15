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

using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.UI.Main.Features.Bases;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Features.SplitTunneling;

public class SplitTunnelingWidgetViewModel : FeatureWidgetViewModelBase
{
    private readonly IApplicationThemeSelector _applicationThemeSelector;

    public override string Header => Localizer.Get("Settings_Connection_SplitTunneling");

    public SplitTunnelingWidgetViewModel(
        IApplicationThemeSelector applicationThemeSelector,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator)
        : base(localizer, logger, issueReporter, mainViewNavigator, settingsViewNavigator, settings, ConnectionFeature.SplitTunneling)
    {
        _applicationThemeSelector = applicationThemeSelector;
    }

    protected override string GetFeatureStatus()
    {
        return Localizer.Get(
            Settings.IsSplitTunnelingEnabled
                ? Settings.SplitTunnelingMode switch
                {
                    SplitTunnelingMode.Standard => "Settings_Connection_SplitTunneling_Standard",
                    SplitTunnelingMode.Inverse => "Settings_Connection_SplitTunneling_Inverse",
                    _ => throw new ArgumentOutOfRangeException(nameof(ISettings.KillSwitchMode))
                }
                : "Common_States_Off");
    }
}