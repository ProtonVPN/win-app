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

using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using ProtonVPN.Client.Bootstrapping;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Common.Core.Extensions;
using Windows.ApplicationModel.Activation;
using WinRT;

namespace ProtonVPN.Client;

public class Program
{
    private const string SINGLE_INSTANCE_APP_MUTEX_NAME = "{588dc704-8eac-4a43-9345-ec7186b23f05}";

    // This method should be async Task to be able to await IsFirstInstanceAsync,
    // but when changing to async Task, the xaml inspector tool is not able to find elements.
    // https://github.com/microsoft/microsoft-ui-xaml/issues/7385
    [STAThread]
    static void Main(string[] args)
    {
        // TODO: launch this process without admin privileges and /DoUninstallActions
        // command line argument during the uninstallation of the app
        if (args.ContainsIgnoringCase("/DoUninstallActions"))
        {
            UninstallActions.DeleteClientData();
            return;
        }

        ComWrappersSupport.InitializeComWrappers();

        bool isFirstInstance = IsFirstInstanceAsync().Result;
        if (isFirstInstance)
        {
            ProtocolActivationManager.Register();
            SetCurrentProcessExplicitAppUserModelID("Proton.VPN");

            Application.Start(_ =>
            {
                DispatcherQueueSynchronizationContext context = new(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });

            ProtocolActivationManager.Unregister();
        }
    }

    private static async Task<bool> IsFirstInstanceAsync()
    {
        if (IsLegacyAppRunning())
        {
            return false;
        }

        bool isFirstInstance = false;
        AppInstance keyInstance = AppInstance.FindOrRegisterForKey(App.APPLICATION_NAME);
        if (keyInstance.IsCurrent)
        {
            isFirstInstance = true;
            keyInstance.Activated += OnActivated;
        }
        else
        {
            AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
            await keyInstance.RedirectActivationToAsync(args);
        }

        return isFirstInstance;
    }

    private static bool IsLegacyAppRunning()
    {
        _ = new Mutex(true, SINGLE_INSTANCE_APP_MUTEX_NAME, out bool isFirstInstance);
        return !isFirstInstance;
    }

    private static void OnActivated(object? sender, AppActivationArguments args)
    {
        switch (args.Kind)
        {
            case ExtendedActivationKind.Launch:
                ActivateMainWindow();
                break;
            case ExtendedActivationKind.Protocol:
                ActivateMainWindow();
                if (args.Data is IProtocolActivatedEventArgs activationArgs)
                {
                    // TODO: use activationArgs.Uri to handle announcements and subscription change notifications
                }
                break;
        }
    }

    private static void ActivateMainWindow()
    {
        App.GetService<IUIThreadDispatcher>().TryEnqueue(() =>
        {
            IMainWindowActivator mainWindowActivator = App.GetService<IMainWindowActivator>();
            mainWindowActivator.Show();
        });
    }

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string appId);
}