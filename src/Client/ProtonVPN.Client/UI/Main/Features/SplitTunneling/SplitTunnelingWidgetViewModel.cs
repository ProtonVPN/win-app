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

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Contracts.Profiles;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Models.Features.SplitTunneling;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Features.Bases;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Client.UI.Main.Settings.Connection;

namespace ProtonVPN.Client.UI.Main.Features.SplitTunneling;

public partial class SplitTunnelingWidgetViewModel : FeatureWidgetViewModelBase
{
    private readonly Lazy<List<ChangedSettingArgs>> _disableSplitTunnelingSettings;
    private readonly Lazy<List<ChangedSettingArgs>> _enableStandardSplitTunnelingSettings;
    private readonly Lazy<List<ChangedSettingArgs>> _enableInverseSplitTunnelingSettings;

    private readonly ISplitTunnelingItemFactory _splitTunnelingItemFactory;

    public override string Header => Localizer.Get("Settings_Connection_SplitTunneling");

    public string InfoMessage => !ConnectionManager.IsConnected || !Settings.IsSplitTunnelingEnabled
        ? Localizer.Get("Flyouts_SplitTunneling_Info")
        : Settings.SplitTunnelingMode switch
        {
            SplitTunnelingMode.Standard => Localizer.Get("Flyouts_SplitTunneling_Standard_Info"),
            SplitTunnelingMode.Inverse => Localizer.Get("Flyouts_SplitTunneling_Inverse_Info"),
            _ => Localizer.Get("Flyouts_SplitTunneling_Info")
        };

    public bool IsInfoMessageVisible => true;

    public bool IsSplitTunnelingComponentVisible => ConnectionManager.IsConnected
                                                 && IsSplitTunnelingEnabled
                                                 && HasItems;

    public SmartObservableCollection<SplitTunnelingGroup> Groups { get; } = [];
    public SmartObservableCollection<SplitTunnelingItemBase> Items { get; } = [];

    public CollectionViewSource GroupsCvs { get; }

    public bool HasItems => GroupsCvs.View.Any();

    public bool IsSplitTunnelingEnabled => Settings.IsSplitTunnelingEnabled;

    public SplitTunnelingMode SplitTunnelingMode => Settings.SplitTunnelingMode;

    public bool IsStandardSplitTunnelingEnabled => IsSplitTunnelingEnabled && SplitTunnelingMode == SplitTunnelingMode.Standard;

    public bool IsInverseSplitTunnelingEnabled => IsSplitTunnelingEnabled && SplitTunnelingMode == SplitTunnelingMode.Inverse;

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
        ISplitTunnelingItemFactory splitTunnelingItemFactory,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        ISettingsConflictResolver settingsConflictResolver,
        IProfileEditor profileEditor)
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
        _splitTunnelingItemFactory = splitTunnelingItemFactory;

        GroupsCvs = new()
        {
            Source = Groups,
            IsSourceGrouped = true
        };

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

    protected override async void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(InfoMessage));

        await InvalidateAppsAndIpsAsync();
    }

    protected override async void OnSettingsChanged()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(IsSplitTunnelingComponentVisible));
        OnPropertyChanged(nameof(IsSplitTunnelingEnabled));
        OnPropertyChanged(nameof(SplitTunnelingMode));
        OnPropertyChanged(nameof(IsStandardSplitTunnelingEnabled));
        OnPropertyChanged(nameof(IsInverseSplitTunnelingEnabled));

        await InvalidateAppsAndIpsAsync();
    }

    protected override async void OnConnectionStatusChanged()
    {
        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(IsSplitTunnelingComponentVisible));

        await InvalidateAppsAndIpsAsync();
    }

    protected override bool IsOnFeaturePage(PageViewModelBase? currentPageContext)
    {
        return currentPageContext is SplitTunnelingPageViewModel;
    }

    private async Task InvalidateAppsAndIpsAsync()
    {
        if (!IsSplitTunnelingEnabled || !ConnectionManager.IsConnected)
        {
            Items.Clear();
            Groups.Clear();
            return;
        }

        SplitTunnelingMode splitTunnelingMode = SplitTunnelingMode;

        List<SplitTunnelingItemBase> items = [];

        List<SplitTunnelingApp> apps = splitTunnelingMode switch
        {
            SplitTunnelingMode.Standard => Settings.SplitTunnelingStandardAppsList,
            SplitTunnelingMode.Inverse => Settings.SplitTunnelingInverseAppsList,
            _ => []
        };

        List<SplitTunnelingIpAddress> ips = splitTunnelingMode switch
        {
            SplitTunnelingMode.Standard => Settings.SplitTunnelingStandardIpAddressesList,
            SplitTunnelingMode.Inverse => Settings.SplitTunnelingInverseIpAddressesList,
            _ => []
        };

        foreach (SplitTunnelingApp app in apps.Where(a => a.IsActive))
        {
            items.Add(await _splitTunnelingItemFactory.GetAppAsync(app, splitTunnelingMode));
        }

        items.AddRange(
            ips.Where(i => i.IsActive)
               .Select(i => _splitTunnelingItemFactory.GetIpAddress(i, splitTunnelingMode))
               .ToList());

        Items.Reset(items.OrderBy(i => i.GroupType));

        Groups.Reset(Items
              .GroupBy(item => item.GroupType)
              .OrderBy(group => group.Key)
              .Select(group => _splitTunnelingItemFactory.GetGroup(group.Key, group)));

        OnPropertyChanged(nameof(IsSplitTunnelingComponentVisible));
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
}