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

using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Exiting;
using ProtonVPN.ProcessCommunication.Client.Installers;

namespace ProtonVPN.Core
{
    public static class SingleInstanceApplication
    {
        private static Mutex _singleInstanceMutex;

        public static async Task<bool> InitializeAsFirstInstance(string uniqueName, string args)
        {
            _singleInstanceMutex = new Mutex(true, uniqueName, out bool firstInstance);

            if (!firstInstance)
            {
                await FirstAppInstanceCallerInitializer.OpenMainWindowAsync(args);
                AppKillInvoker.Kill();
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
    }
}