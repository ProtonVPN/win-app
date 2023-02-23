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

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Configuration;

namespace ProtonVPN.App.Tests.BugReporting.Diagnostic
{
    public class BaseLogTest
    {
        protected const string TmpPath = "BugReporting\\Diagnostic\\Tmp";
        protected IConfiguration Config { get; private set; }

        [TestInitialize]
        public virtual void Initialize()
        {
            if (Directory.Exists(TmpPath))
            {
                Directory.Delete(TmpPath, true);
            }

            Directory.CreateDirectory(TmpPath);
            Config = new Common.Configuration.Config() { DiagnosticsLogFolder = TmpPath };
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            Config = null;
        }
    }
}
