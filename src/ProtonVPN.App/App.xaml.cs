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

using System.Runtime;
using System.Windows;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Config;
using ProtonVPN.Core;
using ProtonVPN.Native.PInvoke;
using ProtonVPN.Windows;

namespace ProtonVPN
{
    public partial class App
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            if (await SingleInstanceApplication.InitializeAsFirstInstance("{588dc704-8eac-4a43-9345-ec7186b23f05}", string.Join(" ", e.Args)))
            {
                BalloonNotification.SetAppId("Proton.VPN");
                SetDllDirectories();

                IConfiguration config = GetConfig();
                CreateProfileOptimization(config);

                base.OnStartup(e);
            }
        }

        private static void CreateProfileOptimization(IConfiguration config)
        {
            ProfileOptimization.SetProfileRoot(config.LocalAppDataFolder);
            ProfileOptimization.StartProfile("Startup.profile");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Bootstrapper.OnExit();
            base.OnExit(e);
        }

        private static IConfiguration GetConfig()
        {
#if DEBUG
            const bool savingAllowed = true;
#else
            const bool savingAllowed = false;
#endif
            IConfiguration config = new ConfigFactory().Config(savingAllowed);
            new ConfigDirectories(config).Prepare();

            return config;
        }

        private static void SetDllDirectories()
        {
            Kernel32.SetDefaultDllDirectories(Kernel32.SetDefaultDllDirectoriesFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);
            Bootstrapper?.OnExit();
        }

        private Bootstrapper Bootstrapper => (Bootstrapper)Current.Resources["Bootstrapper"];
    }
}