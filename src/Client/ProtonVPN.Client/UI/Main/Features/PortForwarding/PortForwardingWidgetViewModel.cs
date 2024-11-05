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

using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Contracts.Services.Selection;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Main.Features.Bases;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Features.PortForwarding;

public partial class PortForwardingWidgetViewModel : FeatureWidgetViewModelBase
{
    private readonly IApplicationThemeSelector _applicationThemeSelector;
    private readonly IConnectionManager _connectionManager;
    private readonly IPortForwardingManager _portForwardingManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActivePortNumber))]
    private int? _activePortNumber;

    public bool HasActivePortNumber => ActivePortNumber.HasValue;

    public override string Header => Localizer.Get("Settings_Connection_PortForwarding");

    public PortForwardingWidgetViewModel(
        IApplicationThemeSelector applicationThemeSelector,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IMainViewNavigator mainViewNavigator,
        IConnectionManager connectionManager,
        IPortForwardingManager portForwardingManager)
        : base(localizer, logger, issueReporter, mainViewNavigator, settings, ConnectionFeature.PortForwarding)
    {
        _applicationThemeSelector = applicationThemeSelector;
        _connectionManager = connectionManager;
        _portForwardingManager = portForwardingManager;
    }

    protected override string GetFeatureStatus()
    {
        if (!_connectionManager.IsConnected || !Settings.IsPortForwardingEnabled)
        {
            return Localizer.GetToggleValue(Settings.IsPortForwardingEnabled);
        }

        return ActivePortNumber?.ToString()
            ?? (_portForwardingManager.IsFetchingPort
                ? Localizer.Get("Settings_Connection_PortForwarding_Loading")
                : Localizer.GetToggleValue(Settings.IsPortForwardingEnabled));
    }
}