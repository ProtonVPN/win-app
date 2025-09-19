/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;

namespace ProtonVPN.Client.Logic.Connection.ConnectionErrors;

public class UnknownConnectionError : ReportableConnectionError
{
    private VpnError _lastError;

    public override string Message => Localizer.GetFormat("Connection_Error_Unknown", (int)_lastError);

    public UnknownConnectionError(
        ILocalizationProvider localizer,
        IReportIssueWindowActivator reportIssueWindowActivator)
        : base(localizer, reportIssueWindowActivator)
    {
    }

    public void SetLastError(VpnError error)
    {
        _lastError = error;
    }
}