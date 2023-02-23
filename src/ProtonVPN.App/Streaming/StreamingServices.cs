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
using System.Linq;
using ProtonVPN.Api.Contracts.Streaming;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Streaming
{
    internal class StreamingServices : IStreamingServices
    {
        private const string COUNTRY_CODE_ANY = "*";
        private StreamingServicesResponse _response;
        private readonly IAppSettings _appSettings;

        public StreamingServices(StreamingServicesUpdater streamingServicesUpdater, IAppSettings appSettings)
        {
            streamingServicesUpdater.StreamingServicesUpdated += OnStreamingServicesUpdated;
            _appSettings = appSettings;
        }

        public IReadOnlyList<StreamingService> GetServices(string countryCode, sbyte tier)
        {
            if (_response == null)
            {
                return new List<StreamingService>();
            }

            Dictionary<string, StreamingService> streamingServicesByName = new();
            if (IsStreamingServicesResponseContainingCountryCodeAndTier(countryCode, tier))
            {
                UpsertStreamingServicesToDictionary(streamingServicesByName, _response.StreamingServices[countryCode][tier]);
            }
            if (IsStreamingServicesResponseContainingCountryCodeAndTier(COUNTRY_CODE_ANY, tier))
            {
                UpsertStreamingServicesToDictionary(streamingServicesByName, _response.StreamingServices[COUNTRY_CODE_ANY][tier]);
            }

            return streamingServicesByName.Values.OrderBy(s => s.Name).ToList();
        }

        private bool IsStreamingServicesResponseContainingCountryCodeAndTier(string countryCode, sbyte tier)
        {
            return countryCode != null &&
                   _response.StreamingServices.ContainsKey(countryCode) &&
                   _response.StreamingServices[countryCode].ContainsKey(tier);
        }

        private void UpsertStreamingServicesToDictionary(Dictionary<string, StreamingService> streamingServicesByName,
            IList<StreamingServiceResponse> streamingServiceResponses)
        {
            foreach (StreamingServiceResponse streamingServiceResponse in streamingServiceResponses)
            {
                UpsertStreamingServiceToDictionary(streamingServicesByName, streamingServiceResponse);
            }
        }

        private void UpsertStreamingServiceToDictionary(Dictionary<string, StreamingService> streamingServicesByName,
            StreamingServiceResponse streamingServiceResponse)
        {
            StreamingService streamingService = MapStreamingService(streamingServiceResponse);
            streamingServicesByName[streamingService.Name] = streamingService;
        }

        private StreamingService MapStreamingService(StreamingServiceResponse streamingServicesResponse)
        {
            return new StreamingService(
                name: streamingServicesResponse.Name,
                iconUrl: GetIconUrl(streamingServicesResponse.Icon));
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