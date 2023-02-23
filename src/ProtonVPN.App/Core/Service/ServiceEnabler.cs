/* Copyright (c) 2023 Proton AG
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

using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppServiceLogs;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Core.Modals;
using ProtonVPN.Modals;

namespace ProtonVPN.Core.Service
{
    public class ServiceEnabler : IServiceEnabler
    {
        private readonly ILogger _logger;
        private readonly IModals _modals;
        private readonly IAppExitInvoker _appExitInvoker;

        public ServiceEnabler(ILogger logger, IModals modals, IAppExitInvoker appExitInvoker)
        {
            _logger = logger;
            _modals = modals;
            _appExitInvoker = appExitInvoker;
        }

        public Result GetServiceEnabledResult(IService service)
        {
            if (!service.Enabled())
            {
                _logger.Info<AppServiceLog>($"Service {service.Name} is disabled. Displaying modal to enable it.");
                bool? result = _modals.Show<DisabledServiceModalViewModel>();
                if (!result.HasValue || !result.Value)
                {
                    _logger.Info<AppServiceLog>($"The user refused to enable service {service.Name}. Shutting down the application.");
                    _appExitInvoker.Exit();
                    return Result.Fail();
                }

                _logger.Info<AppServiceLog>($"The user requested to enable service {service.Name}");
                service.Enable();
            }

            return service.Enabled() ? Result.Ok() : Result.Fail();
        }
    }
}