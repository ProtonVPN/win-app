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

using System;
using System.ServiceModel;
using System.Threading.Tasks;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppServiceLogs;
using ProtonVPN.Common.OS.Services;

namespace ProtonVPN.Core.Service
{
    public class SafeServiceAction : ConcurrentService, ISafeServiceAction
    {
        private readonly ILogger _logger;
        private readonly IServiceEnabler _serviceEnabler;

        public SafeServiceAction(IService service, ILogger logger, IServiceEnabler serviceEnabler) : base(service,
            serviceEnabler)
        {
            _logger = logger;
            _serviceEnabler = serviceEnabler;
        }

        public async Task<Result> InvokeServiceAction(Func<Task<Result>> action)
        {
            if (!Service.Exists())
            {
                _logger.Info<AppServiceLog>($"Failed to execute action on nonexistent service {Service.Name}.");
                return Result.Fail();
            }

            Result serviceEnableResult = _serviceEnabler.GetServiceEnabledResult(Service);
            if (serviceEnableResult.Failure)
            {
                return serviceEnableResult;
            }

            return await ExecuteAction(action);
        }

        private async Task<Result> ExecuteAction(Func<Task<Result>> action)
        {
            try
            {
                return await action();
            }
            catch (EndpointNotFoundException)
            {
                _logger.Info<AppServiceStartLog>($"Service {Service.Name} is not running. Trying to start it.");
                Result serviceStartResult = await Service.StartAsync(new());
                if (serviceStartResult.Failure)
                {
                    _logger.Info<AppServiceStartFailedLog>($"Failed to start service ${Service.Name} after EndpointNotFoundException.");
                    return serviceStartResult;
                }

                return await InvokeServiceAction(action);
            }
            catch (Exception e) when (IsConnectionException(e))
            {
                _logger.Error<AppServiceLog>("Failed to execute service action.", e);
                return Result.Fail(e);
            }
        }

        private bool IsConnectionException(Exception ex) => ex is CommunicationException or TimeoutException;
    }
}