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
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Api.Deserializers;

namespace ProtonVPN.Api.Handlers
{
    /// <summary>
    /// Detects outdated app by checking response code.
    /// </summary>
    public class OutdatedAppHandler : DelegatingHandler
    {
        private readonly IBaseResponseMessageDeserializer _baseResponseDeserializer;
        private readonly IOutdatedClientNotifier _outdatedClientNotifier;

        public OutdatedAppHandler(IBaseResponseMessageDeserializer baseResponseDeserializer, IOutdatedClientNotifier outdatedClientNotifier)
        {
            _baseResponseDeserializer = baseResponseDeserializer;
            _outdatedClientNotifier = outdatedClientNotifier;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            BaseResponse baseResponse = await _baseResponseDeserializer.DeserializeAsync(response, cancellationToken);
            if (baseResponse == null)
            {
                return response;
            }

            if (ForceLogoutRequired(baseResponse.Code))
            {
                _outdatedClientNotifier.OnClientOutdated();
            }

            return response;
        }

        private bool ForceLogoutRequired(int code)
        {
            return code is ResponseCodes.OutdatedApiResponse or ResponseCodes.OutdatedAppResponse;
        }
    }
}