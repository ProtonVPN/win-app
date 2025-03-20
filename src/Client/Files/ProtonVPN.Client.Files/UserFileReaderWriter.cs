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
using ProtonVPN.Client.Files.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Files.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Files;

public class UserFileReaderWriter : IUserFileReaderWriter, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IFileReaderWriter _fileReaderWriter;
    private readonly IUserHashGenerator _userHashGenerator;
    private readonly ILogger _logger;

    private readonly ResettableLazy<string?> _userId;

    public UserFileReaderWriter(IFileReaderWriter fileReaderWriter,
        IUserHashGenerator userHashGenerator,
        ILogger logger)
    {
        _fileReaderWriter = fileReaderWriter;
        _userHashGenerator = userHashGenerator;
        _logger = logger;

        _userId = new(() => _userHashGenerator.Generate());
    }

    public T ReadOrNew<T>(UserFileReaderWriterParameters parameters)
        where T : new()
    {
        try
        {
            return _fileReaderWriter.ReadOrNew<T>(GetFullFilePath(parameters), parameters.Serializer);
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>("Failed to read the file.", ex);
            return new();
        }
    }

    public FileOperationResult Write<T>(T value, UserFileReaderWriterParameters parameters)
    {
        try
        {
            return _fileReaderWriter.Write<T>(value, GetFullFilePath(parameters), parameters.Serializer);
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>("Failed to write the file.", ex);
            return FileOperationResult.Failed;
        }
    }

    public bool DoesFileExist(UserFileReaderWriterParameters parameters)
    {
        try
        {
            string filePath = GetFullFilePath(parameters);
            return File.Exists(filePath);
        }
        catch
        {
            return false;
        }
    }

    private string GetFullFilePath(UserFileReaderWriterParameters parameters)
    {
        string? userId = _userId.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentNullException("UserId");
        }
        return string.Format(parameters.FullFilePathFormat, userId);
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.UserId))
        {
            _userId.Reset();
        }
    }

    public IDictionary<string, T> ReadAllUsers<T>(UserFileReaderWriterParameters parameters)
    {
        return _fileReaderWriter.ReadAllUsers<T>(parameters.FolderPath,
            parameters.FileNamePrefix, parameters.FileExtension, parameters.Serializer);
    }
}