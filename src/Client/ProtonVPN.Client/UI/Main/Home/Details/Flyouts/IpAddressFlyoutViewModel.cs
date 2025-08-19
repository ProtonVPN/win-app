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
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Observers;

namespace ProtonVPN.Client.UI.Main.Home.Details.Flyouts;

public partial class IpAddressFlyoutViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<DeviceLocationChangedMessage>,
    IEventMessageReceiver<ConnectionDetailsChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private const string HIDDEN_IP_ADDRESS = "***.***.***.***";

    private readonly IUrlsBrowser _urlsBrowser;
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;
    private readonly IFeatureFlagsObserver _featureFlagsObserver;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DeviceIpAddressOrHidden))]
    [NotifyPropertyChangedFor(nameof(IsIpAddressExposed))]
    private string _deviceIpAddress = EmptyValueExtensions.DEFAULT;

    [ObservableProperty]
    private string _serverIpv4Address = EmptyValueExtensions.DEFAULT;

    [ObservableProperty]
    private string _serverIpv6Address = EmptyValueExtensions.DEFAULT;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DeviceIpAddressOrHidden))]
    private bool _isIpAddressVisible;

    public string IpAddressLearnMoreUri => _urlsBrowser.IpAddressLearnMore;

    public string Header => Localizer.Get(IsConnected
        ? "Flyouts_IpAddress_VpnIp"
        : "Flyouts_IpAddress_Title");

    public string Description => IsConnected
        ? Localizer.Get("Flyouts_IpAddress_Description_Connected")
        : Localizer.Get("Flyouts_IpAddress_Description_Disconnected1") +
          Environment.NewLine +
          Environment.NewLine +
          Localizer.Get("Flyouts_IpAddress_Description_Disconnected2");

    public bool IsConnected => _connectionManager.IsConnected;
    public bool IsDisconnected => _connectionManager.IsDisconnected;
    public bool IsIpAddressExposed => IsDisconnected && !string.IsNullOrWhiteSpace(_settings.DeviceLocation?.IpAddress);

    public bool IsConnectedWithIpv4 => IsConnected &&
        (!_featureFlagsObserver.IsIpv6SupportEnabled
            || !_settings.IsIpv6Enabled
            || _connectionManager.CurrentConnectionDetails?.IsIpv6Supported == false
            || string.IsNullOrWhiteSpace(_connectionManager.CurrentConnectionDetails?.ServerIpAddress?.Ipv6Address));

    public bool IsConnectedWithIpv6 => IsConnected && !IsConnectedWithIpv4;

    public string DeviceIpAddressOrHidden
        => IsIpAddressVisible
            ? DeviceIpAddress
            : HIDDEN_IP_ADDRESS;

    public IpAddressFlyoutViewModel(
        IUrlsBrowser urlsBrowser,
        ISettings settings,
        IConnectionManager connectionManager,
        IFeatureFlagsObserver featureFlagsObserver,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
        _settings = settings;
        _connectionManager = connectionManager;
        _featureFlagsObserver = featureFlagsObserver;
    }

    public void Receive(DeviceLocationChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateDeviceIpAddress);
        }
    }

    public void Receive(ConnectionDetailsChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateServerIpAddress);
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateConnectionStatus);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateDeviceIpAddress();
        InvalidateServerIpAddress();
        InvalidateConnectionStatus();
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        IsIpAddressVisible = false;
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(Description));
    }

    private void InvalidateDeviceIpAddress()
    {
        DeviceIpAddress = EmptyValueExtensions.GetValueOrDefault(_settings.DeviceLocation?.IpAddress);
    }

    private void InvalidateServerIpAddress()
    {
        ServerIpv4Address = EmptyValueExtensions.GetValueOrDefault(_connectionManager.CurrentConnectionDetails?.ServerIpAddress?.Ipv4Address);
        ServerIpv6Address = EmptyValueExtensions.GetValueOrDefault(_connectionManager.CurrentConnectionDetails?.ServerIpAddress?.Ipv6Address);
    }

    private void InvalidateConnectionStatus()
    {
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(IsDisconnected));
        OnPropertyChanged(nameof(IsIpAddressExposed));
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(Description));
    }

    [RelayCommand]
    private void ToggleIpAddress()
    {
        IsIpAddressVisible = !IsIpAddressVisible;
    }
}