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
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.SerializableEntities;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Client.Logic.Recents.Files;

public class RecentsFileManager : IRecentsFileManager, IEventMessageReceiver<SettingChangedMessage>
{
    private const string FILE_NAME = "RecentConnections.{0}.bin";

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfiguration;
    private readonly IProtobufSerializer _protobufSerializer;
    private readonly IEntityMapper _entityMapper;
    private readonly ISettings _settings;
    private readonly ISha1Calculator _sha1Calculator;

    private readonly object _fullFilePathLock = new();
    private Lazy<string> _fullFilePath;

    private readonly object _cacheLock = new();
    private byte[] _cache = [];

    public RecentsFileManager(ILogger logger,
        IStaticConfiguration staticConfiguration,
        IProtobufSerializer protobufSerializer,
        IEntityMapper entityMapper,
        ISettings settings,
        ISha1Calculator sha1Calculator)
    {
        _logger = logger;
        _staticConfiguration = staticConfiguration;
        _protobufSerializer = protobufSerializer;
        _entityMapper = entityMapper;
        _settings = settings;
        _sha1Calculator = sha1Calculator;

        _fullFilePath = new(GenerateFullFilePath);
    }

    private string GenerateFullFilePath()
    {
        string userId = _settings.UserId;
        string fileName = userId is null ? null : string.Format(FILE_NAME, _sha1Calculator.Hash(userId));
        string fullFilePath = fileName is null ? null : Path.Combine(_staticConfiguration.StorageFolder, fileName);
        return fullFilePath;
    }

    public List<IRecentConnection> Read()
    {
        string fullFilePath = GetFullFilePath();
        lock (_cacheLock)
        {
            try
            {
                if (fullFilePath is not null && File.Exists(fullFilePath))
                {
                    _logger.Info<AppLog>($"Reading recent connections file '{fullFilePath}'.");
                    List<SerializableRecentConnection> serializableRecentConnections;
                    using (MemoryStream memoryStream = new())
                    using (FileStream fileStream = new(fullFilePath, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.CopyTo(memoryStream);
                        _cache = memoryStream.ToArray();
                        serializableRecentConnections =
                            _protobufSerializer.Deserialize<List<SerializableRecentConnection>>(memoryStream) ?? [];
                    }
                    List<IRecentConnection> recents =
                        _entityMapper.Map<SerializableRecentConnection, IRecentConnection>(serializableRecentConnections);
                    _logger.Info<AppLog>($"Read {recents.Count} recent connections.");
                    return recents;
                }
                else
                {
                    string reason = fullFilePath is null ? "the User ID is null" : "the file does not exist";
                    _logger.Info<AppLog>($"Not able to read the recent connections file, because {reason}.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error<SettingsLog>($"Failed to read the recent connections file {fullFilePath}.", ex);
            }
            _cache = [];
        }
        return [];
    }

    private string GetFullFilePath()
    {
        lock (_fullFilePathLock)
        {
            return _fullFilePath.Value;
        }
    }

    public bool Save(List<IRecentConnection> recentConnections)
    {
        string fullFilePath = GetFullFilePath();
        if (fullFilePath is null)
        {
            _logger.Info<AppLog>("Cannot save the recent connections file because the User ID is null.");
            return false;
        }

        try
        {
            lock (_cacheLock)
            {
                if (!Directory.Exists(_staticConfiguration.StorageFolder))
                {
                    Directory.CreateDirectory(_staticConfiguration.StorageFolder);
                }

                List<SerializableRecentConnection> serializableRecentConnections =
                    _entityMapper.Map<IRecentConnection, SerializableRecentConnection>(recentConnections);

                using (MemoryStream memoryStream = _protobufSerializer.Serialize(serializableRecentConnections))
                {
                    if (Enumerable.SequenceEqual(memoryStream.ToArray(), _cache))
                    {
                        _logger.Info<AppLog>($"No need to save recents as cache is equal.");
                        return true;
                    }

                    _logger.Info<AppLog>($"Saving {recentConnections.Count} recent connections to file '{fullFilePath}'.");
                    using (FileStream fileStream = new(fullFilePath, FileMode.Create, FileAccess.Write))
                    {
                        memoryStream.CopyTo(fileStream);
                        _cache = memoryStream.ToArray();
                    }
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to write the recent connections file {fullFilePath}.", ex);
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
            lock (_cacheLock)
            {
                _cache = [];
            }
        }
    }
}