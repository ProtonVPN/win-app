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

using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Gateways.Items;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Gateways.Factories;

public class GatewayViewModelsFactory
{
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly IOverlayActivator _overlayActivator;
    private readonly ILocalizationProvider _localizer;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger _logger;
    private readonly IIssueReporter _issueReporter;

    public GatewayViewModelsFactory(
        ILocalizationProvider localizer,
        IMainViewNavigator mainViewNavigator,
        IOverlayActivator overlayActivator,
        IConnectionManager connectionManager,
        ILogger logger,
        IIssueReporter issueReporter)
    {
        _mainViewNavigator = mainViewNavigator;
        _overlayActivator = overlayActivator;
        _localizer = localizer;
        _connectionManager = connectionManager;
        _logger = logger;
        _issueReporter = issueReporter;
    }

    public GatewayViewModel GetGatewayViewModel(string gatewayName, List<Server> servers)
    {
        List<B2BServerViewModel> gatewayServers = servers.Select(GetB2BServerViewModel).ToList();

        return new GatewayViewModel(_localizer, _mainViewNavigator, _connectionManager, _overlayActivator, _logger, _issueReporter)
        {
            GatewayName = gatewayName,
            Servers = gatewayServers,
            IsUnderMaintenance = gatewayServers.All(s => s.IsUnderMaintenance),
        };
    }

    public B2BServerViewModel GetB2BServerViewModel(Server server)
    {
        B2BServerViewModel serverViewModel = new(_localizer, _mainViewNavigator, _connectionManager, _logger, _issueReporter);

        serverViewModel.CopyPropertiesFromServer(server);

        return serverViewModel;
    }
}