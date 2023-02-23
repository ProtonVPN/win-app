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

namespace ProtonVPN.Service
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.protonServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.protonServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // protonServiceProcessInstaller
            // 
            this.protonServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.protonServiceProcessInstaller.Password = null;
            this.protonServiceProcessInstaller.Username = null;
            // 
            // protonServiceInstaller
            // 
            this.protonServiceInstaller.DisplayName = "ProtonVpn Service";
            this.protonServiceInstaller.ServiceName = "ProtonVpn Service";
            this.protonServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.protonServiceProcessInstaller,
            this.protonServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller protonServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller protonServiceInstaller;
    }
}