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
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Client.Core.Services.SplitTunneling;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

using ProtonVPN.Client.Handlers.Bases;

namespace ProtonVPN.Client.Handlers;

public class SplitTunnelingFolderOrchestrator :
    IHandler,
    IEventMessageReceiver<VpnStateIpcEntity>,
    IEventMessageReceiver<SettingChangedMessage>,
    IDisposable
{
    private readonly IFolderWatcherService _folderWatcherService;
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly ISettings _settings;

    private bool _isVpnConnected;
    private bool _disposed;
    private HashSet<string> _previousActivePaths = new(StringComparer.OrdinalIgnoreCase);

    public SplitTunnelingFolderOrchestrator(
        IFolderWatcherService folderWatcherService,
        IVpnServiceCaller vpnServiceCaller,
        ISettings settings)
    {
        _folderWatcherService = folderWatcherService;
        _vpnServiceCaller = vpnServiceCaller;
        _settings = settings;

        _folderWatcherService.FolderChanged += OnFolderChanged;
    }

    public void Receive(VpnStateIpcEntity message)
    {
        bool wasConnected = _isVpnConnected;
        _isVpnConnected = message.Status == VpnStatusIpcEntity.Connected;

        if (_isVpnConnected && !wasConnected)
        {
            _previousActivePaths = GetCurrentActivePaths();
            UpdateWatchedFolders();
        }
        else if (!_isVpnConnected && wasConnected)
        {
            _folderWatcherService.StopAllWatching();
            _previousActivePaths.Clear();
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.SplitTunnelingStandardFoldersList) ||
            message.PropertyName == nameof(ISettings.SplitTunnelingInverseFoldersList) ||
            message.PropertyName == nameof(ISettings.SplitTunnelingMode) ||
            message.PropertyName == nameof(ISettings.IsSplitTunnelingEnabled))
        {
            OnFolderSettingsChanged();
        }
    }

    private async void OnFolderSettingsChanged()
    {
        UpdateWatchedFolders();

        if (!_isVpnConnected) return;

        HashSet<string> newActivePaths = GetCurrentActivePaths();

        string[] removedPaths = _previousActivePaths.Except(newActivePaths, StringComparer.OrdinalIgnoreCase).ToArray();
        string[] addedPaths = newActivePaths.Except(_previousActivePaths, StringComparer.OrdinalIgnoreCase).ToArray();

        if (removedPaths.Length > 0)
        {
            await _vpnServiceCaller.RemoveAppPathsDynamicallyAsync(
                new DynamicAppPathsIpcEntity { AppPaths = removedPaths });
        }

        if (addedPaths.Length > 0)
        {
            await _vpnServiceCaller.AddAppPathsDynamicallyAsync(
                new DynamicAppPathsIpcEntity { AppPaths = addedPaths });
        }

        _previousActivePaths = newActivePaths;
    }

    private HashSet<string> GetCurrentActivePaths()
    {
        HashSet<string> paths = new(StringComparer.OrdinalIgnoreCase);

        if (!_settings.IsSplitTunnelingEnabled) return paths;

        var folders = _settings.SplitTunnelingMode == Settings.Contracts.Enums.SplitTunnelingMode.Standard
            ? _settings.SplitTunnelingStandardFoldersList
            : _settings.SplitTunnelingInverseFoldersList;

        if (folders == null) return paths;

        foreach (var folder in folders.Where(f => f.IsActive))
        {
            try
            {
                if (Directory.Exists(folder.FolderPath))
                {
                    foreach (string exe in Directory.GetFiles(folder.FolderPath, "*.exe", SearchOption.AllDirectories))
                    {
                        paths.Add(exe);
                    }
                }
            }
            catch
            {
            }
        }

        return paths;
    }

    private void UpdateWatchedFolders()
    {
        _folderWatcherService.StopAllWatching();

        if (!_settings.IsSplitTunnelingEnabled)
        {
            return;
        }

        var folders = _settings.SplitTunnelingMode == Settings.Contracts.Enums.SplitTunnelingMode.Standard
            ? _settings.SplitTunnelingStandardFoldersList
            : _settings.SplitTunnelingInverseFoldersList;

        if (folders == null) return;

        foreach (var folder in folders.Where(f => f.IsActive))
        {
            _folderWatcherService.StartWatching(folder.FolderPath);
        }
    }

    private async void OnFolderChanged(object? sender, FolderChangedEventArgs e)
    {
        if (!_isVpnConnected || !_settings.IsSplitTunnelingEnabled)
        {
            return;
        }

        if (e.AffectedFilePath == null) return;

        var entity = new DynamicAppPathsIpcEntity
        {
            AppPaths = [e.AffectedFilePath]
        };

        if (e.ChangeType == FolderChangeType.ExecutableAdded)
        {
            _previousActivePaths.Add(e.AffectedFilePath);
            await _vpnServiceCaller.AddAppPathsDynamicallyAsync(entity);
        }
        else if (e.ChangeType == FolderChangeType.ExecutableRemoved)
        {
            _previousActivePaths.Remove(e.AffectedFilePath);
            await _vpnServiceCaller.RemoveAppPathsDynamicallyAsync(entity);
        }
        else if (e.ChangeType == FolderChangeType.ExecutableRenamed)
        {
            if (!string.IsNullOrEmpty(e.OldFilePath))
            {
                _previousActivePaths.Remove(e.OldFilePath);
                await _vpnServiceCaller.RemoveAppPathsDynamicallyAsync(new DynamicAppPathsIpcEntity { AppPaths = [e.OldFilePath] });
            }
            _previousActivePaths.Add(e.AffectedFilePath);
            await _vpnServiceCaller.AddAppPathsDynamicallyAsync(entity);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _folderWatcherService.FolderChanged -= OnFolderChanged;
        _folderWatcherService.StopAllWatching();
        _previousActivePaths.Clear();
        _disposed = true;
    }
}
