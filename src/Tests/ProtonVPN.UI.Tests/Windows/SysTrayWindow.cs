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
using System.Threading;
using FlaUI.Core.AutomationElements;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.Windows;

namespace ProtonVPN.UI.Tests.Windows
{
    internal class SysTrayWindow : DesktopActions
    {
        private Button NotificationChevronButton => ElementByName("Notification Chevron").AsButton();
        private Button TrayLogo => ElementByName(TrayIcon.TRAY_ICON_NAME).AsButton();
        private Button QuickConnectElement => ElementByAutomationId("QuickConnect").AsButton();
        private Button OpenProtonVpnButton => ElementByName("Open Proton VPN").AsButton();
        private AutomationElement ExitOption => ElementByName("Exit");
        private Button ProfilesChevron => ElementByAutomationId("TogglePopupButton").AsButton();
        private AutomationElement RandomProfile => ElementByName("Random");

        public SysTrayWindow OpenSysTrayWindow()
        {
            ClickOnNotificationChevron();
            TrayLogo.Click();
            return this;
        }

        public SysTrayWindow RightClickTrayIcon()
        {
            ClickOnNotificationChevron();
            TrayLogo.RightClick();
            return this;
        }

        public SysTrayWindow ExitTheAppViaTray()
        {
            ExitOption.Click();
            return this;
        }

        public SysTrayWindow QuickConnect()
        {
            QuickConnectElement.Invoke();
            return this;
        }

        public SysTrayWindow OpenProtonVpn()
        {
            OpenProtonVpnButton.Invoke();
            return this;
        }

        public SysTrayWindow ConnectToRandomProfile()
        {
            ProfilesChevron.Click();
            RandomProfile.Click();
            return this;
        }

        public SysTrayWindow CloseActiveNotification()
        {
            try
            {
                RefreshDesktop();
                Desktop.FindFirstDescendant(cf => cf.ByAutomationId("DismissButton")).AsButton().Invoke();
            }
            catch (NullReferenceException)
            {
                //Do nothing because notification might not be shown
            }
            return this;
        }

        public SysTrayWindow ClickOnTaskBar()
        {
            if (!TestEnvironment.IsWindows11())
            {
                ElementByClassName("Shell_TrayWnd").Click();
            }
            else
            {
                ElementByAutomationId("TaskbarFrame").Click();
            }
            return this;
        }

        private void ClickOnNotificationChevron()
        {
            Thread.Sleep(3000);
            NotificationChevronButton.Click();
        }
    }
}
