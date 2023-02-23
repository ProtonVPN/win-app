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

using System.Linq;
using TapInstaller.Logging;
using TapInstaller.Win32Processes;

namespace TapInstaller.DriverInstallation
{
    public class TapAdapterInstaller
    {
        private readonly TapInstallRunner _tapInstallRunner;
        private readonly TapInstallerStandardOutputParser _outputParser;
        private readonly ILogger _logger;

        public TapAdapterInstaller(TapInstallRunner tapInstallRunner, TapInstallerStandardOutputParser outputParser)
        {
            _tapInstallRunner = tapInstallRunner;
            _outputParser = outputParser;
            _logger = Logger.GetLogger();
        }

        public SetupResult Install()
        {
            _logger.Log($"TapAdapterInstaller: Starting installing driver.");

            var result = InstalOrUpdateTapAdapter();
            var status = _tapInstallRunner.Status();

            if (result == SetupResult.Failure)
                return ProcessDeviceErrorCode(status);

            if (IsDisabled(status))
                _tapInstallRunner.Enable();
            
            return result;
        }

        public SetupResult Uninstall()
        {
            if (!_tapInstallRunner.IsInstalled())
                return SetupResult.Success;

            _tapInstallRunner.Remove();

            return SetupResult.Success;
        }

        private SetupResult InstalOrUpdateTapAdapter()
        {
            if (_tapInstallRunner.IsInstalled())
            {
                return _tapInstallRunner.Remove().Success ? InstallInner() : SetupResult.Failure;
            }

            return InstallInner();
        }

        private SetupResult InstallInner()
        {
            var result = _tapInstallRunner.Install();
            if (result.Success)
            {
                return SetupResult.Success;
            }

            if (result.RestartRequired)
            {
                return SetupResult.RestartRequired;
            }

            return SetupResult.Failure;
        }

        private SetupResult ProcessDeviceErrorCode(ProcessExecutionResult status)
        {
            if (IsHealthyAndRunning(status))
                return SetupResult.Success;

            if (IsDriverUpdateRequired(status))
            {
                _tapInstallRunner.Update();
                return SetupResult.Success;
            }

            if (IsDriverReinstallationRequired(status) && IsPcRestartRequired(status))
            {
                _logger.Log("TapAdapterInstaller: Driver is reinstalling and requesting restart.");
                _tapInstallRunner.Reinstall();

                return SetupResult.RestartRequired;
            }

            if (IsDriverReinstallationRequired(status))
            {
                _logger.Log("TapAdapterInstaller: Driver is reinstalling.");
                _tapInstallRunner.Reinstall();

                return SetupResult.Success;
            }

            _logger.Log($"TapAdapterInstaller: Installation failed. Status: {status}");
            return SetupResult.Failure;
        }

        private bool IsPcRestartRequired(ProcessExecutionResult result)
        {
            var code = ProblemCode(result);
            return code.HasValue && new[] { 3, 14, 21, 28, 33, 38, 42, 44, 47, 54 }.Any(i => i == code.Value);
        }

        private bool IsDriverUpdateRequired(ProcessExecutionResult result)
        {
            var code = ProblemCode(result);
            return code.HasValue && new[] { 1, 10, 18, 24, 31, 41, 48, 52 }.Any(i => i == code.Value);
        }

        private bool IsDriverReinstallationRequired(ProcessExecutionResult result)
        {
            var code = ProblemCode(result);
            return code.HasValue && new[] {3, 18, 19, 28, 32, 37, 39, 40}.Any(i => i == code.Value);
        }

        private bool IsHealthyAndRunning(ProcessExecutionResult result)
        {
            var status = _outputParser.ParseInstallerStatus(result.Output);

            return status == DriverState.DeviceExists &&
                   status != DriverState.DeviceIsDisabled &&
                   status != DriverState.DeviceIsStopped &&
                   status != DriverState.DeviceHasAProblem;
        }

        private bool IsDisabled(ProcessExecutionResult result)
        {
            return _outputParser.ParseInstallerStatus(result.Output) == DriverState.DeviceIsDisabled;
        }

        private int? ProblemCode(ProcessExecutionResult result)
        {
            return _outputParser.ParseDeviceCode(result.Output);
        }
    }
}
