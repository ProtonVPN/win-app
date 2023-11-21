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
using System.Reflection;
using System.Text;
using ProtonVPN.Configurations.Defaults;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Configurations.Files;

public class DebugConfigurationFileManager : IConfigurationFileManager
{
    private const string FILE_NAME = "_configuration.json";

    private readonly ILogger _logger;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly string? _fullFolderPath;

    public DebugConfigurationFileManager(ILogger logger, IJsonSerializer jsonSerializer)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
        _fullFolderPath = GetFullFolderPath();
    }

    private string? GetFullFolderPath()
    {
        string? processFullFilePath;
        using (Process process = Process.GetCurrentProcess())
        {
            processFullFilePath = process.MainModule?.FileName;
        }
        string? directory = string.IsNullOrWhiteSpace(processFullFilePath)
            ? null
            : Path.GetDirectoryName(processFullFilePath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            _logger.Error<SettingsLog>("Could not obtain the directory of the current process.");
            return null;
        }
        return directory;
    }

    public IDictionary<string, string?> Read()
    {
        string? fullFilePath = null;
        try
        {
            fullFilePath = GetFullFilePath();
            if (fullFilePath is not null)
            {
                if (File.Exists(fullFilePath))
                {
                    return ReadFile(fullFilePath);
                }
                else
                {
                    SaveFile(fullFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to read the configuration file {fullFilePath}.", ex);
        }
        return new Dictionary<string, string?>();
    }

    private string? GetFullFilePath()
    {
        return _fullFolderPath is null ? null : Path.Combine(_fullFolderPath, FILE_NAME);
    }

    private IDictionary<string, string?> ReadFile(string fullFilePath)
    {
        _logger.Info<SettingsLog>($"Reading the configuration file {fullFilePath}.");
        string json = File.ReadAllText(fullFilePath);
        return _jsonSerializer.DeserializeFirstLevel(json);
    }

    private void SaveFile(string fullFilePath)
    {
        _logger.Info<SettingsLog>($"Saving the configuration file {fullFilePath}.");
        try
        {
            string fileContent = _jsonSerializer.SerializePretty(GenerateDefaultConfigurationDictionary());
            File.WriteAllText(fullFilePath, fileContent, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to write the configuration file {fullFilePath}.", ex);
        }
    }

    private Dictionary<string, object?> GenerateDefaultConfigurationDictionary()
    {
        PropertyInfo[] properties = typeof(Configuration).GetProperties(BindingFlagsConstants.PUBLIC_DECLARED_ONLY);
        PropertyInfo[] defaultProperties = typeof(DefaultConfiguration).GetProperties();
        Dictionary<string, object?> dictionary = new();
        foreach (PropertyInfo property in properties)
        {
            PropertyInfo? defaultProperty = defaultProperties.FirstOrDefault(dp => dp.Name == property.Name);
            if (defaultProperty is not null)
            {
                dictionary.Add(property.Name, defaultProperty.GetValue(null));
            }
        }
        return dictionary;
    }
}