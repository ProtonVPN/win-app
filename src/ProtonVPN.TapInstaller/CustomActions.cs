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
using System.IO;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using TapInstaller.DriverInstallation;
using TapInstaller.Logging;
using TapInstaller.Win32Processes;

namespace TapInstaller
{
    public static class CustomActions
    {
        [CustomAction]
        public static ActionResult InstallTapAdapter(Session session)
        {
            var logger = BuildLogger(session);
            Logger.EnableLogging(logger);

            try
            {
                logger.Log("TapInstaller: Starting installation of tap adapter");
                var installer = BuildInstaller(session, logger);

                return Install(installer, session, logger);
            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult UninstallTapAdapter(Session session)
        {
            var logger = BuildLogger(session);
            Logger.EnableLogging(logger);

            try
            {
                var installer = BuildInstaller(session, logger);
                installer.Uninstall();

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());
                return ActionResult.Failure;
            }
        }

        private static ActionResult Install(TapAdapterInstaller installer, Session session, ILogger logger)
        {
            var result = installer.Install();
            if (result == SetupResult.Success)
            {
                return ActionResult.Success;
            }

            if (result == SetupResult.RestartRequired)
            {
                logger.Log("Install: Restart required afted driver installation.");
                session["REBOOT"] = "Force";
                return ActionResult.Success;
            }

            logger.Log("TapInstaller: Installation failed.");
            return ActionResult.Failure;
        }

        private static TapAdapterInstaller BuildInstaller(Session session, ILogger logger)
        {
            string tapPath;
            using (var key = Registry.LocalMachine.OpenSubKey("Software\\ProtonVPN\\TAP-ProtonVPN"))
            {
                tapPath = key?.GetValue("Path").ToString();
                logger.Log($"Install: Drivers path from registry: {tapPath}");
            }

            if (string.IsNullOrWhiteSpace(tapPath))
                tapPath = Path.Combine(session["ProgramFilesFolder"], "Proton Technologies", "ProtonVPNTap");

            var pathResolver = new PathResolver(tapPath);

            var tapInstallRunner = new TapInstallRunner(
                "tapprotonvpn",
                new ProcessRunner(pathResolver.GetTapinstallPath(), pathResolver.GetDriversDir()));

            return new TapAdapterInstaller(tapInstallRunner, new TapInstallerStandardOutputParser());
        }

        private static ILogger BuildLogger(Session session)
        {
            return new InstallerSessionLogger(session);
        }
    }
}
