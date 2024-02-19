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
using ProtonVPN.Configurations.BigTestInfra;
using ProtonVPN.Configurations.Defaults;
using ProtonVPN.Files.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;
using ProtonVPN.Serialization.Contracts;
using ProtonVPN.Serialization.Contracts.Json;

namespace ProtonVPN.Configurations.Files;

public class DebugConfigurationFileManager : IConfigurationFileManager
{
    private const string FILE_NAME = "_configuration.json";

    private readonly ILogger _logger;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IFileReaderWriter _fileReaderWriter;

    private readonly Lazy<string?> _fullFolderPath;
    private readonly PropertyInfo[] _defaultProperties = typeof(DefaultConfiguration).GetProperties();

    public DebugConfigurationFileManager(ILogger logger, IJsonSerializer jsonSerializer, IFileReaderWriter fileReaderWriter)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
        _fileReaderWriter = fileReaderWriter;
        _fullFolderPath = new Lazy<string?>(GetFullFolderPath);
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
                bool isSaved = SaveFileIfNotExists(fullFilePath);
                if (!isSaved)
                {
                    return ReadFile(fullFilePath);
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
        return _fullFolderPath.Value is null ? null : Path.Combine(_fullFolderPath.Value, FILE_NAME);
    }

    private IDictionary<string, string?> ReadFile(string fullFilePath)
    {
        using (FileStream fileStream = new(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (StreamReader streamReader = new(fileStream, new UTF8Encoding(true)))
        {
            _logger.Info<SettingsLog>($"Reading the configuration file {fullFilePath}.");
            string json = streamReader.ReadToEnd();
            _logger.Info<SettingsLog>($"Deserializing the configuration file {fullFilePath}.");
            return _jsonSerializer.DeserializeFirstLevel(json);
        }
    }

    private bool SaveFileIfNotExists(string fullFilePath)
    {
        Dictionary<string, object?> configurations = GenerateDefaultConfigurationDictionary();
        return _fileReaderWriter.CreateNew(configurations, fullFilePath, Serializers.PrettyJson) == FileOperationResult.Success;
    }

    private Dictionary<string, object?> GenerateDefaultConfigurationDictionary()
    {
        PropertyInfo[] properties = typeof(Configuration).GetProperties(BindingFlagsConstants.PUBLIC_DECLARED_ONLY);
        Dictionary<string, object?> dictionary = new();
        foreach (PropertyInfo property in properties)
        {
            dictionary.Add(property.Name, GetValueFromBtiOrDefaultConfiguration(property));
        }
        return dictionary;
    }

    private object? GetValueFromBtiOrDefaultConfiguration(PropertyInfo property)
    {
        object? defaultValue = null;

        PropertyInfo? defaultProperty = _defaultProperties.FirstOrDefault(dp => dp.Name == property.Name);
        if (defaultProperty is not null)
        {
            defaultValue = defaultProperty.GetValue(null);
        }

        return BtiConfigurationLoader.GetValue(property, defaultValue);
    }
}