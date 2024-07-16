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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtonVPN.Files.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;
using ProtonVPN.Serialization.Contracts;
using ProtonVPN.Serialization.Contracts.Json;

namespace ProtonVPN.Files;

public class FileReaderWriter : IFileReaderWriter
{
    private readonly ILogger _logger;
    private readonly IProtobufSerializer _protobufSerializer;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IPrettyJsonSerializer _prettyJsonSerializer;

    public FileReaderWriter(ILogger logger,
        IProtobufSerializer protobufSerializer,
        IJsonSerializer jsonSerializer,
        IPrettyJsonSerializer prettyJsonSerializer)
    {
        _logger = logger;
        _protobufSerializer = protobufSerializer;
        _jsonSerializer = jsonSerializer;
        _prettyJsonSerializer = prettyJsonSerializer;
    }

    public T ReadOrNew<T>(string fullFilePath, Serializers serializer)
        where T : new()
    {
        return ReadOrDefault<T>(fullFilePath, serializer) ?? new();
    }

    public T ReadOrDefault<T>(string fullFilePath, Serializers serializer)
    {
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
                    return GetSerializer(serializer).Deserialize<T>(memoryStream);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>($"Failed to read the file {fullFilePath}.", ex);
        }
        return default(T);
    }

    private ISerializer GetSerializer(Serializers serializer)
    {
        return serializer switch
        {
            Serializers.Protobuf => _protobufSerializer,
            Serializers.Json => _jsonSerializer,
            Serializers.PrettyJson => _prettyJsonSerializer,
            _ => _protobufSerializer,
        };
    }

    public IDictionary<string, T> ReadAllUsers<T>(string folderPath, string fileNamePrefix, string fileExtension, Serializers serializer)
    {
        IList<string> fileNames = Directory.EnumerateFiles(folderPath)
            .Select(Path.GetFileName)
            .Where(f => f.StartsWith(fileNamePrefix) && f.EndsWith(fileExtension))
            .ToList();

        Dictionary<string, T> result = new();
        foreach (string fileName in fileNames)
        {
            string userIdHash = fileName.Replace($"{fileNamePrefix}.", string.Empty).Replace(fileExtension, string.Empty);
            result.Add(userIdHash, ReadOrDefault<T>(Path.Combine(folderPath, fileName), serializer));
        }
        return result;
    }

    public FileOperationResult Write<T>(T value, string fullFilePath, Serializers serializer)
    {
        return WriteByFileMode(value, fullFilePath, serializer, FileMode.Create);
    }

    private FileOperationResult WriteByFileMode<T>(T value, string fullFilePath, Serializers serializer, FileMode fileMode)
    {
        if (fullFilePath is null)
        {
            _logger.Info<AppLog>("Cannot save the file because the FullFilePath is null.");
            return FileOperationResult.Failed;
        }

        CreateDirectory(fullFilePath);

        try
        {
            using (MemoryStream memoryStream = GetSerializer(serializer).Serialize(value))
            using (FileStream fileStream = new(fullFilePath, fileMode, FileAccess.Write, FileShare.None))
            {
                memoryStream.CopyTo(fileStream);
            }
            return FileOperationResult.Success;
        }
        catch (IOException ex) when (ex.HResult == -2147024816) // 0x80070050 - The file already exists
        {
            _logger.Info<SettingsLog>($"The file already exists ({fullFilePath}).");
            return FileOperationResult.AlreadyExists;
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to write the file {fullFilePath}.", ex);
            return FileOperationResult.Failed;
        }
    }

    private void CreateDirectory(string fullFilePath)
    {
        try
        {
            string fullDirectoryPath = Path.GetDirectoryName(fullFilePath);
            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
                _logger.Info<SettingsLog>($"Created the directory '{fullDirectoryPath}'.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to find or create the folder of the file {fullFilePath}.", ex);
        }
    }

    public FileOperationResult CreateNew<T>(T value, string fullFilePath, Serializers serializer)
    {
        return WriteByFileMode(value, fullFilePath, serializer, FileMode.CreateNew);
    }
}