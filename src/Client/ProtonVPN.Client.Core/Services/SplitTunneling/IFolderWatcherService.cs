/*
 * Copyright (c) 2026 Proton AG
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

namespace ProtonVPN.Client.Core.Services.SplitTunneling;

public interface IFolderWatcherService : IDisposable
{
    event EventHandler<FolderChangedEventArgs>? FolderChanged;

    void StartWatching(string folderPath);

    void StopWatching(string folderPath);

    void StopAllWatching();

    IReadOnlyList<string> ScanForExecutables(string folderPath);
}

public class FolderChangedEventArgs : EventArgs
{
    public string FolderPath { get; }
    public FolderChangeType ChangeType { get; }
    public string? AffectedFilePath { get; }
    public string? OldFilePath { get; }

    public FolderChangedEventArgs(
        string folderPath,
        FolderChangeType changeType,
        string? affectedFilePath = null,
        string? oldFilePath = null)
    {
        FolderPath = folderPath;
        ChangeType = changeType;
        AffectedFilePath = affectedFilePath;
        OldFilePath = oldFilePath;
    }
}

public enum FolderChangeType
{
    ExecutableAdded,
    ExecutableRemoved,
    ExecutableRenamed,
    FolderDeleted
}
