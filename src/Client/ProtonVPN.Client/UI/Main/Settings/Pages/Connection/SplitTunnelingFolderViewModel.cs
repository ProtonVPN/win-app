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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.SplitTunneling;

namespace ProtonVPN.Client.UI.Main.Settings.Connection;

public partial class SplitTunnelingFolderViewModel : ViewModelBase
{
    private readonly SplitTunnelingPageViewModel _parentViewModel;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private bool _isValidPath;

    public string FolderPath { get; }

    public string FolderName { get; private set; }

    [ObservableProperty]
    private int _executableCount;

    public SplitTunnelingFolderViewModel(
        IViewModelHelper viewModelHelper,
        SplitTunnelingPageViewModel parentViewModel,
        string folderPath,
        bool isActive)
        : base(viewModelHelper)
    {
        _parentViewModel = parentViewModel;
        FolderPath = folderPath;
        _isActive = isActive;

        IsValidPath = Directory.Exists(FolderPath);
        FolderName = Path.GetFileName(FolderPath);
        if (string.IsNullOrEmpty(FolderName))
        {
            FolderName = FolderPath; // Root drives or UNC
        }
    }

    [RelayCommand]
    public void RemoveFolder()
    {
        _parentViewModel.RemoveFolder(this);
    }

    public void UpdateExecutableCount(int count)
    {
        ExecutableCount = count;
    }

    partial void OnIsActiveChanged(bool value)
    {
        _parentViewModel.InvalidateFoldersCount();
    }
}
