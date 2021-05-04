/*
 * Copyright (c) 2021 Proton Technologies AG
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
using System.Linq;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Streaming
{
    internal class StreamingServices : IStreamingServices
    {
        private StreamingServicesResponse _response;
        private readonly IAppSettings _appSettings;

        public StreamingServices(StreamingServicesUpdater streamingServicesUpdater, IAppSettings appSettings)
        {
            streamingServicesUpdater.StreamingServicesUpdated += OnStreamingServicesUpdated;
            _appSettings = appSettings;
        }

        public IReadOnlyList<StreamingService> GetServices(string countryCode, sbyte tier)
        {
            if (_response == null ||
                !_response.StreamingServices.ContainsKey(countryCode) ||
                !_response.StreamingServices[countryCode].ContainsKey(tier))
            {
                return new List<StreamingService>();
            }

            IReadOnlyList<StreamingServiceResponse> services = _response.StreamingServices[countryCode][tier];

            return services.Select(s => new StreamingService(s.Name, GetIconUrl(s.Icon)))
                .OrderBy(s => s.Name)
                .ToList();
        }

        private string GetIconUrl(string icon)
        {
            return _appSettings.FeatureStreamingServicesLogosEnabled ? _response.ResourceBaseUrl + icon : null;
        }

        private void OnStreamingServicesUpdated(object sender, StreamingServicesResponse response)
        {
            _response = response;
        }
    }
}