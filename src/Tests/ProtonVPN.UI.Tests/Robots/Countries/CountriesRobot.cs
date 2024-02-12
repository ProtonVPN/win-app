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

using FlaUI.Core.AutomationElements;

namespace ProtonVPN.UI.Tests.Robots.Countries;

public partial class CountriesRobot : UIActions
{
    protected AutomationElement SecureCoreTab => ElementByAutomationId("Secure Core");

    protected Button GetConnectButton(string item)
    {
        return ElementByAutomationId($"Connect_to_{item}").AsButton();
    }

    protected Button GetNavigateToCountryButton(string countryCode)
    {
        return ElementByAutomationId($"Navigate_to_{countryCode}").AsButton();
    }

    protected Button GetShowServersButton(string city)
    {
        return ElementByAutomationId($"Show_servers_{city}").AsButton();
    }

    protected AutomationElement GetActiveConnectionDot(string item)
    {
        return ElementByAutomationId($"Active_connection_{item}").AsButton();
    }

    protected Button GetSecureCoreConnectButton(string entryCountry, string exitCountry)
    {
        return ElementByName($"{exitCountry} via {entryCountry}").AsButton();
    }

    public ServerConnectButton GetServerConnectButton()
    {
        Button button = ElementByName("ServerConnectButton").AsButton();
        string serverName = button.AutomationId.Replace("Connect_to_", "");
        string[] parts = serverName.Split('#');
        int.TryParse(parts[1], out int serverNumber);

        return new()
        {
            Button = button,
            Number = serverNumber,
            Name = serverName,
        };
    }
}