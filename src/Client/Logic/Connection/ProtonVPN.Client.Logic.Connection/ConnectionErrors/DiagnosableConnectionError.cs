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

using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Localization.Contracts;

namespace ProtonVPN.Client.Logic.Connection.ConnectionErrors;

public abstract class DiagnosableConnectionError : ConnectionErrorBase
{
    private readonly IUrlsBrowser _urlsBrowser;

    public override string ActionLabel => Localizer.Get("Connection_Error_ViewPossibleSolutions");

    private string Url { get; }

    public DiagnosableConnectionError(
        ILocalizationProvider localizer,
        IUrlsBrowser urlsBrowser,
        string url) : base(localizer)
    {
        _urlsBrowser = urlsBrowser;

        Url = url;
    }

    public override Task ExecuteActionAsync()
    {
        _urlsBrowser.BrowseTo(Url);
        return Task.CompletedTask;
    }
}