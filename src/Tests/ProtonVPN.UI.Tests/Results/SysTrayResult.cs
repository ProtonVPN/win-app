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

using System.Diagnostics;
using System.ServiceProcess;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Results
{
    internal class SysTrayResult : DesktopActions
    {
        public SysTrayResult CheckIfClientIsClosed()
        {
            Process[] pname = Process.GetProcessesByName("Proton VPN");
            Assert.That(pname.Length == 0, Is.True);
            ServiceController service = new ServiceController("ProtonVPN Service");
            Assert.That(service.Status, Is.Not.EqualTo(ServiceControllerStatus.Running));
            return this;
        }

        public SysTrayResult CheckIfClientIsRunning()
        {
            UIActions actions = new UIActions();
            actions.WaitUntilElementExistsByAutomationId("MenuHamburgerButton", TestData.MediumTimeout);
            return this;
        }

        public SysTrayResult WaitUntilConnected()
        {
            WaitUntilDisplayedByAutomationId("QuickLaunchFlag", TestData.MediumTimeout);
            return this;
        }

        public SysTrayResult WaitUntilDisconnected()
        {
            WaitUntilDisplayedByName("You are not protected!", TestData.MediumTimeout);
            return this;
        }
    }
}
