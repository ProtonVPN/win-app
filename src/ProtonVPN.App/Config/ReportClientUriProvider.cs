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

using System;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Config.Url;

namespace ProtonVPN.Config
{
    public class ReportClientUriProvider : IReportClientUriProvider
    {
        private readonly IActiveUrls _activeUrls;

        public ReportClientUriProvider(IActiveUrls activeUrls)
        {
            _activeUrls = activeUrls;
        }

        public Uri GetReportClientUri()
        {
            return _activeUrls.TlsReportUrl.Uri;
        }
    }
}