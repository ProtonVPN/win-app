/*
 * Copyright (c) 2024 Proton AG
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
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Files.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Client.Logic.Announcements.Files;

public class AnnouncementsFileReaderWriter : IAnnouncementsFileReaderWriter
{
    private const string FILE_NAME_PREFIX = "Announcements";
    private const string FILE_EXTENSION = "bin";

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfiguration;
    private readonly IUserFileReaderWriter _userFileReaderWriter;

    private readonly UserFileReaderWriterParameters _fileReaderWriterParameters;

    public AnnouncementsFileReaderWriter(ILogger logger,
        IStaticConfiguration staticConfiguration,
        IUserFileReaderWriter userFileReaderWriter)
    {
        _logger = logger;
        _staticConfiguration = staticConfiguration;
        _userFileReaderWriter = userFileReaderWriter;

        _fileReaderWriterParameters = new(Serializers.Protobuf, _staticConfiguration.StorageFolder, FILE_NAME_PREFIX, FILE_EXTENSION);
    }

    public List<Announcement> Read()
    {
        _logger.Info<AppLog>("Reading announcements file.");
        List<Announcement> announcements = _userFileReaderWriter.ReadOrNew<List<Announcement>>(_fileReaderWriterParameters);
        _logger.Info<AppLog>($"Read {announcements.Count} announcements from file.");
        return announcements;
    }

    public IDictionary<string, List<Announcement>> ReadAllUsers()
    {
        _logger.Info<AppLog>("Reading all announcement files.");
        IDictionary<string, List<Announcement>> announcements = _userFileReaderWriter.ReadAllUsers<List<Announcement>>(_fileReaderWriterParameters);
        _logger.Info<AppLog>($"Read {announcements.Count} announcement files.");
        return announcements;
    }

    public void Save(IList<Announcement> announcements)
    {
        _logger.Info<AppLog>("Writing announcements file.");
        FileOperationResult result = _userFileReaderWriter.Write(announcements, _fileReaderWriterParameters);
        _logger.Info<AppLog>($"Writing announcements file finished with state '{result}'.");
    }
}