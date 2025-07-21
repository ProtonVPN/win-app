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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Api.Contracts.HumanVerification;
using ProtonVPN.Api.Deserializers;

namespace ProtonVPN.Api.Handlers;

public class HumanVerificationHandler : HumanVerificationHandlerBase
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IBaseResponseMessageDeserializer _baseResponseDeserializer;
    private readonly IHumanVerifier _humanVerifier;
    private readonly bool _isEnabled;
    private string _error = string.Empty;

    public HumanVerificationHandler(IBaseResponseMessageDeserializer baseResponseDeserializer,
        IHumanVerifier humanVerifier,
        IHumanVerificationConfig humanVerificationConfig)
    {
        _baseResponseDeserializer = baseResponseDeserializer;
        _humanVerifier = humanVerifier;
        _isEnabled = humanVerificationConfig.IsSupported();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!_isEnabled)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        if (IsHumanVerificationInProgress())
        {
            return FailResponse.HumanVerificationFailureResponse(_error);
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        BaseResponse baseResponse = await _baseResponseDeserializer.DeserializeAsync(response, cancellationToken);
        if (baseResponse == null)
        {
            return response;
        }

        if (IsVerificationRequired(baseResponse))
        {
            _error = baseResponse.Error;

            HttpResponseMessage result = await HandleHumanVerificationAsync(request, cancellationToken,
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

    private bool IsHumanVerificationInProgress()
    {
        return _semaphore.CurrentCount == 0;
    }

    private bool IsHumanVerificationSupported(IReadOnlyList<string> humanVerificationMethods)
    {
        return humanVerificationMethods.Contains(HumanVerificationTypes.Captcha);
    }

    private async Task<HttpResponseMessage> HandleHumanVerificationAsync(HttpRequestMessage request,
        CancellationToken cancellationToken, string hvToken)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);

            string resolvedToken = await _humanVerifier.VerifyAsync(hvToken);
            if (!string.IsNullOrEmpty(resolvedToken))
            {
                request.Headers.Add("x-pm-human-verification-token-type", "captcha");
                request.Headers.Add("x-pm-human-verification-token", resolvedToken);

                return await base.SendAsync(request, cancellationToken);
            }

            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}