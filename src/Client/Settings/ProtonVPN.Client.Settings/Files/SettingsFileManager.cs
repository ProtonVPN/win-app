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

using System.Text;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Client.Settings.Files;

public class SettingsFileManager : ISettingsFileManager
{
    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfiguration;
    private readonly IJsonSerializer _jsonSerializer;

    public SettingsFileManager(ILogger logger,
        IStaticConfiguration staticConfiguration,
        IJsonSerializer jsonSerializer)
    {
        _logger = logger;
        _staticConfiguration = staticConfiguration;
        _jsonSerializer = jsonSerializer;
    }

    public IDictionary<string, string?> Read(string fileName)
    {
        try
        {
            string fullFilePath = GetFullFilePath(fileName);
            if (File.Exists(fullFilePath))
            {
                string json = File.ReadAllText(fullFilePath);
                return _jsonSerializer.Deserialize<Dictionary<string, string?>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to read the settings file {fileName}.", ex);
        }
        return new Dictionary<string, string?>();
    }

    private string GetFullFilePath(string fileName)
    {
        return Path.Combine(_staticConfiguration.StorageFolder, fileName);
    }

    public bool Save(string fileName, IDictionary<string, string?> dictionary)
    {
        try
        {
            if (!Directory.Exists(_staticConfiguration.StorageFolder))
            {
                Directory.CreateDirectory(_staticConfiguration.StorageFolder);
            }
            string fullFilePath = GetFullFilePath(fileName);
            string fileContent = _jsonSerializer.SerializePretty(dictionary);
            File.WriteAllText(fullFilePath, fileContent, Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to write the settings file {fileName}.", ex);
            return false;
        }
    }

    public bool? Delete(string fileName)
    {
        bool? isSuccess = false;
        try
        {
            string fullFilePath = GetFullFilePath(fileName);
            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
                isSuccess = true;
            }
            else
            {
                isSuccess = null;
            }
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to delete the settings file {fileName}.", ex);
        }
        return isSuccess;
    }
}