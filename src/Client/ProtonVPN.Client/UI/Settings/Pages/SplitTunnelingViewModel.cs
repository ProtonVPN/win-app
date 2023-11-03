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
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.UI.Settings.Pages.Entities;
using ProtonVPN.Client.UI.Settings.Pages.SplitTunneling;
using ProtonVPN.Common.Core.Extensions;
using Windows.System;

namespace ProtonVPN.Client.UI.Settings.Pages;

public partial class SplitTunnelingViewModel : SettingsPageViewModelBase
{
    private const string EXE_FILE_EXTENSION = ".exe";

    private readonly IUrls _urls;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddIpAddressCommand))]
    private string? _currentIpAddress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SplitTunnelingFeatureIconSource))]
    private bool _isSplitTunnelingEnabled;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.SplitTunnelingMode))]
    [NotifyPropertyChangedFor(nameof(IsStandardSplitTunneling))]
    [NotifyPropertyChangedFor(nameof(IsInverseSplitTunneling))]    
    [NotifyPropertyChangedFor(nameof(HasStandardApps))]
    [NotifyPropertyChangedFor(nameof(HasInverseApps))]
    [NotifyPropertyChangedFor(nameof(ActiveAppsCount))]
    [NotifyPropertyChangedFor(nameof(HasStandardIpAddresses))]
    [NotifyPropertyChangedFor(nameof(HasInverseIpAddresses))]
    [NotifyPropertyChangedFor(nameof(ActiveIpAddressesCount))]
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

    [property: SettingName(nameof(ISettings.SplitTunnelingStandardAppsList))]
    public ObservableCollection<SplitTunnelingAppViewModel> StandardApps { get; }

    [property: SettingName(nameof(ISettings.SplitTunnelingInverseAppsList))]
    public ObservableCollection<SplitTunnelingAppViewModel> InverseApps { get; }

    public bool HasStandardApps => CurrentSplitTunnelingMode == SplitTunnelingMode.Standard && StandardApps.Any();
    public bool HasInverseApps => CurrentSplitTunnelingMode == SplitTunnelingMode.Inverse && InverseApps.Any();

    public int ActiveAppsCount => CurrentSplitTunnelingMode == SplitTunnelingMode.Standard 
                                      ? StandardApps.Count(a => a.IsActive)
                                      : InverseApps.Count(a => a.IsActive);

    [property: SettingName(nameof(ISettings.SplitTunnelingStandardIpAddressesList))]
    public ObservableCollection<SplitTunnelingIpAddressViewModel> StandardIpAddresses { get; }

    [property: SettingName(nameof(ISettings.SplitTunnelingInverseIpAddressesList))]
    public ObservableCollection<SplitTunnelingIpAddressViewModel> InverseIpAddresses { get; }

    public bool HasStandardIpAddresses => CurrentSplitTunnelingMode == SplitTunnelingMode.Standard && StandardIpAddresses.Any();
    public bool HasInverseIpAddresses => CurrentSplitTunnelingMode == SplitTunnelingMode.Inverse && InverseIpAddresses.Any();

    public int ActiveIpAddressesCount => CurrentSplitTunnelingMode == SplitTunnelingMode.Standard 
                                             ? StandardIpAddresses.Count(ip => ip.IsActive) 
                                             : InverseIpAddresses.Count(ip => ip.IsActive);

    public SplitTunnelingViewModel(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IUrls urls)
        : base(viewNavigator, localizationProvider, settings, settingsConflictResolver, connectionManager)
    {
        _urls = urls;

        StandardApps = new();
        StandardApps.CollectionChanged += OnAppsCollectionChanged;

        InverseApps = new();
        InverseApps.CollectionChanged += OnAppsCollectionChanged;

        StandardIpAddresses = new();
        StandardIpAddresses.CollectionChanged += OnIpAddressesCollectionChanged;

        InverseIpAddresses = new();
        InverseIpAddresses.CollectionChanged += OnIpAddressesCollectionChanged;
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
        ObservableCollection<SplitTunnelingAppViewModel> apps = GetApps();
        string filePath = await ViewNavigator.Window.PickSingleFileAsync(Localizer.Get("Settings_Features_SplitTunneling_Apps_FilesFilterName"), new string[] { EXE_FILE_EXTENSION });
        if (!IsValidAppPath(filePath))
        {
            return;
        }

        SplitTunnelingAppViewModel? app = apps.FirstOrDefault(a => IsSameAppPath(a.AppFilePath, filePath) || a.AlternateAppFilePaths.Any(alt => IsSameAppPath(alt, filePath)));
        if (app != null)
        {
            app.IsActive = true;
        }
        else
        {
            apps.Add(await CreateAppFromPathAsync(filePath, true, null));
        }
    }

    private ObservableCollection<SplitTunnelingAppViewModel> GetApps()
    {
        return CurrentSplitTunnelingMode == SplitTunnelingMode.Standard ? StandardApps : InverseApps;
    }

    [RelayCommand(CanExecute = nameof(CanAddIpAddress))]
    public void AddIpAddress()
    {
        ObservableCollection<SplitTunnelingIpAddressViewModel> ipAddresses = GetIpAddresses();
        SplitTunnelingIpAddressViewModel? ipAddress = ipAddresses.FirstOrDefault(s => s.IpAddress == CurrentIpAddress);
        if (ipAddress != null)
        {
            ipAddress.IsActive = true;
        }
        else
        {
            ipAddresses.Add(new(Localizer, this, CurrentIpAddress!));
        }

        CurrentIpAddress = string.Empty;
    }

    private ObservableCollection<SplitTunnelingIpAddressViewModel> GetIpAddresses()
    {
        return CurrentSplitTunnelingMode == SplitTunnelingMode.Standard ? StandardIpAddresses : InverseIpAddresses;
    }

    public bool CanAddIpAddress()
    {
        return !string.IsNullOrEmpty(CurrentIpAddress)
            && CurrentIpAddress.IsValidIpAddress();
    }

    public void RemoveApp(SplitTunnelingAppViewModel app)
    {
        ObservableCollection<SplitTunnelingAppViewModel> apps = GetApps();
        apps.Remove(app);
    }

    public void InvalidateAppsCount()
    {
        OnPropertyChanged(nameof(ActiveAppsCount));
    }

    public void RemoveIpAddress(SplitTunnelingIpAddressViewModel ipAddress)
    {
        ObservableCollection<SplitTunnelingIpAddressViewModel> ipAddresses = GetIpAddresses();
        ipAddresses.Remove(ipAddress);
    }

    public void InvalidateIpAddressesCount()
    {
        OnPropertyChanged(nameof(ActiveIpAddressesCount));
    }

    protected override void SaveSettings()
    {
        Settings.IsSplitTunnelingEnabled = IsSplitTunnelingEnabled;
        Settings.SplitTunnelingMode = CurrentSplitTunnelingMode;
        Settings.SplitTunnelingStandardAppsList = GetSplitTunnelingAppsList(StandardApps);
        Settings.SplitTunnelingInverseAppsList = GetSplitTunnelingAppsList(InverseApps);
        Settings.SplitTunnelingStandardIpAddressesList = GetSplitTunnelingIpAddressesList(StandardIpAddresses);
        Settings.SplitTunnelingInverseIpAddressesList = GetSplitTunnelingIpAddressesList(InverseIpAddresses);
    }

    private List<SplitTunnelingApp> GetSplitTunnelingAppsList(ObservableCollection<SplitTunnelingAppViewModel> apps)
    {
        return apps.Select(app => new SplitTunnelingApp(app.AppFilePath, app.IsActive)).ToList();
    }

    private List<SplitTunnelingIpAddress> GetSplitTunnelingIpAddressesList(ObservableCollection<SplitTunnelingIpAddressViewModel> ipAddresses)
    {
        return ipAddresses.Select(ip => new SplitTunnelingIpAddress(ip.IpAddress, ip.IsActive)).ToList();
    }

    protected override async void RetrieveSettings()
    {
        IsSplitTunnelingEnabled = Settings.IsSplitTunnelingEnabled;
        CurrentSplitTunnelingMode = Settings.SplitTunnelingMode;

        await SetAppsAsync(StandardApps, Settings.SplitTunnelingStandardAppsList);
        await SetAppsAsync(InverseApps, Settings.SplitTunnelingInverseAppsList);

        SetIpAddresses(StandardIpAddresses, Settings.SplitTunnelingStandardIpAddressesList);
        SetIpAddresses(InverseIpAddresses, Settings.SplitTunnelingInverseIpAddressesList);
    }

    private async Task SetAppsAsync(ObservableCollection<SplitTunnelingAppViewModel> apps, List<SplitTunnelingApp> settingsApps)
    {
        apps.Clear();
        foreach (SplitTunnelingApp app in settingsApps)
        {
            apps.Add(await CreateAppFromPathAsync(app.AppFilePath, app.IsActive, app.AlternateAppFilePaths));
        }
    }

    private void SetIpAddresses(ObservableCollection<SplitTunnelingIpAddressViewModel> ipAddresses, List<SplitTunnelingIpAddress> settingsIpAddresses)
    {
        ipAddresses.Clear();
        foreach (SplitTunnelingIpAddress ip in settingsIpAddresses)
        {
            ipAddresses.Add(new(Localizer, this, ip.IpAddress, ip.IsActive));
        }
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        yield return new(nameof(ISettings.IsSplitTunnelingEnabled), IsSplitTunnelingEnabled,
            Settings.IsSplitTunnelingEnabled != IsSplitTunnelingEnabled);

        yield return new(nameof(ISettings.SplitTunnelingMode), CurrentSplitTunnelingMode,
            Settings.SplitTunnelingMode != CurrentSplitTunnelingMode);

        yield return new(nameof(ISettings.SplitTunnelingStandardAppsList), GetSplitTunnelingAppsList(StandardApps),
            !Settings.SplitTunnelingStandardAppsList.SequenceEqual(GetSplitTunnelingAppsList(StandardApps)));

        yield return new(nameof(ISettings.SplitTunnelingInverseAppsList), GetSplitTunnelingAppsList(InverseApps),
            !Settings.SplitTunnelingInverseAppsList.SequenceEqual(GetSplitTunnelingAppsList(InverseApps)));

        yield return new(nameof(ISettings.SplitTunnelingStandardIpAddressesList), GetSplitTunnelingIpAddressesList(StandardIpAddresses),
            !Settings.SplitTunnelingStandardIpAddressesList.SequenceEqual(GetSplitTunnelingIpAddressesList(StandardIpAddresses)));

        yield return new(nameof(ISettings.SplitTunnelingInverseIpAddressesList), GetSplitTunnelingIpAddressesList(InverseIpAddresses),
            !Settings.SplitTunnelingInverseIpAddressesList.SequenceEqual(GetSplitTunnelingIpAddressesList(InverseIpAddresses)));
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
        OnPropertyChanged(nameof(HasStandardApps));
        OnPropertyChanged(nameof(HasInverseApps));
        InvalidateAppsCount();
    }

    private void OnIpAddressesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasStandardIpAddresses));
        OnPropertyChanged(nameof(HasInverseIpAddresses));
        InvalidateIpAddressesCount();
    }

    public void OnIpAddressKeyDownHandler(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && CanAddIpAddress())
        {
            AddIpAddressCommand.Execute(null);
        }
    }
}