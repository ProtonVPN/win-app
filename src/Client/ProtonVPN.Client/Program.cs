/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Client.Services.Bootstrapping.Helpers;

namespace ProtonVPN.Client;

public class Program
{
    private const string SINGLE_INSTANCE_APP_MUTEX_NAME = "{588dc704-8eac-4a43-9345-ec7186b23f05}";
    private const string APP_USER_MODEL_ID = "Proton.VPN";

    private static Mutex? _mutex; // The variable is kept to hold the Mutex lock

    // This method should be async Task to be able to await IsFirstInstanceAsync,
    // but when changing to async Task, the xaml inspector tool is not able to find elements.
    // https://github.com/microsoft/microsoft-ui-xaml/issues/7385
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.ContainsIgnoringCase("-DoUninstallActions"))
        {
            UninstallActions.DeleteClientData();
            UninstallActions.DeleteRegistryKeys(APP_USER_MODEL_ID, App.APPLICATION_NAME);
            return;
        }

        if (IsFirstInstance())
        {
            SetCurrentProcessExplicitAppUserModelID(APP_USER_MODEL_ID);

            Application.Start(_ =>
            {
                DispatcherQueueSynchronizationContext context = new(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });
        }
        else
        {
            AppInstanceHelper.RedirectActivation();
        }
    }

    private static bool IsFirstInstance()
    {
        _mutex = new Mutex(true, SINGLE_INSTANCE_APP_MUTEX_NAME, out bool isFirstInstance);
        return isFirstInstance;
    }

    public static void ReleaseMutex()
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
    }

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string appId);
}