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

using System.Management;
using System.Text;
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Client.Logic.Feedback.Diagnostics.Logs;

public class InstalledDriversLog : LogBase
{
    public InstalledDriversLog(IStaticConfiguration config)
        : base(config.DiagnosticLogsFolder, "Drivers.txt")
    {
    }

    public override void Write()
    {
        File.WriteAllText(Path, GenerateContent());
    }

    private string GenerateContent()
    {
        try
        {
            StringBuilder fileContent = new();
            fileContent.AppendLine("==========");
            ObjectQuery query = new("SELECT * FROM Win32_SystemDriver");
            using (ManagementObjectSearcher searcher = new(query))
            {
                ManagementObjectCollection queryCollection = searcher.Get();
                foreach (ManagementBaseObject m in queryCollection)
                {
                    Dictionary<string, object> driverProperties = GetDriverProperties(m);
                    fileContent.AppendLine(GenerateDriverLine(driverProperties));
                }
            }
            return fileContent.ToString();
        }
        catch
        {
            return "Error when fetching drivers.";
        }
    }

    private Dictionary<string, object> GetDriverProperties(ManagementBaseObject m)
    {
        Dictionary<string, object> properties = new();
        foreach (PropertyData p in m.Properties)
        {
            properties.Add(p.Name, p.Value);
        }
        return properties;
    }

    private string GenerateDriverLine(Dictionary<string, object> driverProperties)
    {
        StringBuilder driverContent = new();
        AppendDriverProperty(driverContent, driverProperties, "DisplayName");
        AppendDriverProperty(driverContent, driverProperties, "Description");
        AppendDriverProperty(driverContent, driverProperties, "Caption");
        AppendDriverProperty(driverContent, driverProperties, "Name");
        AppendDriverProperty(driverContent, driverProperties, "Started");
        AppendDriverProperty(driverContent, driverProperties, "StartMode");
        AppendDriverProperty(driverContent, driverProperties, "State");
        AppendDriverProperty(driverContent, driverProperties, "Status");
        driverContent.Append("==========");
        return driverContent.ToString();
    }

    private void AppendDriverProperty(StringBuilder driverContent, Dictionary<string, object> driverProperties, string key)
    {
        try
        {
            bool hasProperty = driverProperties.TryGetValue(key, out object value);
            if (hasProperty)
            {
                driverContent.AppendLine($"{key}: {value}");
            }
        }
        catch
        {
        }
    }
}