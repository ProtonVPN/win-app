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
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Countries;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Gateways.Items;

public class B2BServerViewModel : ServerViewModelBase
{
    public string GatewayName { get; private set; } = string.Empty;

    protected override ConnectionIntent ConnectionIntent =>
        new(new GatewayServerLocationIntent(Id, Name, ExitCountryCode, GatewayName), new B2BFeatureIntent());

    protected override ModalSources UpsellModalSources => ModalSources.Undefined;

    public B2BServerViewModel(
        ILocalizationProvider localizationProvider,
        IMainViewNavigator mainViewNavigator,
        IConnectionManager connectionManager,
        ILogger logger,
        IIssueReporter issueReporter,
        IWebAuthenticator webAuthenticator,
        ISettings settings,
        IUrls urls) :
        base(localizationProvider, mainViewNavigator, connectionManager, logger, issueReporter, webAuthenticator, settings, urls)
    { }

    public override void CopyPropertiesFromServer(Server server)
    {
        base.CopyPropertiesFromServer(server);

        GatewayName = server.GatewayName;
    }
}