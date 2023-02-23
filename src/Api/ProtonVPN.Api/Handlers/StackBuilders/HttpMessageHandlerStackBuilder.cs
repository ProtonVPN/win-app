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
using System.Collections.Generic;
using System.Net.Http;

namespace ProtonVPN.Api.Handlers.StackBuilders
{
    public class HttpMessageHandlerStackBuilder
    {
        private readonly List<DelegatingHandler> _delegatingHandlers = new();

        public HttpMessageHandlerStackBuilder AddDelegatingHandler(DelegatingHandler delegatingHandler)
        {
            _delegatingHandlers.Add(delegatingHandler);
            return this;
        }

        public CompleteHttpMessageHandlerStackBuilder AddLastHandler(HttpMessageHandler httpMessageHandler)
        {
            if (httpMessageHandler is null)
            {
                throw new ArgumentNullException(nameof(httpMessageHandler));
            }
            if (httpMessageHandler is DelegatingHandler)
            {
                throw new ArgumentException($"The last handler cannot be of {nameof(DelegatingHandler)} " +
                    $"type as it would not have an InnerHandler defined.");
            }
            return new CompleteHttpMessageHandlerStackBuilder(httpMessageHandler, _delegatingHandlers);
        }
    }
}