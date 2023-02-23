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
using Microsoft.Web.WebView2.Core;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.HumanVerification.Contracts;

namespace ProtonVPN.HumanVerification
{
    public class HumanVerificationConfig : IHumanVerificationConfig
    {
        private readonly ILogger _logger;

        public HumanVerificationConfig(ILogger logger)
        {
            _logger = logger;
        }
        
        public bool IsSupported()
        {
            try
            {
                return !string.IsNullOrEmpty(CoreWebView2Environment.GetAvailableBrowserVersionString());
            }
            catch (Exception e)
            {
                if (e is not (WebView2RuntimeNotFoundException or DllNotFoundException))
                {
                    _logger.Error<AppLog>("Unexpected exception when checking for WebView support.", e);
                }

                return false;
            }
        }
    }
}