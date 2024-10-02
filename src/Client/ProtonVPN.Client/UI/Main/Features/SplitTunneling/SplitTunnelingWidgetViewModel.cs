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
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Helpers;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.UI.Main.Features.Bases;

namespace ProtonVPN.Client.UI.Main.Features.SplitTunneling;

public class SplitTunnelingWidgetViewModel : FeatureWidgetViewModelBase
{
    public override string Header => Localizer.Get("Settings_Features_SplitTunneling");

    public SplitTunnelingWidgetViewModel(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IMainViewNavigator mainViewNavigator)
        : base(localizer, logger, issueReporter, mainViewNavigator, settings, ConnectionFeature.SplitTunneling)
    { }

    protected override string GetFeatureStatus()
    {
        return Localizer.Get(
            Settings.IsSplitTunnelingEnabled
                ? Settings.SplitTunnelingMode switch
                {
                    SplitTunnelingMode.Standard => "Settings_Features_SplitTunneling_Standard",
                    SplitTunnelingMode.Inverse => "Settings_Features_SplitTunneling_Inverse",
                    _ => throw new ArgumentOutOfRangeException(nameof(ISettings.KillSwitchMode))
                }
                : "Common_States_Off");
    }

    protected override ImageSource GetFeatureIconSource()
    {
        return ResourceHelper.GetIllustration(
            Settings.IsSplitTunnelingEnabled
                ? "SplitTunnelingOnIllustrationSource"
                : "SplitTunnelingOffIllustrationSource");
    }

    protected override string GetFeatureToggleSettingName()
    {
        return nameof(Settings.IsSplitTunnelingEnabled);
    }
}