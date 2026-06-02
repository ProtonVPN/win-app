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
using System.IO;
using System.Threading;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SplitTunnelLogs;

namespace ProtonVPN.Client.Core.Services.SplitTunneling;

public class FolderWatcherService : IFolderWatcherService
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, FileSystemWatcher> _watchers;
    private readonly Dictionary<string, Timer> _debounceTimers;
    private readonly Dictionary<string, List<Action>> _pendingActions;
    private readonly object _lock = new();

    private const int DEBOUNCE_DELAY_MS = 500;
    private const int MAX_WATCHED_FOLDERS = 50;

    public event EventHandler<FolderChangedEventArgs>? FolderChanged;

    public FolderWatcherService(ILogger logger)
    {
        _logger = logger;
        _watchers = new(StringComparer.OrdinalIgnoreCase);
        _debounceTimers = new(StringComparer.OrdinalIgnoreCase);
        _pendingActions = new(StringComparer.OrdinalIgnoreCase);
    }

    public void StartWatching(string folderPath)
    {
        lock (_lock)
        {
            if (_watchers.Count >= MAX_WATCHED_FOLDERS)
            {
                _logger.Warn<SplitTunnelLog>(
                    $"Maximum watched folders limit reached: {MAX_WATCHED_FOLDERS}");
                return;
            }

            if (_watchers.ContainsKey(folderPath))
            {
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                _logger.Warn<SplitTunnelLog>(
                    $"Cannot watch non-existent folder: {folderPath}");
                return;
            }

            try
            {
                FileSystemWatcher watcher = new(folderPath)
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
                };

                watcher.Created += OnFileCreated;
                watcher.Deleted += OnFileDeleted;
                watcher.Renamed += OnFileRenamed;
                watcher.Error += OnWatcherError;

                watcher.EnableRaisingEvents = true;
                _watchers[folderPath] = watcher;

                _logger.Info<SplitTunnelLog>(
                    $"Started watching folder: {folderPath}");
            }
            catch (Exception ex)
            {
                _logger.Error<SplitTunnelLog>(
                    $"Failed to start watching folder: {folderPath}", ex);
            }
        }
    }

    public void StopWatching(string folderPath)
    {
        lock (_lock)
        {
            if (_watchers.TryGetValue(folderPath, out FileSystemWatcher? watcher))
            {
                DisposeWatcher(watcher);
                _watchers.Remove(folderPath);

                if (_debounceTimers.TryGetValue(folderPath, out Timer? timer))
                {
                    timer.Dispose();
                    _debounceTimers.Remove(folderPath);
                }

                _pendingActions.Remove(folderPath);

                _logger.Info<SplitTunnelLog>(
                    $"Stopped watching folder: {folderPath}");
            }
        }
    }

    public void StopAllWatching()
    {
        lock (_lock)
        {
            foreach (FileSystemWatcher watcher in _watchers.Values)
            {
                DisposeWatcher(watcher);
            }

            _watchers.Clear();

            foreach (Timer timer in _debounceTimers.Values)
            {
                timer.Dispose();
            }

            _debounceTimers.Clear();
            _pendingActions.Clear();

            _logger.Info<SplitTunnelLog>("Stopped watching all folders");
        }
    }

    public IReadOnlyList<string> ScanForExecutables(string folderPath)
    {
        try
        {
            if (!Directory.Exists(folderPath))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(folderPath, "*.exe", SearchOption.AllDirectories);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Warn<SplitTunnelLog>(
                $"Access denied scanning folder: {folderPath}", ex);
            return Array.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.Error<SplitTunnelLog>(
                $"Error scanning folder: {folderPath}", ex);
            return Array.Empty<string>();
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        string? folderPath = GetWatchedFolderPath(e.FullPath);
        if (folderPath == null) return;

        try
        {
            if (Directory.Exists(e.FullPath))
            {
                EnqueueDebouncedAction(folderPath, () =>
                {
                    try
                    {
                        string[] files = Directory.GetFiles(e.FullPath, "*.exe", SearchOption.AllDirectories);
                        foreach (string file in files)
                        {
                            FolderChanged?.Invoke(this, new FolderChangedEventArgs(
                                folderPath, FolderChangeType.ExecutableAdded, file));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error<SplitTunnelLog>($"Error scanning directory {e.FullPath}", ex);
                    }
                });
            }
            else if (e.FullPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                EnqueueDebouncedAction(folderPath, () =>
                {
                    FolderChanged?.Invoke(this, new FolderChangedEventArgs(
                        folderPath, FolderChangeType.ExecutableAdded, e.FullPath));
                });
            }
        }
        catch (Exception ex)
        {
            _logger.Error<SplitTunnelLog>($"Error processing FileCreated for {e.FullPath}", ex);
        }
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        string? folderPath = GetWatchedFolderPath(e.FullPath);
        if (folderPath == null) return;

        if (e.FullPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            EnqueueDebouncedAction(folderPath, () =>
            {
                FolderChanged?.Invoke(this, new FolderChangedEventArgs(
                    folderPath, FolderChangeType.ExecutableRemoved, e.FullPath));
            });
        }
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        string? folderPath = GetWatchedFolderPath(e.FullPath);
        if (folderPath == null)
        {
            return;
        }

        bool oldIsExe = e.OldFullPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
        bool newIsExe = e.FullPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);

        if (oldIsExe && newIsExe)
        {
            EnqueueDebouncedAction(folderPath, () =>
            {
                FolderChanged?.Invoke(this, new FolderChangedEventArgs(
                    folderPath, FolderChangeType.ExecutableRenamed,
                    e.FullPath, e.OldFullPath));
            });
        }
        else if (!oldIsExe && newIsExe)
        {
            EnqueueDebouncedAction(folderPath, () =>
            {
                FolderChanged?.Invoke(this, new FolderChangedEventArgs(
                    folderPath, FolderChangeType.ExecutableAdded, e.FullPath));
            });
        }
        else if (oldIsExe && !newIsExe)
        {
            EnqueueDebouncedAction(folderPath, () =>
            {
                FolderChanged?.Invoke(this, new FolderChangedEventArgs(
                    folderPath, FolderChangeType.ExecutableRemoved, e.OldFullPath));
            });
        }
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        _logger.Error<SplitTunnelLog>(
            "FileSystemWatcher error", e.GetException());

        if (sender is FileSystemWatcher watcher)
        {
            string folderPath = watcher.Path;
            try
            {
                watcher.EnableRaisingEvents = false;
                if (Directory.Exists(folderPath))
                {
                    watcher.EnableRaisingEvents = true;
                    _logger.Info<SplitTunnelLog>(
                        $"Successfully restarted watcher for: {folderPath}");
                }
                else
                {
                    FolderChanged?.Invoke(this, new FolderChangedEventArgs(
                        folderPath, FolderChangeType.FolderDeleted));
                }
            }
            catch (Exception ex)
            {
                _logger.Error<SplitTunnelLog>(
                    $"Failed to restart watcher for: {folderPath}", ex);
                FolderChanged?.Invoke(this, new FolderChangedEventArgs(
                    folderPath, FolderChangeType.FolderDeleted));
            }
        }
    }

    private void EnqueueDebouncedAction(string folderPath, Action action)
    {
        lock (_lock)
        {
            if (!_pendingActions.ContainsKey(folderPath))
            {
                _pendingActions[folderPath] = new();
            }

            _pendingActions[folderPath].Add(action);


            if (_debounceTimers.TryGetValue(folderPath, out Timer? existingTimer))
            {
                existingTimer.Dispose();
            }

            Timer timer = new(_ =>
            {
                List<Action>? actions;
                lock (_lock)
                {
                    _pendingActions.TryGetValue(folderPath, out actions);
                    _pendingActions.Remove(folderPath);
                    _debounceTimers.Remove(folderPath);
                }

                if (actions != null)
                {
                    foreach (Action a in actions)
                    {
                        try
                        {
                            a();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error<SplitTunnelLog>(
                                "Error processing folder change", ex);
                        }
                    }
                }
            }, null, DEBOUNCE_DELAY_MS, Timeout.Infinite);

            _debounceTimers[folderPath] = timer;
        }
    }

    private string? GetWatchedFolderPath(string filePath)
    {
        lock (_lock)
        {
            foreach (string watchedPath in _watchers.Keys)
            {
                if (filePath.StartsWith(watchedPath, StringComparison.OrdinalIgnoreCase))
                {
                    return watchedPath;
                }
            }
        }

        return null;
    }

    private void DisposeWatcher(FileSystemWatcher watcher)
    {
        watcher.EnableRaisingEvents = false;
        watcher.Created -= OnFileCreated;
        watcher.Deleted -= OnFileDeleted;
        watcher.Renamed -= OnFileRenamed;
        watcher.Error -= OnWatcherError;
        watcher.Dispose();
    }

    public void Dispose()
    {
        StopAllWatching();
    }
}
