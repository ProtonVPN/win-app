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

using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Contracts.Profiles;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Models;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Features.Bases;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Client.UI.Main.Settings.Connection;
using ProtonVPN.Client.UI.Overlays.Selection.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.UI.Main.Features.SplitTunneling;

public partial class SplitTunnelingWidgetViewModel : FeatureWidgetViewModelBase
{
    private readonly Lazy<List<ChangedSettingArgs>> _disableSplitTunnelingSettings;
    private readonly Lazy<List<ChangedSettingArgs>> _enableStandardSplitTunnelingSettings;
    private readonly Lazy<List<ChangedSettingArgs>> _enableInverseSplitTunnelingSettings;
    private readonly Lazy<List<ChangedSettingArgs>> _modifySplitTunnelingStandardAppsList;
    private readonly Lazy<List<ChangedSettingArgs>> _modifySplitTunnelingStandardIpAddressesList;
    private readonly Lazy<List<ChangedSettingArgs>> _modifySplitTunnelingInverseAppsList;
    private readonly Lazy<List<ChangedSettingArgs>> _modifySplitTunnelingInverseIpAddressesList;
    private readonly Lazy<List<ChangedSettingArgs>> _enableIpv6Settings;

    private readonly IAppSelector _appSelector;
    private readonly IIpSelector _ipSelector;

    private bool _wasIpv6WarningDisplayed;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasIpv6AddressesWhileIpv6Disabled))]
    private bool _isIpv6Enabled;

    public override string Header => Localizer.Get("Settings_Connection_SplitTunneling");

    public string InfoMessage => !ConnectionManager.IsConnected || !Settings.IsSplitTunnelingEnabled
        ? Localizer.Get("Flyouts_SplitTunneling_Info")
        : Localizer.Get(Settings.SplitTunnelingMode switch
        {
            SplitTunnelingMode.Standard => "Flyouts_SplitTunneling_Standard_Info",
            SplitTunnelingMode.Inverse => "Flyouts_SplitTunneling_Inverse_Info",
            _ => "Flyouts_SplitTunneling_Info"
        });

    public bool IsInfoMessageVisible => true;

    public bool IsSplitTunnelingComponentVisible => IsSplitTunnelingEnabled;

    public bool IsSplitTunnelingComponentDimmed => !ConnectionManager.IsConnected;

    public bool IsSplitTunnelingEnabled => Settings.IsSplitTunnelingEnabled;

    public SplitTunnelingMode SplitTunnelingMode => Settings.SplitTunnelingMode;

    public bool IsStandardSplitTunneling => SplitTunnelingMode == SplitTunnelingMode.Standard;

    public bool IsInverseSplitTunneling => SplitTunnelingMode == SplitTunnelingMode.Inverse;

    public bool IsStandardSplitTunnelingEnabled => IsSplitTunnelingEnabled && IsStandardSplitTunneling;

    public bool IsInverseSplitTunnelingEnabled => IsSplitTunnelingEnabled && IsInverseSplitTunneling;

    public SmartObservableCollection<SelectableNetworkAddress> IncludedIpAddresses { get; } = [];
    public SmartObservableCollection<SelectableNetworkAddress> ExcludedIpAddresses { get; } = [];

    public SmartObservableCollection<SelectableNetworkAddress> IpAddresses
        => IsStandardSplitTunneling ? ExcludedIpAddresses : IncludedIpAddresses;

    public IEnumerable<NetworkAddress> SelectedIpAddresses
        => IpAddresses.Where(ip => ip.IsSelected).Select(ip => ip.Value);

    public bool HasSelectedIpAddresses => SelectedIpAddresses.Any();

    public bool HasIpv6AddressesWhileIpv6Disabled
        => IsInverseSplitTunnelingEnabled
        && !IsIpv6Enabled
        && SelectedIpAddresses.Any(ip => ip.IsIpV6);

    public string IpAddressesHeader => Localizer.GetFormat(IsStandardSplitTunneling
        ? "Settings_Connection_SplitTunneling_IpAddresses_Excluded_FormattedHeader"
        : "Settings_Connection_SplitTunneling_IpAddresses_Included_FormattedHeader", SelectedIpAddresses.Count());

    public SmartObservableCollection<SelectableTunnelingApp> IncludedApps { get; } = [];
    public SmartObservableCollection<SelectableTunnelingApp> ExcludedApps { get; } = [];

    public SmartObservableCollection<SelectableTunnelingApp> Apps
        => IsStandardSplitTunneling ? ExcludedApps : IncludedApps;

    public IEnumerable<TunnelingApp> SelectedApps
        => Apps.Where(app => app.IsSelected && app.Value.IsValid).Select(app => app.Value);

    public bool HasSelectedApps => SelectedApps.Any();

    public string AppsHeader => Localizer.GetFormat(IsStandardSplitTunneling
        ? "Settings_Connection_SplitTunneling_Apps_Excluded_FormattedHeader"
        : "Settings_Connection_SplitTunneling_Apps_Included_FormattedHeader", SelectedApps.Count());

    protected override UpsellFeatureType? UpsellFeature { get; } = UpsellFeatureType.SplitTunneling;

    public SplitTunnelingWidgetViewModel(
        IViewModelHelper viewModelHelper,
        IApplicationThemeSelector applicationThemeSelector,
        ISettings settings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        ISettingsConflictResolver settingsConflictResolver,
        IProfileEditor profileEditor,
        IAppSelector appSelector,
        IIpSelector ipSelector)
        : base(viewModelHelper,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               connectionManager,
               upsellCarouselWindowActivator,
               requiredReconnectionSettings,
               settingsConflictResolver,
               profileEditor,
               ConnectionFeature.SplitTunneling)
    {
        _appSelector = appSelector;
        _ipSelector = ipSelector;

        ExcludedIpAddresses.CollectionChanged += OnIpAddressesCollectionChanged;
        IncludedIpAddresses.CollectionChanged += OnIpAddressesCollectionChanged;
        ExcludedApps.CollectionChanged += OnAppsCollectionChanged;
        IncludedApps.CollectionChanged += OnAppsCollectionChanged;

        _disableSplitTunnelingSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.IsSplitTunnelingEnabled, () => false)
        ]);

        _enableStandardSplitTunnelingSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingMode, () => SplitTunnelingMode.Standard),
            ChangedSettingArgs.Create(() => Settings.IsSplitTunnelingEnabled, () => true)
        ]);

        _enableInverseSplitTunnelingSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingMode, () => SplitTunnelingMode.Inverse),
            ChangedSettingArgs.Create(() => Settings.IsSplitTunnelingEnabled, () => true)
        ]);

        _modifySplitTunnelingStandardAppsList = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingStandardAppsList, () => GetSettingsApps(ExcludedApps)),
        ]);

        _modifySplitTunnelingStandardIpAddressesList = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingStandardIpAddressesList, () => GetSettingsIpAddresses(ExcludedIpAddresses)),
            ChangedSettingArgs.Create(() => Settings.IsIpv6Enabled, () => IsIpv6Enabled),
        ]);

        _modifySplitTunnelingInverseAppsList = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingInverseAppsList, () => GetSettingsApps(IncludedApps)),
        ]);

        _modifySplitTunnelingInverseIpAddressesList = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.SplitTunnelingInverseIpAddressesList, () => GetSettingsIpAddresses(IncludedIpAddresses)),
            ChangedSettingArgs.Create(() => Settings.IsIpv6Enabled, () => IsIpv6Enabled),
        ]);

        _enableIpv6Settings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.IsIpv6Enabled, () => IsIpv6Enabled),
        ]);
    }

    protected override IEnumerable<string> GetSettingsChangedForUpdate()
    {
        yield return nameof(ISettings.IsSplitTunnelingEnabled);
        yield return nameof(ISettings.SplitTunnelingMode);
        yield return nameof(ISettings.SplitTunnelingStandardAppsList);
        yield return nameof(ISettings.SplitTunnelingStandardIpAddressesList);
        yield return nameof(ISettings.SplitTunnelingInverseAppsList);
        yield return nameof(ISettings.SplitTunnelingInverseIpAddressesList);
    }

    protected override string GetFeatureStatus()
    {
        return Localizer.Get(
            IsSplitTunnelingEnabled
                ? SplitTunnelingMode switch
                {
                    SplitTunnelingMode.Standard => "Settings_Connection_SplitTunneling_Standard_Short",
                    SplitTunnelingMode.Inverse => "Settings_Connection_SplitTunneling_Inverse_Short",
                    _ => throw new ArgumentOutOfRangeException(nameof(ISettings.SplitTunnelingMode))
                }
                : "Common_States_Off");
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(AppsHeader));
        OnPropertyChanged(nameof(IpAddressesHeader));
    }

    protected override void OnSettingsChanged()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(IsSplitTunnelingComponentVisible));
        OnPropertyChanged(nameof(IsSplitTunnelingEnabled));
        OnPropertyChanged(nameof(SplitTunnelingMode));
        OnPropertyChanged(nameof(IsStandardSplitTunnelingEnabled));
        OnPropertyChanged(nameof(IsInverseSplitTunnelingEnabled));

        OnRetrieveSettingsAsync().FireAndForget();
    }

    protected override void OnConnectionStatusChanged()
    {
        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(IsSplitTunnelingComponentDimmed));
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        OnRetrieveSettingsAsync().FireAndForget();
    }

    protected override bool IsOnFeaturePage(PageViewModelBase? currentPageContext)
    {
        return currentPageContext is SplitTunnelingPageViewModel;
    }

    protected override void OnFeatureFlyoutOpened()
    {
        base.OnFeatureFlyoutOpened();

        OnRetrieveSettingsAsync().FireAndForget();
    }

    private async Task OnRetrieveSettingsAsync()
    {
        if (!IsSplitTunnelingComponentVisible)
        {
            return;
        }

        try
        {
            IsLoading = true;

            IsIpv6Enabled = Settings.IsIpv6Enabled;

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

    private void OnAppsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Apps));
        OnPropertyChanged(nameof(AppsHeader));
        OnPropertyChanged(nameof(SelectedApps));
        OnPropertyChanged(nameof(HasSelectedApps));
    }

    private void OnIpAddressesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IpAddresses));
        OnPropertyChanged(nameof(IpAddressesHeader));
        OnPropertyChanged(nameof(SelectedIpAddresses));
        OnPropertyChanged(nameof(HasSelectedIpAddresses));
        OnPropertyChanged(nameof(HasIpv6AddressesWhileIpv6Disabled));
    }

    [RelayCommand]
    private Task<bool> DisableSplitTunnelingAsync()
    {
        return TryChangeFeatureSettingsAsync(_disableSplitTunnelingSettings.Value);
    }

    [RelayCommand]
    private Task<bool> EnableStandardSplitTunnelingAsync()
    {
        return TryChangeFeatureSettingsAsync(_enableStandardSplitTunnelingSettings.Value);
    }

    [RelayCommand]
    private Task<bool> EnableInverseSplitTunnelingAsync()
    {
        return TryChangeFeatureSettingsAsync(_enableInverseSplitTunnelingSettings.Value);
    }

    [RelayCommand]
    private async Task SelectAppsAsync()
    {
        _appSelector.Title = Localizer.Get(IsStandardSplitTunneling
            ? "Settings_Connection_SplitTunneling_Apps_Excluded_Header"
            : "Settings_Connection_SplitTunneling_Apps_Included_Header");
        _appSelector.Description = Localizer.Get(IsStandardSplitTunneling
            ? "Settings_Connection_SplitTunneling_Apps_Excluded_Description"
            : "Settings_Connection_SplitTunneling_Apps_Included_Description");

        List<SelectableTunnelingApp>? result = await _appSelector.SelectAsync(Apps.Select(app => app.Clone()).ToList());
        if (result == null)
        {
            return;
        }

        Apps.Reset(result);

        bool haveFeatureSettingsChanged = await TryChangeFeatureSettingsAsync(IsStandardSplitTunneling
            ? _modifySplitTunnelingStandardAppsList.Value
            : _modifySplitTunnelingInverseAppsList.Value);
        if (!haveFeatureSettingsChanged)
        {
            await OnRetrieveSettingsAsync();
        }
    }

    [RelayCommand]
    private async Task SelectIpsAsync()
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
        if (result == null)
        {
            return;
        }

        IpAddresses.Reset(result);

        if (HasIpv6AddressesWhileIpv6Disabled && !_wasIpv6WarningDisplayed)
        {
            await ShowIpv6DisabledWarningAsync();
        }

        bool haveFeatureSettingsChanged = await TryChangeFeatureSettingsAsync(IsStandardSplitTunneling
            ? _modifySplitTunnelingStandardIpAddressesList.Value
            : _modifySplitTunnelingInverseIpAddressesList.Value);
        if (!haveFeatureSettingsChanged)
        {
            await OnRetrieveSettingsAsync();
        }
    }

    [RelayCommand]
    private async Task TriggerIpv6DisabledWarningAsync()
    {
        if (await ShowIpv6DisabledWarningAsync())
        {
            bool hasFeatureSettingsChanged = await TryChangeFeatureSettingsAsync(_enableIpv6Settings.Value);
            if (!hasFeatureSettingsChanged)
            {
                await OnRetrieveSettingsAsync();
            }
        }
    }

    private async Task<bool> ShowIpv6DisabledWarningAsync()
    {
        ContentDialogResult result = await MainWindowOverlayActivator.ShowMessageAsync(new()
        {
            Title = Localizer.Get("Overlay_Ipv6Disabled_Title"),
            Message = Localizer.Get("Overlay_Ipv6Disabled_Description"),
            PrimaryButtonText = Localizer.Get("Overlay_Ipv6Disabled_PrimaryButton"),
            SecondaryButtonText = Localizer.Get("Overlay_Ipv6Disabled_SecondaryButton"),
        });

        // Show this warning only once per app launch
        _wasIpv6WarningDisplayed = true;

        if (result == ContentDialogResult.Primary)
        {
            IsIpv6Enabled = true;
            return true;
        }

        return false;
    }
}