/*
 * Copyright (c) 2026 Proton AG
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
using System.Security.AccessControl;

namespace ProtonVPN.Vpn.Common;

public class SecureConfigFileWriter : ISecureConfigFileWriter
{
    // SYSTEM: Full Control; Administrators: Delete only; inheritance blocked.
    // Only processes running under the SYSTEM account can read.
    private const string SECURE_SDDL = "O:SYG:SYD:P(A;;FA;;;SY)(A;;SD;;;BA)";

    public void Write(string path, string content)
    {
        // Delete first so FileMode.CreateNew applies the FileSecurity
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        FileSecurity fileSecurity = new();
        fileSecurity.SetSecurityDescriptorSddlForm(SECURE_SDDL);

        FileInfo fileInfo = new(path);
        using FileStream stream = fileInfo.Create(
            FileMode.CreateNew,
            FileSystemRights.WriteData,
            FileShare.None,
            bufferSize: 4096,
            FileOptions.None,
            fileSecurity);
        using StreamWriter writer = new(stream);
        writer.Write(content);
    }
}