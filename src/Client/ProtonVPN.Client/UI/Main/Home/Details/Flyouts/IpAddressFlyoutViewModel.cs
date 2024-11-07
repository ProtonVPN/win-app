﻿/*
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
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Home.Details.Flyouts;

public partial class IpAddressFlyoutViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<DeviceLocationChangedMessage>,
    IEventMessageReceiver<ConnectionDetailsChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private const string HIDDEN_IP_ADDRESS = "***.***.***.***";

    private readonly IUrls _urls;
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DeviceIpAddressOrHidden))]
    private string _deviceIpAddress = string.Empty;

    [ObservableProperty]
    private string _serverIpAddress = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DeviceIpAddressOrHidden))]
    private bool _isIpAddressVisible;

    public string IpAddressLearnMoreUri => _urls.IpAddressLearnMore;

    public string Header => Localizer.Get(IsConnected
        ? "Flyouts_IpAddress_VpnIp"
        : "Flyouts_IpAddress_Title");

    public string Description => Localizer.Get(IsConnected
        ? "Flyouts_IpAddress_Description_Connected"
        : "Flyouts_IpAddress_Description_Disconnected");

    public bool IsConnected => _connectionManager.IsConnected;

    public bool IsDisconnected => _connectionManager.IsDisconnected;

    public string DeviceIpAddressOrHidden
        => IsIpAddressVisible
            ? DeviceIpAddress
            : HIDDEN_IP_ADDRESS;

    public IpAddressFlyoutViewModel(
        IUrls urls,
        ISettings settings,
        IConnectionManager connectionManager,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter) :
        base(localizer, logger, issueReporter)
    {
        _urls = urls;
        _settings = settings;
        _connectionManager = connectionManager;
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
        ExecuteOnUIThread(() =>
        {
            ServerIpAddress = message.ServerIpAddress;
        });
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
        DeviceIpAddress = _settings.DeviceLocation?.IpAddress ?? string.Empty;
    }

    private void InvalidateConnectionStatus()
    {
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(IsDisconnected));
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(Description));
    }

    [RelayCommand]
    private void ToggleIpAddress()
    {
        IsIpAddressVisible = !IsIpAddressVisible;
    }
}