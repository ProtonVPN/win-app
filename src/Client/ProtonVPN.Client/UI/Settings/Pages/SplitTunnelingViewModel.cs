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

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Settings.Pages.SplitTunneling;
using ProtonVPN.Common.Core.Enums;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Models;

namespace ProtonVPN.Client.UI.Settings.Pages;

public partial class SplitTunnelingViewModel : PageViewModelBase<IMainViewNavigator>
{
    private const string EXE_FILE_EXTENSION = ".exe";

    private readonly ISettings _settings;
    private readonly IUrls _urls;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddIpAddressCommand))]
    private string? _currentIpAddress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SplitTunnelingFeatureIconSource))]
    private bool _isSplitTunnelingEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStandardSplitTunneling))]
    [NotifyPropertyChangedFor(nameof(IsInverseSplitTunneling))]
    private SplitTunnelingMode _currentSplitTunnelingMode;

    public override string? Title => Localizer.Get("Settings_Features_SplitTunneling");

    public ImageSource SplitTunnelingFeatureIconSource => GetFeatureIconSource(IsSplitTunnelingEnabled);

    public string LearnMoreUrl => _urls.SplitTunnelingLearnMore;

    public bool IsStandardSplitTunneling
    {
        get => IsSplitTunnelingMode(SplitTunnelingMode.Standard);
        set => SetSplitTunnelingMode(value, SplitTunnelingMode.Standard);
    }

    public bool IsInverseSplitTunneling
    {
        get => IsSplitTunnelingMode(SplitTunnelingMode.Inverse);
        set => SetSplitTunnelingMode(value, SplitTunnelingMode.Inverse);
    }

    public ObservableCollection<SplitTunnelingAppViewModel> Apps { get; }

    public bool HasApps => Apps.Any();

    public int ActiveAppsCount => Apps.Count(a => a.IsActive);

    public ObservableCollection<SplitTunnelingIpAddressViewModel> IpAddresses { get; }

    public bool HasIpAddresses => IpAddresses.Any();

    public int ActiveIpAddressesCount => IpAddresses.Count(ip => ip.IsActive);

    public SplitTunnelingViewModel(IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ISettings settings,
        IUrls urls)
        : base(viewNavigator, localizationProvider)
    {
        _settings = settings;
        _urls = urls;

        Apps = new();
        Apps.CollectionChanged += OnAppsCollectionChanged;

        IpAddresses = new();
        IpAddresses.CollectionChanged += OnIpAddressesCollectionChanged;
    }

    public static ImageSource GetFeatureIconSource(bool isEnabled)
    {
        return isEnabled
            ? ResourceHelper.GetIllustration("SplitTunnelingOnIllustrationSource")
            : ResourceHelper.GetIllustration("SplitTunnelingOffIllustrationSource");
    }

    [RelayCommand]
    public async Task AddAppAsync()
    {
        string filePath = await App.MainWindow.PickSingleFileAsync(Localizer.Get("Settings_Features_SplitTunneling_Apps_FilesFilterName"), new string[] { EXE_FILE_EXTENSION });
        if (!IsValidAppPath(filePath))
        {
            return;
        }

        SplitTunnelingAppViewModel? app = Apps.FirstOrDefault(a => IsSameAppPath(a.AppFilePath, filePath) || a.AlternateAppFilePaths.Any(alt => IsSameAppPath(alt, filePath)));
        if (app != null)
        {
            app.IsActive = true;
        }
        else
        {
            Apps.Add(await CreateAppFromPathAsync(filePath, true, null));
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddIpAddress))]
    public void AddIpAddress()
    {
        SplitTunnelingIpAddressViewModel? ipAddress = IpAddresses.FirstOrDefault(s => s.IpAddress == CurrentIpAddress);
        if (ipAddress != null)
        {
            ipAddress.IsActive = true;
        }
        else
        {
            IpAddresses.Add(new(Localizer, this, CurrentIpAddress));
        }

        CurrentIpAddress = string.Empty;
    }

    public bool CanAddIpAddress()
    {
        return !string.IsNullOrEmpty(CurrentIpAddress)
            && CurrentIpAddress.IsValidIpAddress();
    }

    public void RemoveApp(SplitTunnelingAppViewModel app)
    {
        Apps.Remove(app);
    }

    public void InvalidateAppsCount()
    {
        OnPropertyChanged(nameof(ActiveAppsCount));
    }

    public void RemoveIpAddress(SplitTunnelingIpAddressViewModel ipAddress)
    {
        IpAddresses.Remove(ipAddress);
    }

    public void InvalidateIpAddressesCount()
    {
        OnPropertyChanged(nameof(ActiveIpAddressesCount));
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();

        // TODO: Prompt for reconnection. Only update settings if user approves

        _settings.IsSplitTunnelingEnabled = IsSplitTunnelingEnabled;
        _settings.SplitTunnelingMode = CurrentSplitTunnelingMode;
        _settings.SplitTunnelingAppsList = Apps.Select(app => new SplitTunnelingApp(app.AppFilePath, app.IsActive)).ToList();
        _settings.SplitTunnelingIpAddressesList = IpAddresses.Select(ip => new SplitTunnelingIpAddress(ip.IpAddress, ip.IsActive)).ToList();
    }

    public override async void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        IsSplitTunnelingEnabled = _settings.IsSplitTunnelingEnabled;
        CurrentSplitTunnelingMode = _settings.SplitTunnelingMode;

        Apps.Clear();
        foreach (SplitTunnelingApp app in _settings.SplitTunnelingAppsList)
        {
            Apps.Add(await CreateAppFromPathAsync(app.AppFilePath, app.IsActive, app.AlternateAppFilePaths));
        }

        IpAddresses.Clear();
        foreach (SplitTunnelingIpAddress ip in _settings.SplitTunnelingIpAddressesList)
        {
            IpAddresses.Add(new(Localizer, this, ip.IpAddress, ip.IsActive));
        }
    }

    private async Task<SplitTunnelingAppViewModel> CreateAppFromPathAsync(string filePath, bool isActive, List<string>? alternateFilePaths)
    {
        SplitTunnelingAppViewModel app = new(Localizer, this, filePath, isActive, alternateFilePaths);
        await app.InitializeAsync();
        return app;
    }

    private bool IsValidAppPath(string filePath)
    {
        try
        {
            return !string.IsNullOrEmpty(filePath)
                && Path.IsPathRooted(filePath)
                && string.Equals(Path.GetExtension(filePath), EXE_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase)
                && File.Exists(filePath);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private bool IsSameAppPath(string filePathA, string filePathB)
    {
        return string.Equals(filePathA, filePathB, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsSplitTunnelingMode(SplitTunnelingMode splitTunnelingMode)
    {
        return CurrentSplitTunnelingMode == splitTunnelingMode;
    }

    private void SetSplitTunnelingMode(bool value, SplitTunnelingMode splitTunnelingMode)
    {
        if (value)
        {
            CurrentSplitTunnelingMode = splitTunnelingMode;
        }
    }

    private void OnAppsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasApps));

        InvalidateAppsCount();
    }

    private void OnIpAddressesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasIpAddresses));

        InvalidateIpAddressesCount();
    }
}