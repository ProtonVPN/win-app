/*
 * Copyright (c) 2025 Proton AG
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
        string formattedRequest = GetFormattedRequest(request);
        try
        {
            LogHttpRequest(formattedRequest, request);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            LogHttpResponse(formattedRequest, response);

            return response;
        }
        catch (Exception ex)
        {
            LogHttpError(formattedRequest, ex);
            throw;
        }
    }

    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string formattedRequest = GetFormattedRequest(request);
        try
        {
            LogHttpRequest(formattedRequest, request);

            HttpResponseMessage response = base.Send(request, cancellationToken);

            LogHttpResponse(formattedRequest, response);

            return response;
        }
        catch (Exception ex)
        {
            LogHttpError(formattedRequest, ex);
            throw;
        }
    }

    private void LogHttpRequest(string formattedRequest, HttpRequestMessage request)
    {
        _logger.Info<ApiRequestLog>(formattedRequest);
#if DEBUG
        _logger.Debug<ApiRequestLog>($"{formattedRequest} request headers: {GetFormattedHeaders(request?.Headers)}");
#endif
    }

    private void LogHttpResponse(string formattedRequest, HttpResponseMessage response)
    {
        _logger.Info<ApiResponseLog>($"{formattedRequest}: {GetFormattedResponse(response)}");
#if DEBUG
        _logger.Debug<ApiResponseLog>($"{formattedRequest} response headers: {GetFormattedHeaders(response.Headers)}");
#endif
    }

    private void LogHttpError(string formattedRequest, Exception exception)
    {
        _logger.Error<ApiErrorLog>($"{formattedRequest} failed: {exception.CombinedMessage()}");
    }

    private string GetFormattedRequest(HttpRequestMessage request)
    {
        return request == null
            ? string.Empty
            : $"{request.Method.Method} \"{request.RequestUri}\"";
    }

    private string GetFormattedResponse(HttpResponseMessage response)
    {
        return response == null
            ? string.Empty
            : $"{(int)response?.StatusCode} {response?.StatusCode}\"";
    }

    private string GetFormattedHeaders(HttpHeaders headers)
    {
        return headers == null
            ? string.Empty
            : string.Join(',', headers.Select(kvp => $"{kvp.Key}: [{string.Join("],[", kvp.Value)}]"));
    }
}