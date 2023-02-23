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

using System.Collections.Generic;

namespace ProtonVPN.Api.Handlers.TlsPinning
{
    public class CachingReportClient : IReportClient
    {
        private readonly List<string> _cache = new();

        private readonly IReportClient _origin;

        public CachingReportClient(IReportClient origin)
        {
            _origin = origin;
        }

        public void Send(ReportBody body)
        {
            string hash = body.Hash();
            if (_cache.Contains(hash))
            {
                return;
            }

            _cache.Add(hash);
            _origin.Send(body);
        }
    }
}