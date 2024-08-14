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

using ProtonVPN.Client.Files.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.SerializableEntities;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Files.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Client.Logic.Recents.Files;

public class RecentsFileReaderWriter : IRecentsFileReaderWriter
{
    private const string FILE_NAME_PREFIX = "RecentConnections";
    private const string FILE_EXTENSION = "bin";

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfiguration;
    private readonly IEntityMapper _entityMapper;
    private readonly IUserFileReaderWriter _userFileReaderWriter;

    private readonly UserFileReaderWriterParameters _fileReaderWriterParameters;

    public RecentsFileReaderWriter(ILogger logger,
        IStaticConfiguration staticConfiguration,
        IEntityMapper entityMapper,
        IUserFileReaderWriter userFileReaderWriter)
    {
        _logger = logger;
        _staticConfiguration = staticConfiguration;
        _entityMapper = entityMapper;
        _userFileReaderWriter = userFileReaderWriter;

        _fileReaderWriterParameters = new(Serializers.Protobuf, _staticConfiguration.StorageFolder, FILE_NAME_PREFIX, FILE_EXTENSION);
    }

    public List<IRecentConnection> Read()
    {
        try
        {
            _logger.Info<AppLog>("Reading recent connections file.");

            List<SerializableRecentConnection> serializableRecentConnections =
                _userFileReaderWriter.ReadOrNew<List<SerializableRecentConnection>>(_fileReaderWriterParameters);

            List<IRecentConnection> recents =
                _entityMapper.Map<SerializableRecentConnection, IRecentConnection>(serializableRecentConnections)
                             .Where(r => r != null).ToList();

            _logger.Info<AppLog>($"Read {recents.Count} recent connections from file.");
            return recents;
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>("Failed to read the recent connections file.", ex);
        }
        return [];
    }

    public void Save(List<IRecentConnection> recentConnections)
    {
        _logger.Info<AppLog>("Writing recent connections file.");

        List<SerializableRecentConnection> serializableRecentConnections =
            _entityMapper.Map<IRecentConnection, SerializableRecentConnection>(recentConnections)
                .Where(r => r != null).ToList();

        FileOperationResult result = _userFileReaderWriter.Write(serializableRecentConnections, _fileReaderWriterParameters);

        _logger.Info<AppLog>($"Writing recent connections file finished with state '{result}'.");
    }
}