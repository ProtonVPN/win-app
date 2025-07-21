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
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Api.Deserializers;

namespace ProtonVPN.Api.Tests.Deserializers
{
    public class MockOfBaseResponseMessageDeserializer : IBaseResponseMessageDeserializer
    {
        public BaseResponse ExpectedBaseResponse { get; set; }

        public Task<BaseResponse> DeserializeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return Task.FromResult(ExpectedBaseResponse);
        }
    }
}