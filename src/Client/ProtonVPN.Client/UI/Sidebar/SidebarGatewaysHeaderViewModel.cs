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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.UI.Sidebar.Bases;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Sidebar;

public class SidebarGatewaysHeaderViewModel : SidebarHeaderViewModelBase,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IServersLoader _serversLoader;

    public override string Header => Localizer.Get("Gateways_Page_Title");

    public override bool IsVisible => _serversLoader.GetGateways().Any();

    public override string AutomationId => "Sidebar_Gateways_Header";

    public SidebarGatewaysHeaderViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        IServersLoader serversLoader)
        : base(localizationProvider,
               logger,
               issueReporter)
    {
        _serversLoader = serversLoader;
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsVisible)));
    }
}