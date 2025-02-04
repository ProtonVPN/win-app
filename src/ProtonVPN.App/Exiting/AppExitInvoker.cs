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

using System.Windows;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Exiting
{
    public class AppExitInvoker : IAppExitInvoker
    {
        private const int APP_EXIT_TIMEOUT_IN_SECONDS = 3;

        private readonly ILogger _logger;
        private readonly IConfiguration _appConfig;
        private readonly IOsProcesses _osProcesses;

        public AppExitInvoker(ILogger logger, IConfiguration appConfig, IOsProcesses osProcesses)
        {
            _logger = logger;
            _appConfig = appConfig;
            _osProcesses = osProcesses;
        }

        public void Restart()
        {
            StartNewAppProcess();
            Close();
        }

        private void StartNewAppProcess()
        {
            _logger.Info<AppLog>($"Starting a new client process in {APP_EXIT_TIMEOUT_IN_SECONDS} seconds.");
            string cmd = $"/c Timeout /t {APP_EXIT_TIMEOUT_IN_SECONDS} >nul & \"{_appConfig.AppLauncherExePath}\"";
            using IOsProcess process = _osProcesses.CommandLineProcess(cmd);
            process.Start();
        }

        public void Close()
        {
            _logger.Info<AppLog>("Shutting down the app.");
            Application.Current.Shutdown();
        }

        public void Kill(int code = 0)
        {
            AppKillInvoker.Kill(code);
        }
    }
}