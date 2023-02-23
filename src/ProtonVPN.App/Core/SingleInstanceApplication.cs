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
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ProtonVPN.Account;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.ServiceModel.Server;
using ProtonVPN.Core.Service;
using ProtonVPN.Login.Views;
using ProtonVPN.Windows;
using WinApplication = System.Windows.Application;

namespace ProtonVPN.Core
{
    internal static class SingleInstanceApplication
    {
        private static Mutex _singleInstanceMutex;
        private static ServiceHost _host;

        public static async Task<bool> InitializeAsFirstInstance(string uniqueName, string[] args)
        {
            _singleInstanceMutex = new Mutex(true, uniqueName, out bool firstInstance);

            try
            {
                if (firstInstance)
                {
                    _host = new InprocHostFactory().Create<ApplicationProxy>("protonvpn-app/initialization");
                    _host.Open(TimeSpan.FromSeconds(10));
                }
                else
                {
                    using ServiceChannel<IApplicationProxy> channel = new ServiceChannelFactory().Create<IApplicationProxy>("protonvpn-app/initialization");
                    await channel.Proxy.InvokeRunningInstance(args);
                }
            }
            catch (CommunicationException)
            {
            }
            catch (TimeoutException)
            {
            }

            return firstInstance;
        }

        public static void ReleaseSingleInstanceLock()
        {
            if (_singleInstanceMutex != null)
            {
                _singleInstanceMutex.Close();
                _singleInstanceMutex = null;
            }
        }

        [ServiceContract]
        private interface IApplicationProxy
        {
            [OperationContract(IsOneWay = true)]
            Task InvokeRunningInstance(IList<string> args);
        }

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
        private class ApplicationProxy : IApplicationProxy
        {
            public async Task InvokeRunningInstance(IList<string> args)
            {
                if (WinApplication.Current != null)
                {
                    await WinApplication.Current.Dispatcher.InvokeAsync(() => ActivateFirstInstance(args));
                }
            }

            private static void ActivateFirstInstance(IList<string> args)
            {
                if (WinApplication.Current != null)
                {
                    Window window = ((App)WinApplication.Current).MainWindow;
                    if (window is AppWindow or LoginWindow && window.IsLoaded)
                    {
                        window.WindowState = WindowState.Minimized;
                        window.Show();
                        window.WindowState = WindowState.Normal;
                        window.Activate();

                        if (window is AppWindow appWindow && args.Count > 0 &&
                            args[0].ContainsIgnoringCase(SubscriptionManager.REDIRECT_ENDPOINT))
                        {
                            appWindow.TriggerAccountInfoUpdate();
                        }
                    }
                }
            }
        }
    }
}