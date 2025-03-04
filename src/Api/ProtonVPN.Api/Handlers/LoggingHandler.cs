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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Categories;
using ProtonVPN.Logging.Contracts.Events;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;

namespace ProtonVPN.Api.Handlers;

/// <summary>Logs all Http requests and responses.</summary>
public class LoggingHandler : LoggingHandlerBase
{
    private readonly ILogger _logger;

    public LoggingHandler(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string req = $"{request.Method.Method} \"{request.RequestUri}\"";
        try
        {
            _logger.Info<ApiRequestLog>(req);
#if DEBUG
            LogHttpHeaders<ApiRequestLog>($"{req} request", request.Headers);
#endif
            HttpResponseMessage result = await base.SendAsync(request, cancellationToken);
            _logger.Info<ApiResponseLog>($"{req}: {(int)result.StatusCode} {result.StatusCode}");
#if DEBUG
            LogHttpHeaders<ApiResponseLog>($"{req} response", result.Headers);
#endif
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error<ApiErrorLog>($"{req} failed: {ex.CombinedMessage()}");
            throw;
        }
    }

    private void LogHttpHeaders<T>(string req, HttpHeaders headers) where T : LogEventBase<ApiLogCategory>, new()
    {
        string mergedHeaders = string.Join(',', headers.Select(kvp => $"{kvp.Key}: [{string.Join("],[", kvp.Value)}]"));
        _logger.Debug<T>($"{req} headers: {mergedHeaders}");
    }
}