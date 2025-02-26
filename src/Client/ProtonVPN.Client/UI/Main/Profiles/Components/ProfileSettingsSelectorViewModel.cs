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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Models.Settings;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Profiles.Contracts;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.UI.Main.Profiles.Components;

public partial class ProfileSettingsSelectorViewModel : ViewModelBase, IProfileSettingsSelector
{
    private readonly ISettings _settings;
    private readonly ICommonItemFactory _commonItemFactory;
    private readonly IRequiredReconnectionSettings _requiredReconnectionSettings;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IUrlsBrowser _urlsBrowser;

    private IProfileSettings _originalProfileSettings = ProfileSettings.Default;

    [ObservableProperty]
    private ProtocolItem? _selectedProtocol;

    [ObservableProperty]
    private NetShieldModeItem? _selectedNetShieldMode;

    [ObservableProperty]
    private NatTypeItem? _selectedNatType;

    [ObservableProperty]
    private PortForwardingItem? _selectedPortForwardingState;

    [ObservableProperty]
    private bool _isCustomSettingsSectionExpanded;

    public string NetShieldLearnMoreUrl => _urlsBrowser.NetShieldLearnMore;

    public SmartObservableCollection<ProtocolItem> Protocols { get; } = [];

    public SmartObservableCollection<NetShieldModeItem> NetShieldModes { get; } = [];

    public SmartObservableCollection<NatTypeItem> NatTypes { get; } = [];

    public SmartObservableCollection<PortForwardingItem> PortForwardingStates { get; } = [];

    protected bool ProtocolHasChanged => _originalProfileSettings.VpnProtocol != SelectedProtocol?.Protocol;

    protected bool NetShieldStateHasChanged => _originalProfileSettings.IsNetShieldEnabled != SelectedNetShieldMode?.IsEnabled;

    protected bool NetShieldModeHasChanged => _originalProfileSettings.NetShieldMode != SelectedNetShieldMode?.Mode;

    protected bool NetShieldHasChanged => NetShieldStateHasChanged || NetShieldModeHasChanged;

    protected bool NatTypeHasChanged => _originalProfileSettings.NatType != SelectedNatType?.NatType;

    protected bool PortForwardingHasChanged => _originalProfileSettings.IsPortForwardingEnabled != SelectedPortForwardingState?.IsEnabled;

    public ProfileSettingsSelectorViewModel(
        IViewModelHelper viewModelHelper,
        ISettings settings,
        ICommonItemFactory commonItemFactory,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IUrlsBrowser urlsBrowser)
        : base(viewModelHelper)
    {
        _settings = settings;
        _commonItemFactory = commonItemFactory;
        _requiredReconnectionSettings = requiredReconnectionSettings;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _urlsBrowser = urlsBrowser;
    }

    public IProfileSettings GetProfileSettings()
    {
        return new ProfileSettings()
        {
            VpnProtocol = SelectedProtocol?.Protocol ?? DefaultSettings.VpnProtocol,
            IsNetShieldEnabled = SelectedNetShieldMode?.IsEnabled ?? DefaultSettings.IsNetShieldEnabled(true),
            NetShieldMode = SelectedNetShieldMode?.Mode ?? DefaultSettings.NetShieldMode,
            NatType = SelectedNatType?.NatType ?? DefaultSettings.NatType,
            IsPortForwardingEnabled = SelectedPortForwardingState?.IsEnabled ?? DefaultSettings.IsPortForwardingEnabled
        };
    }

    public void SetProfileSettings(IProfileSettings settings)
    {
        IsCustomSettingsSectionExpanded = false;

        _originalProfileSettings = settings ?? ProfileSettings.Default;

        InvalidateCollections();

        SelectedProtocol = Protocols.FirstOrDefault(p => p.Protocol == _originalProfileSettings.VpnProtocol);
        SelectedNetShieldMode = _originalProfileSettings.IsNetShieldEnabled
            ? NetShieldModes.FirstOrDefault(p => p.IsEnabled && _originalProfileSettings.NetShieldMode == p.Mode)
            : NetShieldModes.FirstOrDefault(p => !p.IsEnabled);
        SelectedNatType = NatTypes.FirstOrDefault(p => p.NatType == _originalProfileSettings.NatType);
        SelectedPortForwardingState = PortForwardingStates.First(m => m.IsEnabled == _originalProfileSettings.IsPortForwardingEnabled);
    }

    public bool HasChanged()
    {
        return ProtocolHasChanged
            || NetShieldHasChanged
            || NatTypeHasChanged
            || PortForwardingHasChanged;
    }

    public bool IsReconnectionRequired()
    {
        return ProtocolHasChanged
            || (NetShieldStateHasChanged && _settings.IsCustomDnsServersEnabled);
    }

    private static IEnumerable<VpnProtocol> GetProtocolsByOrder()
    {
        yield return VpnProtocol.Smart;
        yield return VpnProtocol.WireGuardUdp;
        yield return VpnProtocol.WireGuardTcp;
        yield return VpnProtocol.WireGuardTls;
        yield return VpnProtocol.OpenVpnUdp;
        yield return VpnProtocol.OpenVpnTcp;
    }

    private static IEnumerable<NetShieldMode?> GetNetShieldModesByOrder()
    {
        yield return null;
        yield return NetShieldMode.BlockMalwareOnly;
        yield return NetShieldMode.BlockAdsMalwareTrackers;
    }

    private static IEnumerable<NatType> GetNatTypesByOrder()
    {
        yield return NatType.Strict;
        yield return NatType.Moderate;
    }

    private static IEnumerable<bool> GetPortForwardingModesByOrder()
    {
        yield return false;
        yield return true;
    }

    private void InvalidateCollections()
    {
        Protocols.Reset(GetProtocolsByOrder().Select(_commonItemFactory.GetProtocol));
        NetShieldModes.Reset(GetNetShieldModesByOrder().Select(_commonItemFactory.GetNetShieldMode));
        NatTypes.Reset(GetNatTypesByOrder().Select(_commonItemFactory.GetNatType));
        PortForwardingStates.Reset(GetPortForwardingModesByOrder().Select(_commonItemFactory.GetPortForwardingMode));
    }

    [RelayCommand]
    private void ToggleExpander()
    {
        IsCustomSettingsSectionExpanded = !IsCustomSettingsSectionExpanded;
    }

    [RelayCommand]
    private Task<bool> DisableNetShieldAsync()
    {
        return TryChangeNetShieldModeAsync(false);
    }

    [RelayCommand]
    private Task<bool> EnableStandardNetShieldAsync()
    {
        return TryChangeNetShieldModeAsync(true, NetShieldMode.BlockMalwareOnly);
    }

    [RelayCommand]
    private Task<bool> EnableAdvancedNetShieldAsync()
    {
        return TryChangeNetShieldModeAsync(true, NetShieldMode.BlockAdsMalwareTrackers);
    }

    private async Task<bool> TryChangeNetShieldModeAsync(bool isEnabled, NetShieldMode? netShieldMode = null)
    {
        if (isEnabled && SelectedNetShieldMode?.IsEnabled != isEnabled && _settings.IsCustomDnsServersEnabled)
        {
            ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(new()
            {
                Title = Localizer.Get("Settings_Connection_NetShield_Conflict_Title"),
                Message = Localizer.Get("Profile_NetShield_Conflict_Description"),
                PrimaryButtonText = Localizer.Get("Common_Actions_Enable"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
                MessageType = DialogMessageType.RichText,
                TrailingInlineButton = new()
                {
                    Text = Localizer.Get("Common_Links_LearnMore"),
                    Url = _urlsBrowser.NetShieldLearnMore,
                },
            });
            if (result != ContentDialogResult.Primary)
            {
                return false;
            }
        }

        SelectedNetShieldMode = isEnabled
            ? NetShieldModes.FirstOrDefault(nsm => nsm.IsEnabled && nsm.Mode == netShieldMode)
            : NetShieldModes.FirstOrDefault(nsm => !nsm.IsEnabled);

        return true;
    }

    [RelayCommand]
    private Task<bool> DisablePortForwardingAsync()
    {
        return TryChangePortForwardingAsync(false);
    }

    [RelayCommand]
    private Task<bool> EnablePortForwardingAsync()
    {
        return TryChangePortForwardingAsync(true);
    }

    private async Task<bool> TryChangePortForwardingAsync(bool isEnabled)
    {
        if (isEnabled && SelectedPortForwardingState?.IsEnabled != isEnabled && SelectedNatType?.NatType == NatType.Moderate)
        {
            ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(new()
            {
                Title = Localizer.Get("Settings_Connection_PortForwarding_Conflict_Title"),
                Message = Localizer.Get("Profile_PortForwarding_Conflict_Description"),
                PrimaryButtonText = Localizer.Get("Common_Actions_Enable"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
                MessageType = DialogMessageType.RichText,
                TrailingInlineButton = new()
                {
                    Text = Localizer.Get("Common_Links_LearnMore"),
                    Url = _urlsBrowser.PortForwardingLearnMore,
                },
            });
            if (result != ContentDialogResult.Primary)
            {
                return false;
            }

            SelectedNatType = NatTypes.FirstOrDefault(nt => nt.NatType == NatType.Strict);
        }

        SelectedPortForwardingState = PortForwardingStates.First(pfs => pfs.IsEnabled == isEnabled);

        return true;
    }

    [RelayCommand]
    private Task<bool> EnableStrictNatAsync()
    {
        return TryChangeNatTypeAsync(NatType.Strict);
    }

    [RelayCommand]
    private Task<bool> EnableModerateNatAsync()
    {
        return TryChangeNatTypeAsync(NatType.Moderate);
    }

    private async Task<bool> TryChangeNatTypeAsync(NatType natType)
    {
        if (natType == NatType.Moderate && SelectedNatType?.NatType != natType && SelectedPortForwardingState?.IsEnabled == true)
        {
            ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(new()
            {
                Title = Localizer.Get("Settings_Connection_Advanced_NatType_Conflict_Title"),
                Message = Localizer.Get("Profile_NatType_Conflict_Description"),
                PrimaryButtonText = Localizer.Get("Common_Actions_Enable"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
                MessageType = DialogMessageType.RichText,
                TrailingInlineButton = new()
                {
                    Text = Localizer.Get("Common_Links_LearnMore"),
                    Url = _urlsBrowser.NatTypeLearnMore,
                },
            });
            if (result != ContentDialogResult.Primary)
            {
                return false;
            }

            SelectedPortForwardingState = PortForwardingStates.First(s => !s.IsEnabled);
        }

        SelectedNatType = NatTypes.FirstOrDefault(nt => nt.NatType == natType);

        return true;
    }

    [RelayCommand]
    private void SelectSmartProtocol()
    {
        SelectProtocol(VpnProtocol.Smart);
    }

    [RelayCommand]
    private void SelectWireGuardUdpProtocol()
    {
        SelectProtocol(VpnProtocol.WireGuardUdp);
    }

    [RelayCommand]
    private void SelectWireGuardTcpProtocol()
    {
        SelectProtocol(VpnProtocol.WireGuardTcp);
    }

    [RelayCommand]
    private void SelectWireGuardTlsProtocol()
    {
        SelectProtocol(VpnProtocol.WireGuardTls);
    }

    [RelayCommand]
    private void SelectOpenVpnUdpProtocol()
    {
        SelectProtocol(VpnProtocol.OpenVpnUdp);
    }

    [RelayCommand]
    private void SelectOpenVpnTcpProtocol()
    {
        SelectProtocol(VpnProtocol.OpenVpnTcp);
    }

    private void SelectProtocol(VpnProtocol protocol)
    {
        SelectedProtocol = Protocols.FirstOrDefault(p => p.Protocol == protocol);
    }
}