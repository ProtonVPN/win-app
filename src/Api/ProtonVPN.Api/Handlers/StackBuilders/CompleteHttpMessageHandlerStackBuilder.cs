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
using System.Linq;
using System.Net.Http;

namespace ProtonVPN.Api.Handlers.StackBuilders
{
    public class CompleteHttpMessageHandlerStackBuilder
    {
        private readonly HttpMessageHandler _httpMessageHandler;
        private readonly List<DelegatingHandler> _delegatingHandlers;

        public CompleteHttpMessageHandlerStackBuilder(HttpMessageHandler httpMessageHandler,
            List<DelegatingHandler> delegatingHandlers = null)
        {
            _httpMessageHandler = httpMessageHandler ?? throw new ArgumentNullException(nameof(httpMessageHandler));
            _delegatingHandlers = delegatingHandlers ?? new();
        }

        public HttpMessageHandler Build()
        {
            List<DelegatingHandler> delegatingHandlers = _delegatingHandlers.ToList();
            delegatingHandlers.Reverse();
            HttpMessageHandler innerHttpMessageHandler = _httpMessageHandler;

            foreach (DelegatingHandler delegatingHandler in delegatingHandlers)
            {
                delegatingHandler.InnerHandler = innerHttpMessageHandler;
                innerHttpMessageHandler = delegatingHandler;
            }

            return innerHttpMessageHandler;
        }
    }
}