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

using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Client.Files.Contracts;

public class UserFileReaderWriterParameters
{
    public Serializers Serializer { get; }

    public string FolderPath { get; }
    public string FileNamePrefix { get; }
    public string FileExtension { get; }

    public string FullFilePathFormat { get; }

    public UserFileReaderWriterParameters(Serializers serializer, string folderPath, string fileNamePrefix, string fileExtension)
    {
        Serializer = serializer;

        FolderPath = folderPath.Trim();
        FileNamePrefix = fileNamePrefix.Trim();
        string tempFileExtension = fileExtension.Trim();
        FileExtension = tempFileExtension.StartsWith('.') ? tempFileExtension : $".{tempFileExtension}";

        FullFilePathFormat = Path.Combine(FolderPath, $"{FileNamePrefix}.{{0}}{FileExtension}");
    }
}