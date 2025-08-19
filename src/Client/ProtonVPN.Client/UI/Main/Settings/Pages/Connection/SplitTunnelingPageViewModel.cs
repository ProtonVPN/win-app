/*
 * Copyright (c) 2024 Proton AG
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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Common.Core.Networking;
using Windows.System;

namespace ProtonVPN.Client.UI.Main.Settings.Connection;

public partial class SplitTunnelingPageViewModel : SettingsPageViewModelBase
{
    public override string Title => Localizer.Get("Settings_Connection_SplitTunneling");

    private const string EXE_FILE_EXTENSION = ".exe";

    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IMainWindowActivator _mainWindowActivator;

    [ObservableProperty]
    private string? _currentIpAddress;

    [ObservableProperty]
    private string? _ipAddressError;

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
    [NotifyPropertyChangedFor(nameof(SplitTunnelingFeatureIconSource))]
    private SplitTunnelingMode _currentSplitTunnelingMode;

    private bool _wasIpv6WarningDisplayed;

    public ImageSource SplitTunnelingFeatureIconSource => GetFeatureIconSource(IsSplitTunnelingEnabled, CurrentSplitTunnelingMode);

    public string LearnMoreUrl => _urlsBrowser.SplitTunnelingLearnMore;

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

    public SplitTunnelingPageViewModel(
        IUrlsBrowser urlsBrowser,
        IMainWindowActivator mainWindowActivator,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
        _mainWindowActivator = mainWindowActivator;

        StandardApps = new();
        StandardApps.CollectionChanged += OnAppsCollectionChanged;

        InverseApps = new();
        InverseApps.CollectionChanged += OnAppsCollectionChanged;

        StandardIpAddresses = new();
        StandardIpAddresses.CollectionChanged += OnIpAddressesCollectionChanged;

        InverseIpAddresses = new();
        InverseIpAddresses.CollectionChanged += OnIpAddressesCollectionChanged;

        PageSettings =
        [
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingStandardAppsList, () => GetSplitTunnelingAppsList(StandardApps)),
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingStandardIpAddressesList, () => GetSplitTunnelingIpAddressesList(StandardIpAddresses)),
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingInverseAppsList, () => GetSplitTunnelingAppsList(InverseApps)),
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingInverseIpAddressesList, () => GetSplitTunnelingIpAddressesList(InverseIpAddresses)),
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingMode, () => CurrentSplitTunnelingMode),
            ChangedSettingArgs.Create(() => Settings.IsSplitTunnelingEnabled, () => IsSplitTunnelingEnabled)
        ];
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        ResetCurrentIpAddress();
        ResetIpAddressError();
    }

    public static ImageSource GetFeatureIconSource(bool isEnabled, SplitTunnelingMode mode)
    {
        if (!isEnabled)
        {
            return ResourceHelper.GetIllustration("SplitTunnelingOffIllustrationSource");
        }

        return mode switch
        {
            SplitTunnelingMode.Standard => ResourceHelper.GetIllustration("SplitTunnelingStandardIllustrationSource"),
            SplitTunnelingMode.Inverse => ResourceHelper.GetIllustration("SplitTunnelingInverseIllustrationSource"),
            _ => throw new ArgumentOutOfRangeException(nameof(mode)),
        };
    }

    [RelayCommand]
    public async Task AddAppAsync()
    {
        if (_mainWindowActivator.Window == null)
        {
            return;
        }

        ObservableCollection<SplitTunnelingAppViewModel> apps = GetApps();
        string filePath = await _mainWindowActivator.Window.PickSingleFileAsync(Localizer.Get("Settings_Connection_SplitTunneling_Apps_FilesFilterName"), [EXE_FILE_EXTENSION]);
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

    [RelayCommand]
    public async Task AddIpAddressAsync()
    {
        NetworkAddress? address = GetValidatedCurrentIpAddress();
        string? error = GetIpAddressError(address);

        if (error != null || address == null)
        {
            IpAddressError = error ?? Localizer.Get("Settings_Common_IpAddresses_Invalid");
            return;
        }

        ObservableCollection<SplitTunnelingIpAddressViewModel> ipAddresses = GetIpAddresses();
        ipAddresses.Add(new(ViewModelHelper, Settings, this, address.Value.FormattedAddress));

        ResetCurrentIpAddress();
        ResetIpAddressError();

        if (address?.IsIpV6 == true && !Settings.IsIpv6Enabled && !_wasIpv6WarningDisplayed)
        {
            await ShowIpv6DisabledWarningInnerAsync();

            // Show this warning only once per app launch
            _wasIpv6WarningDisplayed = true;
        }
    }

    [RelayCommand]
    public Task TriggerIpv6DisabledWarningAsync()
    {
        return ShowIpv6DisabledWarningInnerAsync();
    }

    private async Task ShowIpv6DisabledWarningInnerAsync()
    {
        if (CurrentSplitTunnelingMode != SplitTunnelingMode.Inverse)
        {
            return;
        }

        await ShowIpv6DisabledWarningAsync();
    }

    protected override void OnIpv6WarningClosedWithPrimaryAction()
    {
        List<SplitTunnelingIpAddressViewModel> inverseIpAddresses = InverseIpAddresses.ToList();
        InverseIpAddresses.Clear();

        foreach (SplitTunnelingIpAddressViewModel ip in inverseIpAddresses)
        {
            InverseIpAddresses.Add(new(ViewModelHelper, Settings, this, ip.IpAddress, ip.IsActive));
        }
    }

    private string? GetIpAddressError(NetworkAddress? address)
    {
        if (address == null)
        {
            return Localizer.Get("Settings_Common_IpAddresses_Invalid");
        }

        if (GetIpAddresses().FirstOrDefault(ip => ip.IpAddress == address.Value.FormattedAddress) != null)
        {
            return Localizer.Get("Settings_Common_IpAddresses_AlreadyExists");
        }

        return null;
    }

    private ObservableCollection<SplitTunnelingIpAddressViewModel> GetIpAddresses()
    {
        return CurrentSplitTunnelingMode == SplitTunnelingMode.Standard ? StandardIpAddresses : InverseIpAddresses;
    }

    private NetworkAddress? GetValidatedCurrentIpAddress()
    {
        return NetworkAddress.TryParse(CurrentIpAddress, out NetworkAddress address) ? address : null;
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

    private List<SplitTunnelingApp> GetSplitTunnelingAppsList(ObservableCollection<SplitTunnelingAppViewModel> apps)
    {
        return apps.Select(app => new SplitTunnelingApp(app.AppFilePath, app.IsActive)).ToList();
    }

    private List<SplitTunnelingIpAddress> GetSplitTunnelingIpAddressesList(ObservableCollection<SplitTunnelingIpAddressViewModel> ipAddresses)
    {
        return ipAddresses.Select(ip => new SplitTunnelingIpAddress(ip.IpAddress, ip.IsActive)).ToList();
    }

    protected override async Task OnRetrieveSettingsAsync()
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
            ipAddresses.Add(new(ViewModelHelper, Settings, this, ip.IpAddress, ip.IsActive));
        }
    }

    private async Task<SplitTunnelingAppViewModel> CreateAppFromPathAsync(string filePath, bool isActive, List<string>? alternateFilePaths)
    {
        SplitTunnelingAppViewModel app = new(ViewModelHelper, this, filePath, isActive, alternateFilePaths);
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
        if (e.Key == VirtualKey.Enter)
        {
            AddIpAddressCommand.Execute(null);
        }
    }

    protected override bool IsReconnectionRequiredDueToChanges(IEnumerable<ChangedSettingArgs> changedSettings)
    {
        bool isReconnectionRequired = base.IsReconnectionRequiredDueToChanges(changedSettings);
        if (isReconnectionRequired)
        {
            // Check if there was any active apps or IP adresses from the settings
            // then check if there is any active apps or IP adresses now.
            // If there is none in both case, no need to reconnect.
            bool isSameSplitTunnelingMode = CurrentSplitTunnelingMode == Settings.SplitTunnelingMode;
            bool hadAnyActiveAppsOrIps =
                Settings.IsSplitTunnelingEnabled &&
                Settings.SplitTunnelingMode switch
                {
                    SplitTunnelingMode.Standard => Settings.SplitTunnelingStandardAppsList.Any(s => s.IsActive) || Settings.SplitTunnelingStandardIpAddressesList.Any(s => s.IsActive),
                    SplitTunnelingMode.Inverse => Settings.SplitTunnelingInverseAppsList.Any(s => s.IsActive) || Settings.SplitTunnelingInverseIpAddressesList.Any(s => s.IsActive),
                    _ => false
                };
            bool hasAnyActiveAppsOrIps =
                IsSplitTunnelingEnabled &&
                CurrentSplitTunnelingMode switch
                {
                    SplitTunnelingMode.Standard => StandardApps.Any(s => s.IsActive) || StandardIpAddresses.Any(s => s.IsActive),
                    SplitTunnelingMode.Inverse => InverseApps.Any(s => s.IsActive) || InverseIpAddresses.Any(s => s.IsActive),
                    _ => false
                };
            if (isSameSplitTunnelingMode && !hadAnyActiveAppsOrIps && !hasAnyActiveAppsOrIps)
            {
                return false;
            }
        }

        return isReconnectionRequired;
    }

    partial void OnCurrentIpAddressChanged(string? value)
    {
        ResetIpAddressError();
    }

    private void ResetIpAddressError()
    {
        IpAddressError = null;
    }

    private void ResetCurrentIpAddress()
    {
        CurrentIpAddress = null;
    }
}