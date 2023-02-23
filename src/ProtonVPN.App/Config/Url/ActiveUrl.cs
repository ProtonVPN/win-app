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

using ProtonVPN.Common.OS.Processes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace ProtonVPN.Config.Url
{
    public class ActiveUrl : IActiveUrl
    {
        private readonly IOsProcesses _processes;
        private string _url;

        public ActiveUrl(IOsProcesses processes, string url)
        {
            _processes = processes;
            _url = url;
        }

        public ActiveUrl WithQueryParams(Dictionary<string, string> parameters)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder(_url);
                NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query);
                foreach (KeyValuePair<string, string> item in parameters)
                {
                    query[item.Key] = item.Value;
                }

                uriBuilder.Query = query.ToString();
                _url = uriBuilder.ToString();
            }
            catch (UriFormatException)
            {
                // ignore
            }

            return this;
        }

        public Uri Uri => new(_url);

        public void Open()
        {
            if (string.IsNullOrEmpty(_url))
            {
                return;
            }

            _processes.Open(_url);
        }
    }
}