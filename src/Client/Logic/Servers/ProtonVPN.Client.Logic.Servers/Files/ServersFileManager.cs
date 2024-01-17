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

using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Client.Logic.Servers.Files;

public class ServersFileManager : IServersFileManager
{
    private const string FILE_NAME = "Servers.bin";

    private readonly Lazy<string> _fullFilePath;

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfiguration;
    private readonly IProtobufSerializer _protobufSerializer;

    public ServersFileManager(ILogger logger,
        IStaticConfiguration staticConfiguration,
        IProtobufSerializer protobufSerializer)
    {
        _logger = logger;
        _staticConfiguration = staticConfiguration;
        _protobufSerializer = protobufSerializer;

        _fullFilePath = new(() => Path.Combine(_staticConfiguration.StorageFolder, FILE_NAME));
    }

    public IReadOnlyList<Server> Read()
    {
        try
        {
            if (File.Exists(_fullFilePath.Value))
            {
                using (MemoryStream memoryStream = new())
                {
                    using (FileStream fileStream = new(_fullFilePath.Value, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.CopyTo(memoryStream);
                    }
                    return _protobufSerializer.Deserialize<List<Server>>(memoryStream) ?? new();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to read the servers file {FILE_NAME}.", ex);
        }
        return new List<Server>();
    }

    public bool Save(IList<Server> servers)
    {
        try
        {
            if (!Directory.Exists(_staticConfiguration.StorageFolder))
            {
                Directory.CreateDirectory(_staticConfiguration.StorageFolder);
            }

            using (MemoryStream memoryStream = _protobufSerializer.Serialize(servers))
            using (FileStream fileStream = new(_fullFilePath.Value, FileMode.Create, FileAccess.Write))
            {
                memoryStream.CopyTo(fileStream);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to write the servers file {FILE_NAME}.", ex);
            return false;
        }
    }
}