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

using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.Logic.Connection.ConnectionErrors;

public class SessionLimitReachedConnectionError : ReportableConnectionError
{
    private readonly ISettings _settings;

    public override string Message => Localizer.Get(
        _settings.VpnPlan.IsPaid
            ? "Connection_Error_SessionLimitReachedPaidUser"
            : "Connection_Error_SessionLimitReachedFreeUser");

    public SessionLimitReachedConnectionError(
        ILocalizationProvider localizer,
        ISettings settings,
        IReportIssueWindowActivator reportIssueWindowActivator) : base(localizer, reportIssueWindowActivator)
    {
        _settings = settings;
    }
}