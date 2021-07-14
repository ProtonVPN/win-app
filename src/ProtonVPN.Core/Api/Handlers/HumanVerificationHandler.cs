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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Api.Extensions;
using ProtonVPN.Core.HumanVerification;

namespace ProtonVPN.Core.Api.Handlers
{
    public class HumanVerificationHandler : DelegatingHandler
    {
        private readonly IHumanVerifier _humanVerifier;
        private static readonly SemaphoreSlim Semaphore = new(1, 1);
        private string _error = string.Empty;
        private readonly bool _enabled;

        public HumanVerificationHandler(IHumanVerifier humanVerifier, bool enabled)
        {
            _humanVerifier = humanVerifier;
            _enabled = enabled;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!_enabled)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            if (IsVerifyInProgress())
            {
                return FailResponse.HumanVerificationFailureResponse(_error);
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            BaseResponse baseResponse = await response.GetBaseResponseMessage();
            if (baseResponse == null)
            {
                return response;
            }

            if (IsVerificationRequired(baseResponse))
            {
                _error = baseResponse.Error;

                HttpResponseMessage result = await HandleHumanVerification(request, cancellationToken,
                    baseResponse.Details.HumanVerificationToken);
                if (result != null)
                {
                    return result;
                }
            }

            return response;
        }

        private bool IsVerificationRequired(BaseResponse response)
        {
            return response.Code == ResponseCodes.HumanVerificationRequired &&
                   IsHumanVerificationSupported(response.Details.HumanVerificationMethods);
        }

        private bool IsVerifyInProgress()
        {
            return Semaphore.CurrentCount == 0;
        }

        private bool IsHumanVerificationSupported(IReadOnlyList<string> humanVerificationMethods)
        {
            return humanVerificationMethods.Contains(HumanVerificationTypes.Captcha);
        }

        private async Task<HttpResponseMessage> HandleHumanVerification(HttpRequestMessage request,
            CancellationToken cancellationToken, string hvToken)
        {
            try
            {
                await Semaphore.WaitAsync();

                bool? result = _humanVerifier.Verify(hvToken);
                if (result.HasValue && result.Value)
                {
                    string token = _humanVerifier.GetResolvedToken();
                    request.Headers.Add("x-pm-human-verification-token-type", "captcha");
                    request.Headers.Add("x-pm-human-verification-token", token);

                    return await base.SendAsync(request, cancellationToken);
                }

                return null;
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}