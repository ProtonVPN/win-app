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

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Models;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Client.UI.Overlays.Selection.Contracts;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.UI.Main.Settings.Connection;

public partial class SplitTunnelingPageViewModel : SettingsPageViewModelBase
{
    private const int MAX_FOLDERS = 50;

    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IIpSelector _ipSelector;
    private readonly IAppSelector _appSelector;
    private readonly IMainWindowActivator _mainWindowActivator;

    private bool _wasIpv6WarningDisplayed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasIpv6AddressesWhileIpv6Disabled))]
    private bool _isIpv6Enabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SplitTunnelingFeatureIconSource))]
    [NotifyPropertyChangedFor(nameof(HasIpv6AddressesWhileIpv6Disabled))]
    private bool _isSplitTunnelingEnabled;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.SplitTunnelingMode))]
    [NotifyPropertyChangedFor(nameof(IsStandardSplitTunneling))]
    [NotifyPropertyChangedFor(nameof(IsInverseSplitTunneling))]
    [NotifyPropertyChangedFor(nameof(HasStandardFolders))]
    [NotifyPropertyChangedFor(nameof(HasInverseFolders))]
    [NotifyPropertyChangedFor(nameof(ActiveFoldersCount))]
    [NotifyPropertyChangedFor(nameof(SplitTunnelingFeatureIconSource))]
    [NotifyPropertyChangedFor(nameof(IpAddresses))]
    [NotifyPropertyChangedFor(nameof(IpAddressesHeader))]
    [NotifyPropertyChangedFor(nameof(SelectedIpAddresses))]
    [NotifyPropertyChangedFor(nameof(HasSelectedIpAddresses))]
    [NotifyPropertyChangedFor(nameof(Apps))]
    [NotifyPropertyChangedFor(nameof(AppsHeader))]
    [NotifyPropertyChangedFor(nameof(SelectedApps))]
    [NotifyPropertyChangedFor(nameof(HasSelectedApps))]
    [NotifyPropertyChangedFor(nameof(HasIpv6AddressesWhileIpv6Disabled))]
    private SplitTunnelingMode _currentSplitTunnelingMode;

    [ObservableProperty]
    private bool _isLoading;

    public override string Title => Localizer.Get("Settings_Connection_SplitTunneling");

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

    // Folders
    [property: SettingName(nameof(ISettings.SplitTunnelingStandardFoldersList))]
    public ObservableCollection<SplitTunnelingFolderViewModel> StandardFolders { get; }

    [property: SettingName(nameof(ISettings.SplitTunnelingInverseFoldersList))]
    public ObservableCollection<SplitTunnelingFolderViewModel> InverseFolders { get; }

    public bool HasStandardFolders => CurrentSplitTunnelingMode == SplitTunnelingMode.Standard && StandardFolders.Any();
    public bool HasInverseFolders => CurrentSplitTunnelingMode == SplitTunnelingMode.Inverse && InverseFolders.Any();

    public int ActiveFoldersCount => CurrentSplitTunnelingMode == SplitTunnelingMode.Standard
                                      ? StandardFolders.Count(a => a.IsActive)
                                      : InverseFolders.Count(a => a.IsActive);

    public bool CanAddFolder => StandardFolders.Count + InverseFolders.Count < MAX_FOLDERS;

    // IP Addresses
    [property: SettingName(nameof(ISettings.SplitTunnelingStandardIpAddressesList))]
    public SmartObservableCollection<SelectableNetworkAddress> ExcludedIpAddresses { get; } = [];

    [property: SettingName(nameof(ISettings.SplitTunnelingInverseIpAddressesList))]
    public SmartObservableCollection<SelectableNetworkAddress> IncludedIpAddresses { get; } = [];

    public SmartObservableCollection<SelectableNetworkAddress> IpAddresses
        => IsStandardSplitTunneling ? ExcludedIpAddresses : IncludedIpAddresses;

    public IEnumerable<NetworkAddress> SelectedIpAddresses
        => IpAddresses.Where(ip => ip.IsSelected).Select(ip => ip.Value);

    public bool HasSelectedIpAddresses => SelectedIpAddresses.Any();

    public bool HasIpv6AddressesWhileIpv6Disabled
        => IsSplitTunnelingEnabled
        && IsInverseSplitTunneling
        && !IsIpv6Enabled
        && SelectedIpAddresses.Any(ip => ip.IsIpV6);

    public string IpAddressesHeader => Localizer.GetFormat(IsStandardSplitTunneling
        ? "Settings_Connection_SplitTunneling_IpAddresses_Excluded_FormattedHeader"
        : "Settings_Connection_SplitTunneling_IpAddresses_Included_FormattedHeader", SelectedIpAddresses.Count());

    // Apps
    [property: SettingName(nameof(ISettings.SplitTunnelingStandardAppsList))]
    public SmartObservableCollection<SelectableTunnelingApp> ExcludedApps { get; } = [];

    [property: SettingName(nameof(ISettings.SplitTunnelingInverseAppsList))]
    public SmartObservableCollection<SelectableTunnelingApp> IncludedApps { get; } = [];

    public SmartObservableCollection<SelectableTunnelingApp> Apps
        => IsStandardSplitTunneling ? ExcludedApps : IncludedApps;

    public IEnumerable<TunnelingApp> SelectedApps
        => Apps.Where(app => app.IsSelected && app.Value.IsValid).Select(app => app.Value);

    public bool HasSelectedApps => SelectedApps.Any();

    public string AppsHeader => Localizer.GetFormat(IsStandardSplitTunneling
        ? "Settings_Connection_SplitTunneling_Apps_Excluded_FormattedHeader"
        : "Settings_Connection_SplitTunneling_Apps_Included_FormattedHeader", SelectedApps.Count());

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
        IIpSelector ipSelector,
        IAppSelector appSelector,
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
        _ipSelector = ipSelector;
        _appSelector = appSelector;

        StandardFolders = new();
        StandardFolders.CollectionChanged += OnFoldersCollectionChanged;

        InverseFolders = new();
        InverseFolders.CollectionChanged += OnFoldersCollectionChanged;

        ExcludedIpAddresses.CollectionChanged += OnIpAddressesCollectionChanged;
        IncludedIpAddresses.CollectionChanged += OnIpAddressesCollectionChanged;
        ExcludedApps.CollectionChanged += OnAppsCollectionChanged;
        IncludedApps.CollectionChanged += OnAppsCollectionChanged;

        PageSettings =
        [
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingStandardAppsList, () => GetSettingsApps(ExcludedApps)),
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingStandardFoldersList, () => GetSplitTunnelingFoldersList(StandardFolders)),
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingStandardIpAddressesList, () => GetSettingsIpAddresses(ExcludedIpAddresses)),
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingInverseAppsList, () => GetSettingsApps(IncludedApps)),
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingInverseFoldersList, () => GetSplitTunnelingFoldersList(InverseFolders)),
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingInverseIpAddressesList, () => GetSettingsIpAddresses(IncludedIpAddresses)),
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingMode, () => CurrentSplitTunnelingMode),
            ChangedSettingArgs.Create(() => Settings.IsSplitTunnelingEnabled, () => IsSplitTunnelingEnabled),
            ChangedSettingArgs.Create(() => Settings.IsIpv6Enabled, () => IsIpv6Enabled)
        ];
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
    public async Task TriggerIpv6DisabledWarningAsync()
    {
        if (await ShowIpv6DisabledWarningAsync())
        {
            IsIpv6Enabled = true;
        }

        // Show this warning only once per app launch
        _wasIpv6WarningDisplayed = true;
    }

    [RelayCommand]
    public async Task AddFolderAsync()
    {
        if (_mainWindowActivator.Window == null)
        {
            return;
        }

        if (!CanAddFolder)
        {
            return;
        }

        ObservableCollection<SplitTunnelingFolderViewModel> folders = GetFolders();
        string? folderPath = await _mainWindowActivator.Window.PickSingleFolderAsync();

        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            return;
        }

        SplitTunnelingFolderViewModel? existing = folders.FirstOrDefault(f =>
            string.Equals(f.FolderPath, folderPath, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            existing.IsActive = true;
        }
        else
        {
            folders.Add(new SplitTunnelingFolderViewModel(ViewModelHelper, this, folderPath, true));
        }
    }

    [RelayCommand]
    public async Task SelectIpsAsync()
    {
        _ipSelector.Title = Localizer.Get(IsStandardSplitTunneling
            ? "Settings_Connection_SplitTunneling_IpAddresses_Excluded_Header"
            : "Settings_Connection_SplitTunneling_IpAddresses_Included_Header");
        _ipSelector.Description = Localizer.Get(IsStandardSplitTunneling
            ? "Settings_Connection_SplitTunneling_IpAddresses_Excluded_Description"
            : "Settings_Connection_SplitTunneling_IpAddresses_Included_Description");
        _ipSelector.Caption = Localizer.Get("Settings_Connection_SplitTunneling_IpAddresses_AddNew");
        _ipSelector.CanReorder = false;
        _ipSelector.IsAddressRangeAuthorized = true;

        List<SelectableNetworkAddress>? result = await _ipSelector.SelectAsync(IpAddresses.Select(ip => ip.Clone()).ToList());
        if (result != null)
        {
            IpAddresses.Reset(result);

            if (HasIpv6AddressesWhileIpv6Disabled && !_wasIpv6WarningDisplayed)
            {
                await TriggerIpv6DisabledWarningAsync();
            }
        }
    }

    [RelayCommand]
    public async Task SelectAppsAsync()
    {
        _appSelector.Title = Localizer.Get(IsStandardSplitTunneling
            ? "Settings_Connection_SplitTunneling_Apps_Excluded_Header"
            : "Settings_Connection_SplitTunneling_Apps_Included_Header");
        _appSelector.Description = Localizer.Get(IsStandardSplitTunneling
            ? "Settings_Connection_SplitTunneling_Apps_Excluded_Description"
            : "Settings_Connection_SplitTunneling_Apps_Included_Description");

        List<SelectableTunnelingApp>? result = await _appSelector.SelectAsync(Apps.Select(app => app.Clone()).ToList());
        if (result != null)
        {
            Apps.Reset(result);
        }
    }

    public void RemoveFolder(SplitTunnelingFolderViewModel folder)
    {
        GetFolders().Remove(folder);
    }

    public void InvalidateFoldersCount()
    {
        OnPropertyChanged(nameof(ActiveFoldersCount));
    }

    protected override async Task OnRetrieveSettingsAsync()
    {
        try
        {
            IsLoading = true;

            IsIpv6Enabled = Settings.IsIpv6Enabled;
            IsSplitTunnelingEnabled = Settings.IsSplitTunnelingEnabled;
            CurrentSplitTunnelingMode = Settings.SplitTunnelingMode;

            SetFolders(StandardFolders, Settings.SplitTunnelingStandardFoldersList);
            SetFolders(InverseFolders, Settings.SplitTunnelingInverseFoldersList);

            ExcludedIpAddresses.Reset(GetObservableIpAddresses(Settings.SplitTunnelingStandardIpAddressesList));
            IncludedIpAddresses.Reset(GetObservableIpAddresses(Settings.SplitTunnelingInverseIpAddressesList));

            ExcludedApps.Reset(await GetObservableAppsAsync(Settings.SplitTunnelingStandardAppsList));
            IncludedApps.Reset(await GetObservableAppsAsync(Settings.SplitTunnelingInverseAppsList));
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected override bool IsReconnectionRequiredDueToChanges(IEnumerable<ChangedSettingArgs> changedSettings)
    {
        bool isReconnectionRequired = base.IsReconnectionRequiredDueToChanges(changedSettings);
        if (isReconnectionRequired)
        {
            bool isSameSplitTunnelingMode = CurrentSplitTunnelingMode == Settings.SplitTunnelingMode;
            bool hadAnyActiveAppsOrIps =
                Settings.IsSplitTunnelingEnabled &&
                Settings.SplitTunnelingMode switch
                {
                    SplitTunnelingMode.Standard => Settings.SplitTunnelingStandardAppsList.Any(app => app.IsActive)
                                                || Settings.SplitTunnelingStandardFoldersList.Any(f => f.IsActive)
                                                || Settings.SplitTunnelingStandardIpAddressesList.Any(ip => ip.IsActive),
                    SplitTunnelingMode.Inverse => Settings.SplitTunnelingInverseAppsList.Any(app => app.IsActive)
                                               || Settings.SplitTunnelingInverseFoldersList.Any(f => f.IsActive)
                                               || Settings.SplitTunnelingInverseIpAddressesList.Any(ip => ip.IsActive),
                    _ => false
                };
            bool hasAnyActiveAppsOrIps =
                IsSplitTunnelingEnabled &&
                CurrentSplitTunnelingMode switch
                {
                    SplitTunnelingMode.Standard => ExcludedApps.Any(app => app.IsSelected)
                                                || StandardFolders.Any(f => f.IsActive)
                                                || ExcludedIpAddresses.Any(ip => ip.IsSelected),
                    SplitTunnelingMode.Inverse => IncludedApps.Any(app => app.IsSelected)
                                               || InverseFolders.Any(f => f.IsActive)
                                               || IncludedIpAddresses.Any(ip => ip.IsSelected),
                    _ => false
                };
            if (isSameSplitTunnelingMode && !hadAnyActiveAppsOrIps && !hasAnyActiveAppsOrIps)
            {
                return false;
            }
        }

        return isReconnectionRequired;
    }

    private ObservableCollection<SplitTunnelingFolderViewModel> GetFolders()
    {
        return CurrentSplitTunnelingMode == SplitTunnelingMode.Standard ? StandardFolders : InverseFolders;
    }

    private void SetFolders(ObservableCollection<SplitTunnelingFolderViewModel> folders, List<SplitTunnelingFolder> settingsFolders)
    {
        folders.Clear();
        foreach (SplitTunnelingFolder folder in settingsFolders)
        {
            folders.Add(new SplitTunnelingFolderViewModel(ViewModelHelper, this, folder.FolderPath, folder.IsActive));
        }
    }

    private List<SplitTunnelingFolder> GetSplitTunnelingFoldersList(ObservableCollection<SplitTunnelingFolderViewModel> folders)
    {
        return folders.Select(f => new SplitTunnelingFolder(f.FolderPath, f.IsActive)).ToList();
    }

    private List<SplitTunnelingIpAddress> GetSettingsIpAddresses(IEnumerable<SelectableNetworkAddress> ipAddresses)
    {
        return ipAddresses.Select(ip => new SplitTunnelingIpAddress(ip.Value.ToString(), ip.IsSelected)).ToList();
    }

    private List<SelectableNetworkAddress> GetObservableIpAddresses(List<SplitTunnelingIpAddress> settingsIpAddresses)
    {
        List<SelectableNetworkAddress> addresses = [];
        foreach (SplitTunnelingIpAddress ip in settingsIpAddresses)
        {
            if (NetworkAddress.TryParse(ip.IpAddress, out NetworkAddress address))
            {
                addresses.Add(new SelectableNetworkAddress(address, ip.IsActive));
            }
        }
        return addresses;
    }

    private List<SplitTunnelingApp> GetSettingsApps(IEnumerable<SelectableTunnelingApp> apps)
    {
        return apps.Select(ip => new SplitTunnelingApp(ip.Value.AppPath, ip.Value.AlternateAppPaths, ip.IsSelected)).ToList();
    }

    private async Task<List<SelectableTunnelingApp>> GetObservableAppsAsync(List<SplitTunnelingApp> settingsApps)
    {
        List<SelectableTunnelingApp> apps = [];
        foreach (SplitTunnelingApp app in settingsApps)
        {
            TunnelingApp tunnelingApp = await TunnelingApp.TryCreateAsync(app.AppFilePath, app.AlternateAppFilePaths)
                ?? TunnelingApp.NotFound(app.AppFilePath, Localizer.Get("Common_Message_AppNotFound"), app.AlternateAppFilePaths);
            apps.Add(new SelectableTunnelingApp(tunnelingApp, app.IsActive));
        }
        return apps;
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
        OnPropertyChanged(nameof(Apps));
        OnPropertyChanged(nameof(AppsHeader));
        OnPropertyChanged(nameof(SelectedApps));
        OnPropertyChanged(nameof(HasSelectedApps));
    }

    private void OnFoldersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasStandardFolders));
        OnPropertyChanged(nameof(HasInverseFolders));
        OnPropertyChanged(nameof(CanAddFolder));
        InvalidateFoldersCount();
    }

    private void OnIpAddressesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IpAddresses));
        OnPropertyChanged(nameof(IpAddressesHeader));
        OnPropertyChanged(nameof(SelectedIpAddresses));
        OnPropertyChanged(nameof(HasSelectedIpAddresses));
        OnPropertyChanged(nameof(HasIpv6AddressesWhileIpv6Disabled));
    }
}
