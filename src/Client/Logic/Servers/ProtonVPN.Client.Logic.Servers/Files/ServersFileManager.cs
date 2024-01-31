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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Client.Logic.Servers.Files;

public class ServersFileManager : IServersFileManager, IEventMessageReceiver<SettingChangedMessage>
{
    private const string FILE_NAME = "Servers.{0}.bin";

    private readonly object _fullFilePathLock = new();
    private Lazy<string?> _fullFilePath;

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfiguration;
    private readonly IProtobufSerializer _protobufSerializer;
    private readonly ISettings _settings;
    private readonly ISha1Calculator _sha1Calculator;

    public ServersFileManager(ILogger logger,
        IStaticConfiguration staticConfiguration,
        IProtobufSerializer protobufSerializer,
        ISettings settings,
        ISha1Calculator sha1Calculator)
    {
        _logger = logger;
        _staticConfiguration = staticConfiguration;
        _protobufSerializer = protobufSerializer;
        _settings = settings;
        _sha1Calculator = sha1Calculator;

        _fullFilePath = new(GenerateFullFilePath);
    }

    private string? GenerateFullFilePath()
    {
        string? userId = _settings.UserId;
        string? fileName = userId is null ? null : string.Format(FILE_NAME, _sha1Calculator.Hash(userId));
        string? fullFilePath = fileName is null ? null : Path.Combine(_staticConfiguration.StorageFolder, fileName);
        return fullFilePath;
    }

    public IReadOnlyList<Server> Read()
    {
        string? fullFilePath = GetFullFilePath();
        try
        {
            if (File.Exists(fullFilePath))
            {
                using (MemoryStream memoryStream = new())
                {
                    using (FileStream fileStream = new(fullFilePath, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.CopyTo(memoryStream);
                    }
                    return _protobufSerializer.Deserialize<List<Server>>(memoryStream) ?? new();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to read the servers file {fullFilePath}.", ex);
        }
        return new List<Server>();
    }

    private string? GetFullFilePath()
    {
        lock (_fullFilePathLock)
        {
            return _fullFilePath.Value;
        }
    }

    public bool Save(IList<Server> servers)
    {
        string? fullFilePath = GetFullFilePath();
        if (fullFilePath is null)
        {
            _logger.Info<AppLog>("Cannot save the servers file because the User ID is null.");
            return false;
        }

        try
        {
            if (!Directory.Exists(_staticConfiguration.StorageFolder))
            {
                Directory.CreateDirectory(_staticConfiguration.StorageFolder);
            }

            using (MemoryStream memoryStream = _protobufSerializer.Serialize(servers))
            using (FileStream fileStream = new(fullFilePath, FileMode.Create, FileAccess.Write))
            {
                memoryStream.CopyTo(fileStream);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to write the servers file {fullFilePath}.", ex);
            return false;
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.UserId))
        {
            lock (_fullFilePathLock)
            {
                _fullFilePath = new(GenerateFullFilePath);
            }
        }
    }
}