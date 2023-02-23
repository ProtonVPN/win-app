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

using ProtonVPN.Api.Contracts;
using ProtonVPN.HumanVerification.Contracts;

namespace ProtonVPN.HumanVerification
{
    public class CaptchaUrlProvider : ICaptchaUrlProvider
    {
        private readonly IApiHostProvider _apiHostProvider;

        public CaptchaUrlProvider(IApiHostProvider apiHostProvider)
        {
            _apiHostProvider = apiHostProvider;
        }

        public string GetCaptchaUrl(string token)
        {
            return $"https://{_apiHostProvider.GetHost()}/core/v4/captcha?Token={token}";
        }
    }
}