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
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Main.Features.Bases;
using ProtonVPN.Client.UI.Main.Settings.Connection;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Features.PortForwarding;

public partial class PortForwardingWidgetViewModel : FeatureWidgetViewModelBase
{
    public override string Header => Localizer.Get("Settings_Connection_PortForwarding");

    public string InfoMessage => IsActivePortComponentVisible
        ? Localizer.Get("Flyouts_PortForwarding_ActivePort_Info")
        : Localizer.Get("Flyouts_PortForwarding_Info");

    public string WarningMessage => Localizer.Get("Flyouts_PortForwarding_Warning");

    public bool IsInfoMessageVisible => !ConnectionManager.IsConnected
                                     || !Settings.IsPortForwardingEnabled
                                     || DoesServerSupportP2P();

    public bool IsWarningMessageVisible => ConnectionManager.IsConnected
                                        && !DoesServerSupportP2P();

    public bool IsActivePortComponentVisible => ConnectionManager.IsConnected
                                             && Settings.IsPortForwardingEnabled;

    public PortForwardingWidgetViewModel(
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
               ConnectionFeature.PortForwarding)
    { }

    protected override IEnumerable<string> GetSettingsChangedForUpdate()
    {
        yield return nameof(ISettings.IsPortForwardingEnabled);
    }

    protected override string GetFeatureStatus()
    {
        return Localizer.GetToggleValue(Settings.IsPortForwardingEnabled);
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(WarningMessage));
    }

    protected override void OnSettingsChanged()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsWarningMessageVisible));
        OnPropertyChanged(nameof(IsActivePortComponentVisible));
    }

    protected override void OnConnectionStatusChanged()
    {
        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsWarningMessageVisible));
        OnPropertyChanged(nameof(IsActivePortComponentVisible));
    }

    protected override bool IsOnFeaturePage(PageViewModelBase? currentPageContext)
    {
        return currentPageContext is PortForwardingPageViewModel;
    }

    private bool DoesServerSupportP2P()
    {
        return ConnectionManager.IsConnected
            && ConnectionManager.CurrentConnectionDetails != null
            && ConnectionManager.CurrentConnectionDetails.IsP2P;
    }
}