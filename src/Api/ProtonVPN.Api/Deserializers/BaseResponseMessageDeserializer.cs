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

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;

namespace ProtonVPN.Api.Deserializers
{
    public class BaseResponseMessageDeserializer : IBaseResponseMessageDeserializer
    {
        private readonly ILogger _logger;

        public BaseResponseMessageDeserializer(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<BaseResponse> DeserializeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }
            
            try
            {
                return JsonConvert.DeserializeObject<BaseResponse>(content);
            }
            catch (JsonException e)
            {
                _logger.Error<ApiLog>("Failed to deserialize base response message from " +
                    $"{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}.", e);
                return null;
            }
        }
    }
}