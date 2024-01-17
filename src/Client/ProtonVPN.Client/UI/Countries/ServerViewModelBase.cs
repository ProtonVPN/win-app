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
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Navigation;

namespace ProtonVPN.Client.UI.Countries;

public abstract class ServerViewModelBase : LocationViewModelBase
{
    public string Id { get; private set; }
    public string ExitCountryCode { get; private set; }
    public string ExitCountryName => Localizer.GetCountryName(ExitCountryCode);
    public string Name { get; private set; }
    public double Load { get; private set; }
    public string LoadPercent => $"{Load:P0}";
    public bool IsUnderMaintenance { get; private set; }

    public string ConnectButtonAutomationId => $"Connect_to_{Name}";
    public virtual string ConnectButtonAutomationName => ExitCountryName;
    public string ActiveConnectionAutomationId => $"Active_connection_{Name}";

    public override bool IsActiveConnection => ConnectionDetails != null
                                            && ConnectionDetails.ServerId == Id;

    protected ServerViewModelBase(
        ILocalizationProvider localizationProvider,
        IMainViewNavigator mainViewNavigator,
        IConnectionManager connectionManager)
        : base(localizationProvider, mainViewNavigator, connectionManager)
    { }

    public virtual void CopyPropertiesFromServer(Server server)
    {
        Id = server.Id;
        Name = server.Name;
        Load = server.Load / 100d;
        IsUnderMaintenance = server.IsUnderMaintenance();
        ExitCountryCode = server.ExitCountry;
    }
}