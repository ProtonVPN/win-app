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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.UI.Extensions;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;

namespace ProtonVPN.Client.UI.Main.Settings.Connection;

public partial class SplitTunnelingAppViewModel : ViewModelBase
{
    private readonly SplitTunnelingPageViewModel _parentViewModel;

    [ObservableProperty]
    private bool _isActive;

    public string AppFilePath { get; }

    public List<string> AlternateAppFilePaths { get; }

    public string? AppName { get; private set; }

    public ImageSource? AppIcon { get; private set; }

    public SplitTunnelingAppViewModel(
        IViewModelHelper viewModelHelper,
        SplitTunnelingPageViewModel parentViewModel,
        string appFilePath,
        List<string>? alternateAppFilePaths)
        : this(viewModelHelper, parentViewModel, appFilePath, true, alternateAppFilePaths)
    { }

    public SplitTunnelingAppViewModel(
        IViewModelHelper viewModelHelper,
        SplitTunnelingPageViewModel parentViewModel,
        string appFilePath,
        bool isActive,
        List<string>? alternateAppFilePaths)
        : base(viewModelHelper)
    {
        _parentViewModel = parentViewModel;

        _isActive = isActive;

        AppFilePath = appFilePath;
        AlternateAppFilePaths = alternateAppFilePaths ?? new List<string>();
    }

    [RelayCommand]
    public void RemoveApp()
    {
        _parentViewModel.RemoveApp(this);
    }

    public async Task InitializeAsync()
    {
        AppName = AppFilePath.GetAppName();
        OnPropertyChanged(nameof(AppName));

        AppIcon = await AppFilePath.GetAppIconAsync();
        OnPropertyChanged(nameof(AppIcon));
    }

    partial void OnIsActiveChanged(bool value)
    {
        _parentViewModel.InvalidateAppsCount();
    }
}