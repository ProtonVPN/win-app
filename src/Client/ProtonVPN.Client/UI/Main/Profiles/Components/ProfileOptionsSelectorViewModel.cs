/*
 * Copyright (c) 2025 Proton AG
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

using System.Security;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Core.Models;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Models.Profiles;
using ProtonVPN.Client.UI.Main.Profiles.Contracts;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.UI.Main.Profiles.Components;

public partial class ProfileOptionsSelectorViewModel : ViewModelBase, IProfileOptionsSelector
{
    private const string EXE_FILE_EXTENSION = ".exe";

    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly ICommonItemFactory _commonItemFactory;

    private static readonly ConnectAndGoMode?[] _connectAndGoModes =
    {
        null,
        ConnectAndGoMode.Website,
        ConnectAndGoMode.Application
    };

    private IProfileOptions _originalProfileOptions = ProfileOptions.Default;

    [ObservableProperty]
    private bool _isEnabled;

    [ObservableProperty]
    private ConnectAndGoModeItem? _selectedConnectAndGoMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectAndGoUrlValid))]
    [NotifyPropertyChangedFor(nameof(ConnectAndGoUrlErrorMessage))]
    private string _connectAndGoUrl = string.Empty;

    [ObservableProperty]
    private bool _usePrivateBrowsingMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectAndGoAppName))]
    private ExternalApp? _connectAndGoApp;

    public bool IsConnectAndGoUrlValid => string.IsNullOrEmpty(ConnectAndGoUrl) || ConnectAndGoUrl.IsValidUrl();

    public string ConnectAndGoUrlErrorMessage => !IsConnectAndGoUrlValid
        ? Localizer.Get("Profile_Options_Url_Error")
        : string.Empty;

    public string ConnectAndGoAppName => ConnectAndGoApp?.AppName ?? Localizer.Get("Profile_Options_Application_Select");

    public SmartObservableCollection<ConnectAndGoModeItem> ConnectAndGoModes { get; } = [];

    private bool ConnectAndGoStateHasChanged => _originalProfileOptions.ConnectAndGo.IsEnabled != SelectedConnectAndGoMode?.IsEnabled;

    private bool ConnectAndGoModeHasChanged => _originalProfileOptions.ConnectAndGo.Mode != SelectedConnectAndGoMode?.Mode;

    public ProfileOptionsSelectorViewModel(
        IMainWindowActivator mainWindowActivator,
        ICommonItemFactory commonItemFactory,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _mainWindowActivator = mainWindowActivator;
        _commonItemFactory = commonItemFactory;
    }

    public IProfileOptions GetProfileOptions()
    {
        // Turn off Connect and go on save if the URL or application is not valid
        bool isConnectAndGoEnabled = 
            SelectedConnectAndGoMode != null &&
            SelectedConnectAndGoMode.IsEnabled && 
            SelectedConnectAndGoMode.Mode switch
            {
                ConnectAndGoMode.Website => ConnectAndGoUrl.IsValidUrl(),
                ConnectAndGoMode.Application => ConnectAndGoApp?.IsValid ?? false,
                _ => false
            };

        return new ProfileOptions()
        {
            ConnectAndGo = new ConnectAndGoOption()
            {
                IsEnabled = isConnectAndGoEnabled,
                Mode = isConnectAndGoEnabled && SelectedConnectAndGoMode != null
                    ? SelectedConnectAndGoMode.Mode
                    : DefaultProfileSettings.ConnectAndGoMode,
                UsePrivateBrowsingMode = UsePrivateBrowsingMode,
                Url = ConnectAndGoUrl.Trim(),
                AppPath = ConnectAndGoApp?.AppPath
            }
        };
    }

    public async Task SetProfileOptionsAsync(IProfileOptions options)
    {
        _originalProfileOptions = options ?? ProfileOptions.Default;

        InvalidateCollections();

        SelectedConnectAndGoMode = _originalProfileOptions.ConnectAndGo.IsEnabled
            ? ConnectAndGoModes.FirstOrDefault(m => m.IsEnabled && _originalProfileOptions.ConnectAndGo.Mode == m.Mode)
            : ConnectAndGoModes.FirstOrDefault(m => !m.IsEnabled);
        UsePrivateBrowsingMode = _originalProfileOptions.ConnectAndGo.UsePrivateBrowsingMode;
        ConnectAndGoUrl = _originalProfileOptions.ConnectAndGo.Url;
        ConnectAndGoApp = await ExternalApp.TryCreateAsync(_originalProfileOptions.ConnectAndGo.AppPath ?? string.Empty);
    }

    public bool HasChanged()
    {
        return ConnectAndGoStateHasChanged
            || ConnectAndGoModeHasChanged
            || SelectedConnectAndGoMode?.Mode switch
            {
                ConnectAndGoMode.Website =>
                    _originalProfileOptions.ConnectAndGo.Url != ConnectAndGoUrl.Trim() ||
                    _originalProfileOptions.ConnectAndGo.UsePrivateBrowsingMode != UsePrivateBrowsingMode,
                ConnectAndGoMode.Application =>
                    _originalProfileOptions.ConnectAndGo.AppPath != ConnectAndGoApp?.AppPath,
                _ => false
            };
    }

    public bool IsReconnectionRequired()
    {
        return false;
    }

    [RelayCommand]
    public async Task SelectAppAsync()
    {
        if (_mainWindowActivator.Window == null)
        {
            return;
        }

        string filePath = await _mainWindowActivator.Window.PickSingleFileAsync(Localizer.Get("Settings_Connection_SplitTunneling_Apps_FilesFilterName"), [EXE_FILE_EXTENSION]);
        ConnectAndGoApp = await ExternalApp.TryCreateAsync(filePath);
    }

    private void InvalidateCollections()
    {
        ConnectAndGoModes.Reset(_connectAndGoModes.Select(_commonItemFactory.GetConnectAndGoMode));
    }

    [RelayCommand]
    private void DisableConnectAndGo()
    {
        SelectConnectAndGoMode(false);
    }

    [RelayCommand]
    private void EnableConnectAndGoToWebsite()
    {
        SelectConnectAndGoMode(true, ConnectAndGoMode.Website);
    }

    [RelayCommand]
    private void EnableConnectAndGoToApplication()
    {
        SelectConnectAndGoMode(true, ConnectAndGoMode.Application);
    }

    private void SelectConnectAndGoMode(bool isEnabled, ConnectAndGoMode? mode = null)
    {
        SelectedConnectAndGoMode = isEnabled
            ? ConnectAndGoModes.FirstOrDefault(nsm => nsm.IsEnabled && nsm.Mode == mode)
            : ConnectAndGoModes.FirstOrDefault(nsm => !nsm.IsEnabled);
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(ConnectAndGoUrlErrorMessage));
        OnPropertyChanged(nameof(ConnectAndGoAppName));
    }
}