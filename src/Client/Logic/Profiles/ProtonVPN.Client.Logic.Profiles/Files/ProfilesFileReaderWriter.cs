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
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Profiles.Contracts.SerializableEntities;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Files.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Client.Logic.Profiles.Files;

public class ProfilesFileReaderWriter : IProfilesFileReaderWriter
{
    private const string FILE_NAME_PREFIX = "Profiles";
    private const string FILE_EXTENSION = "bin";

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfiguration;
    private readonly IEntityMapper _entityMapper;
    private readonly IUserFileReaderWriter _userFileReaderWriter;

    private readonly UserFileReaderWriterParameters _fileReaderWriterParameters;

    public ProfilesFileReaderWriter(
        ILogger logger,
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

    public List<IConnectionProfile> Read(out bool doesFileExists)
    {
        try
        {
            _logger.Info<AppLog>("Reading profiles file.");

            doesFileExists = _userFileReaderWriter.DoesFileExist(_fileReaderWriterParameters);
            if (!doesFileExists)
            {
                _logger.Info<AppLog>("Profiles file not found for the current user.");
                return [];
            }

            List<SerializableProfile> serializableProfiles =
                _userFileReaderWriter.ReadOrNew<List<SerializableProfile>>(_fileReaderWriterParameters);

            List<IConnectionProfile> profiles =
                _entityMapper.Map<SerializableProfile, IConnectionProfile>(serializableProfiles);

            _logger.Info<AppLog>($"Read {profiles.Count} profiles from file.");
            return profiles;
            
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>("Failed to read the profiles file.", ex);
            doesFileExists = false;
        }

        return [];
    }

    public void Save(List<IConnectionProfile> profiles)
    {
        _logger.Info<AppLog>("Writing profiles file.");

        List<SerializableProfile> serializableProfiles =
            _entityMapper.Map<IConnectionProfile, SerializableProfile>(profiles);

        FileOperationResult result = _userFileReaderWriter.Write(serializableProfiles, _fileReaderWriterParameters);

        _logger.Info<AppLog>($"Writing profiles file finished with state '{result}'.");
    }
}