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

using System.IO;
using ProtonVPN.Client.Files.Contracts;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Files.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Serialization.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Models;

namespace ProtonVPN.StatisticalEvents.Files;

public class StatisticalEventsFileReaderWriter : IStatisticalEventsFileReaderWriter
{
    private const Serializers SERIALIZER = Serializers.Protobuf;
    private const string AUTH_EVENTS_FILE_NAME_PREFIX = "AuthenticatedStatisticalEvents";
    private const string FILE_EXTENSION = "bin";
    private const string UNAUTH_EVENTS_FILE_NAME = $"UnauthenticatedStatisticalEvents.{FILE_EXTENSION}";

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfiguration;
    private readonly IUserFileReaderWriter _userFileReaderWriter;
    private readonly IFileReaderWriter _fileReaderWriter;

    private readonly UserFileReaderWriterParameters _authEventsFileReaderWriterParameters;
    private readonly string _unauthEventsFullFilePath;

    public StatisticalEventsFileReaderWriter(
        ILogger logger,
        IStaticConfiguration staticConfiguration,
        IUserFileReaderWriter userFileReaderWriter,
        IFileReaderWriter fileReaderWriter)
    {
        _logger = logger;
        _staticConfiguration = staticConfiguration;
        _userFileReaderWriter = userFileReaderWriter;
        _fileReaderWriter = fileReaderWriter;

        _authEventsFileReaderWriterParameters = new(SERIALIZER, _staticConfiguration.StorageFolder, AUTH_EVENTS_FILE_NAME_PREFIX, FILE_EXTENSION);
        _unauthEventsFullFilePath = Path.Combine(_staticConfiguration.StorageFolder, UNAUTH_EVENTS_FILE_NAME);
    }

    public StatisticalEventsFile ReadAuthenticatedEvents()
    {
        _logger.Info<AppLog>("Reading authenticated statistical events file.");
        StatisticalEventsFile file = _userFileReaderWriter.ReadOrNew<StatisticalEventsFile>(_authEventsFileReaderWriterParameters);
        _logger.Info<AppLog>($"Read {file.StatisticalEvents.Count} authenticated statistical events from file.");
        return file;
    }

    public void SaveAuthenticatedEvents(StatisticalEventsFile file)
    {
        _logger.Info<AppLog>("Writing authenticated statistical events file.");
        FileOperationResult result = _userFileReaderWriter.Write(file, _authEventsFileReaderWriterParameters);
        _logger.Info<AppLog>($"Writing authenticated statistical events file finished with state '{result}'.");
    }

    public StatisticalEventsFile ReadUnauthenticatedEvents()
    {
        _logger.Info<AppLog>("Reading unauthenticated statistical events file.");
        StatisticalEventsFile file = _fileReaderWriter.ReadOrNew<StatisticalEventsFile>(_unauthEventsFullFilePath, SERIALIZER);
        _logger.Info<AppLog>($"Read {file.StatisticalEvents.Count} unauthenticated statistical events from file.");
        return file;
    }

    public void SaveUnauthenticatedEvents(StatisticalEventsFile file)
    {
        _logger.Info<AppLog>("Writing unauthenticated statistical events file.");
        FileOperationResult result = _fileReaderWriter.Write(file, _unauthEventsFullFilePath, SERIALIZER);
        _logger.Info<AppLog>($"Writing unauthenticated statistical events file finished with state '{result}'.");
    }
}