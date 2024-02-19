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

namespace ProtonVPN.Client.Files;

public class UserFileReaderWriter : IUserFileReaderWriter, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IFileReaderWriter _fileReaderWriter;
    private readonly IUserHashGenerator _userHashGenerator;

    private readonly ResettableLazy<string?> _userId;

    public UserFileReaderWriter(IFileReaderWriter fileReaderWriter, IUserHashGenerator userHashGenerator)
    {
        _fileReaderWriter = fileReaderWriter;
        _userHashGenerator = userHashGenerator;

        _userId = new(() => _userHashGenerator.Generate());
    }

    public T ReadOrNew<T>(UserFileReaderWriterParameters parameters)
        where T : new()
    {
        return _fileReaderWriter.ReadOrNew<T>(GetFullFilePath(parameters), parameters.Serializer);
    }

    public FileOperationResult Write<T>(T value, UserFileReaderWriterParameters parameters)
    {
        return _fileReaderWriter.Write<T>(value, GetFullFilePath(parameters), parameters.Serializer);
    }

    private string GetFullFilePath(UserFileReaderWriterParameters parameters)
    {
        return string.Format(parameters.FullFilePathFormat, _userId.Value);
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.UserId))
        {
            _userId.Reset();
        }
    }
}