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

using ProtonVPN.Client.Files.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Files.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Client.Logic.Servers.Files;

public class LocationNamesFileReaderWriter : ILocationNamesFileReaderWriter
{
    private const string FILE_NAME_PREFIX = "LocationNames";
    private const string FILE_EXTENSION = "bin";

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfiguration;
    private readonly IUserFileReaderWriter _userFileReaderWriter;

    private readonly UserFileReaderWriterParameters _fileReaderWriterParameters;

    public LocationNamesFileReaderWriter(
        ILogger logger,
        IStaticConfiguration staticConfiguration,
        IUserFileReaderWriter userFileReaderWriter)
    {
        _logger = logger;
        _staticConfiguration = staticConfiguration;
        _userFileReaderWriter = userFileReaderWriter;

        _fileReaderWriterParameters = new(Serializers.Protobuf, _staticConfiguration.StorageFolder, FILE_NAME_PREFIX, FILE_EXTENSION);
    }

    public LocationNamesFile Read()
    {
        LocationNamesFile file = _userFileReaderWriter.ReadOrNew<LocationNamesFile>(_fileReaderWriterParameters);
        _logger.Info<AppLog>($"Read location names file with {file.Languages.Count} cached language(s).");
        return file;
    }

    public void Save(LocationNamesFile file)
    {
        _logger.Info<AppLog>($"Writing location names file with {file.Languages.Count} cached language(s).");
        FileOperationResult result = _userFileReaderWriter.Write(file, _fileReaderWriterParameters);
    }
}