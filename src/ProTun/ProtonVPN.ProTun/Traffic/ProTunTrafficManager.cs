/*
 * Copyright (c) 2026 Proton AG
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

using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.ProTun.Contracts;
using ProtonVPN.ProTun.Contracts.Traffic;

namespace ProtonVPN.ProTun.Traffic;

public class ProTunTrafficManager : IProTunTrafficManager
{
    private readonly ILogger _logger;
    private readonly IProTunManager _proTunManager;

    public ProTunTrafficManager(ILogger logger, IProTunManager proTunManager)
    {
        _logger = logger;
        _proTunManager = proTunManager;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _proTunManager.RequestStatsAsync();
                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellations are expected
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>($"Error occurred in {nameof(ProTunTrafficManager)}.", e);
        }
    }
}