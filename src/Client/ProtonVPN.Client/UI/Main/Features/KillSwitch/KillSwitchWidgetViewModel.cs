/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.UI.Main.Features.Bases;
using ProtonVPN.Client.UI.Main.Settings.Connection;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Features.KillSwitch;

public class KillSwitchWidgetViewModel : FeatureWidgetViewModelBase
{
    public override string Header => Localizer.Get("Settings_Connection_KillSwitch");

    public string InfoMessage => Localizer.Get("Flyouts_KillSwitch_Info");

    public string WarningMessage => ConnectionManager.IsConnected
        ? Localizer.Get("Flyouts_KillSwitch_Warning_Connected")
        : Localizer.Get("Flyouts_KillSwitch_Warning_Disconnected");

    public string SuccessMessage => Settings.KillSwitchMode == KillSwitchMode.Advanced
        ? Localizer.Get("Flyouts_KillSwitch_Advanced_Success")
        : Localizer.Get("Flyouts_KillSwitch_Standard_Success");

    public bool IsInfoMessageVisible => !Settings.IsKillSwitchEnabled
                                     || (!ConnectionManager.IsConnected && Settings.KillSwitchMode == KillSwitchMode.Standard);

    public bool IsWarningMessageVisible => Settings.IsKillSwitchEnabled
                                        && Settings.KillSwitchMode == KillSwitchMode.Advanced;

    public bool IsSuccessMessageVisible => Settings.IsKillSwitchEnabled
                                        && (Settings.KillSwitchMode == KillSwitchMode.Advanced || ConnectionManager.IsConnected);

    public override bool IsRestricted => false;

    protected override UpsellFeatureType? UpsellFeature { get; } = null;

    public KillSwitchWidgetViewModel(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator)
        : base(localizer,
               logger,
               issueReporter,
               mainViewNavigator,
               settingsViewNavigator,
               settings,
               connectionManager,
               upsellCarouselWindowActivator,
               ConnectionFeature.KillSwitch)
    { }

    protected override IEnumerable<string> GetSettingsChangedForUpdate()
    {
        yield return nameof(ISettings.IsKillSwitchEnabled);
        yield return nameof(ISettings.KillSwitchMode);
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

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(WarningMessage));
        OnPropertyChanged(nameof(SuccessMessage));
    }

    protected override void OnSettingsChanged()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(SuccessMessage));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsWarningMessageVisible));
        OnPropertyChanged(nameof(IsSuccessMessageVisible));
    }

    protected override void OnConnectionStatusChanged()
    {
        OnPropertyChanged(nameof(WarningMessage));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsSuccessMessageVisible));
    }

    protected override bool IsOnFeaturePage(PageViewModelBase? currentPageContext)
    {
        return currentPageContext is KillSwitchPageViewModel;
    }
}