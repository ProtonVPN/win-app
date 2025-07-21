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
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Api.Handlers;

/// <summary>
/// Cancels all not finished requests on user logout.
/// </summary>
public class CancellingHandler : CancellingHandlerBase
{
    private readonly IConfiguration _config;
    private readonly IUserSession _userSession;
    private readonly CancellationHandle _cancellationHandle = new();

    public CancellingHandler(IConfiguration config, IUserSession userSession)
    {
        _config = config;
        _userSession = userSession;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (IsLogout(request))
        {
            _cancellationHandle.Cancel();
        }

        return await base.SendAsync(request, _userSession.IsLoggedIn ? _cancellationHandle.Token : cancellationToken);
    }

    private bool IsLogout(HttpRequestMessage request)
    {
        string endpoint = request.RequestUri.AbsoluteUri.Replace(_config.Urls.ApiUrl, "");
        return request.Method == HttpMethod.Delete && endpoint == "/auth";
    }
}