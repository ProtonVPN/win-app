/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Client.Contracts.Services.Lifecycle;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.OperatingSystems.Processes.Contracts;

namespace ProtonVPN.Client.Services.Lifecycle;

public class AppExitInvoker : IAppExitInvoker
{
    private const int APP_EXIT_TIMEOUT_IN_SECONDS = 3;

    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly ICommandLineCaller _commandLineCaller;

    public AppExitInvoker(ILogger logger, IConfiguration config, ICommandLineCaller commandLineCaller)
    {
        _logger = logger;
        _config = config;
        _commandLineCaller = commandLineCaller;
    }

    public void Restart()
    {
        StartNewAppProcess();
        Exit();
    }

    private void StartNewAppProcess()
    {
        _logger.Info<AppLog>($"Starting a new client process in {APP_EXIT_TIMEOUT_IN_SECONDS} seconds.");
        string cmd = $"/c Timeout /t {APP_EXIT_TIMEOUT_IN_SECONDS} >nul & \"{_config.ClientLauncherExePath}\"";
        _commandLineCaller.Execute(cmd);
    }

    public void Exit(int code = 0)
    {
        _logger.Info<AppStopLog>(code == 0 ? "Exiting the app" : $"Exiting the app with code {code}");
        AppKillInvoker.Kill(code);
    }
}