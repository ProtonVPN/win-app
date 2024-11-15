/*
 * Copyright (c) 2024 Proton AG
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
using System.Linq;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests;

[SetUpFixture]
class SetUpFixture
{
    [OneTimeSetUp]
    public void GlobalOneTimeSetup()
    {
        KillProtonVPNClient();
        ArtifactsHelper.CreateTestFailureFolderIfNotExists();
    }

    [OneTimeTearDown]
    public void GlobalOneTimeTearDown()
    {
        KillProtonVPNClient();
    }

    private static void KillProtonVPNClient()
    {
        Process.GetProcesses()
            .Where(process => process.ProcessName.StartsWith("ProtonVPN"))
            .ToList()
            .ForEach(process =>
            {
                process.Kill();
                process.Dispose();
            });
    }
}
