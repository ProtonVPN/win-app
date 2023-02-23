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
using System.Threading.Tasks;
using Polly.Timeout;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Translations;

namespace ProtonVPN.Account
{
    public class PromoCodeManager : IPromoCodeManager
    {
        private readonly IApiClient _apiClient;

        public PromoCodeManager(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<Result> ApplyPromoCodeAsync(string code)
        {
            try
            {
                PromoCodeRequest request = new() { Codes = new[] { code } };
                ApiResponseResult<BaseResponse> response = await _apiClient.ApplyPromoCodeAsync(request);
                return response.Success ? Result.Ok() : Result.Fail(response.Error);
            }
            catch (Exception e) when (e.InnerException is TimeoutRejectedException)
            {
                return Result.Fail(Translation.Get("Api_error_Timeout"));
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        }
    }
}