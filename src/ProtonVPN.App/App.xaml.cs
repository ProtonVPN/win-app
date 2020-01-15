/*
 * Copyright (c) 2020 Proton Technologies AG
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Threading.Tasks;
using System.Windows;
using ProtonVPN.Common.Cli;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.CrashReporting;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Config;
using ProtonVPN.Core;
using ProtonVPN.Native.PInvoke;

namespace ProtonVPN
{
    public partial class App
    {
        private static Bootstrapper _bootstrapper;
        private static readonly List<string> FailedLibs = new List<string>();

        [STAThread]
        public static void Main(string[] args)
        {
            Run(args).GetAwaiter().GetResult();
        }

        private static async Task Run(string[] args)
        {
            if (await SingleInstanceApplication.InitializeAsFirstInstance("{588dc704-8eac-4a43-9345-ec7186b23f05}", args))
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyLoadFailed;

                SetDllDirectories();

                var config = GetConfig();

                InitCrashReporting(config);
                CreateProfileOptimization(config);

                var app = new App();
                app.InitializeComponent();
                
                _bootstrapper = new Bootstrapper();
                _bootstrapper.Initialize();

                HandleIntentionalCrash(app, args);

                app.Run();
            }
        }

        private static void InitCrashReporting(Common.Configuration.Config config)
        {
            CrashReports.Init(config);
        }

        private static Assembly OnAssemblyLoadFailed(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            if (name.ContainsIgnoringCase(".resources") ||
                name.EndsWithIgnoringCase("XmlSerializers") ||
                name.StartsWithIgnoringCase("PresentationFramework.")
                )
            {
                return null;
            }

#if DEBUG
            if (name.StartsWithIgnoringCase("System.Windows"))
            {
                return null;
            }
#endif

            if (!FailedLibs.Contains(name))
            {
                Process.Start("ProtonVPN.ErrorMessage.exe", $"\"The application is missing required file\" \"{args.Name}\"");
                FailedLibs.Add(name);
            }

            return null;
        }

        private static void CreateProfileOptimization(Common.Configuration.Config config)
        {
            ProfileOptimization.SetProfileRoot(config.LocalAppDataFolder);
            ProfileOptimization.StartProfile("Startup.profile");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _bootstrapper.OnExit();
            base.OnExit(e);
        }

        private static Common.Configuration.Config GetConfig()
        {
#if DEBUG
            const bool savingAllowed = true;
#else
            const bool savingAllowed = false;
#endif
            var config = new ConfigFactory().Config(savingAllowed);
            new ConfigDirectories(config).Prepare();

            return config;
        }

        private static void HandleIntentionalCrash(Application app, string[] args)
        {
            var option = new CommandLineOption("crash", args);
            if (!option.Exists())
                return;

            app.Deactivated += (sender, ea) => throw new StackOverflowException("Intentional crash test");
        }

        private static void SetDllDirectories()
        {
            Kernel32.SetDefaultDllDirectories(Kernel32.SetDefaultDllDirectoriesFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
        }
    }
}
