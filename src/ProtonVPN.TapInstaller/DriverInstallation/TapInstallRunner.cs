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

using TapInstaller.Extensions;
using TapInstaller.Logging;
using TapInstaller.Win32Processes;

namespace TapInstaller.DriverInstallation
{
    public class TapInstallRunner
    {
        private readonly ProcessRunner _processRunner;
        private readonly ILogger _logger;
        private readonly string _hwid;

        public TapInstallRunner(string hwid, ProcessRunner processRunner)
        {
            _hwid = hwid;
            _processRunner = processRunner;
            _logger = Logger.GetLogger();
        }

        public ProcessExecutionResult Status()
        {
            _logger.Log("TapInstallRunner: Retrieving driver status.");
            var result = _processRunner.Run($"status {_hwid}");
            _logger.Log($"TapInstallerRunner: Result: {result}");

            return result;
        }

        public bool IsInstalled()
        {
            _logger.Log("TapInstallRunner: Checking if driver is installed.");
            var result = _processRunner.Run($"hwids {_hwid}");
            _logger.Log($"TapInstallerRunner: Result: {result}");

            return result.Output.ContainsIgnoringCase(_hwid);
        }

        public InstallationResult Install()
        {
            _logger.Log("TapInstallRunner: Running installation.");
            var result = _processRunner.Run($"install OemVista.inf {_hwid}");
            _logger.Log($"TapInstallerRunner: Result: {result}");

            return new InstallationResult(result);
        }

        public InstallationResult Reinstall()
        {
            _logger.Log("TapInstallRunner: Reinstalling driver.");
            Remove();
            var result = Install();
            _logger.Log("TapInstallRunner: Reinstalling completed.");

            return result;
        }

        public InstallationResult Update()
        {
            _logger.Log("TapInstallRunner: Running update.");
            var result = _processRunner.Run($"update OemVista.inf {_hwid}");
            _logger.Log($"TapInstallerRunner: Result: {result}");

            return new InstallationResult(result);
        }

        public InstallationResult Remove()
        {
            _logger.Log("TapInstallRunner: Running remove.");
            var result = _processRunner.Run($"remove {_hwid}");
            _logger.Log($"TapInstallerRunner: Result: {result}");

            return new InstallationResult(result);
        }

        public ProcessExecutionResult Enable()
        {
            _logger.Log("TapInstallRunner: Running enable.");
            var result = _processRunner.Run($"enable {_hwid}");
            _logger.Log($"TapInstallerRunner: Result: {result}");

            return result;
        }
    }
}