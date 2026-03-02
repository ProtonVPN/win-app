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
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Models;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Services.Bootstrapping.Helpers;
using ProtonVPN.Client.UI.Overlays.Selection.Contracts;
using ProtonVPN.Common.Core.Networking;
using Windows.System;

namespace ProtonVPN.Client.UI.Overlays.Selection;

public partial class IpSelectorOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>, IIpSelector
{
    private bool _isRunningAsAdmin;

    private List<SelectableNetworkAddress> _originalAddresses = [];

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _caption = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDragDropEnabled))]
    private bool _canReorder;

    [ObservableProperty]
    private bool _isAddressRangeAuthorized;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddAddressCommand))]
    private string _currentAddress = string.Empty;

    [ObservableProperty]
    private string? _currentAddressError;

    public SmartNotifyObservableCollection<SelectableNetworkAddress> Addresses { get; } = [];

    public bool HasAddresses => Addresses.Count > 0;

    public string MoveUpTooltip => Localizer.Get("Common_Actions_MoveUp");

    public string MoveDownTooltip => Localizer.Get("Common_Actions_MoveDown");

    public string RemoveTooltip => Localizer.Get("Common_Actions_Remove");

    public bool IsDragDropEnabled => CanReorder && !_isRunningAsAdmin;

    public bool HasChanges => !AreAddressesEqual(_originalAddresses, Addresses);

    public IpSelectorOverlayViewModel(
        IMainWindowOverlayActivator overlayActivator,
        IViewModelHelper viewModelHelper)
        : base(overlayActivator, viewModelHelper)
    {
        _isRunningAsAdmin = AppInstanceHelper.IsRunningAsAdmin();

        Addresses.CollectionChanged += OnAddressesCollectionChanged;
        Addresses.ItemPropertyChanged += OnAddressesItemPropertyChanged;
    }

    public async Task<List<SelectableNetworkAddress>?> SelectAsync(List<SelectableNetworkAddress> addresses)
    {
        ResetCurrentAddress();
        ResetCurrentAddressError();

        _originalAddresses = addresses.Select(a => a.Clone()).ToList();

        Addresses.Reset(addresses);

        ContentDialogResult result = await InvokeAsync();
        return result switch
        {
            ContentDialogResult.Primary => Addresses.ToList(),
            _ => null
        };
    }

    public void OnCurrentAddressKeyDownHandler(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            AddAddress();
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(MoveUpTooltip));
        OnPropertyChanged(nameof(MoveDownTooltip));
        OnPropertyChanged(nameof(RemoveTooltip));
    }

    private void OnAddressesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasAddresses));
        OnPropertyChanged(nameof(HasChanges));

        MoveAddressUpCommand.NotifyCanExecuteChanged();
        MoveAddressDownCommand.NotifyCanExecuteChanged();
    }

    private void OnAddressesItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasChanges));
    }

    [RelayCommand(CanExecute = nameof(CanAddAddress))]
    private void AddAddress()
    {
        NetworkAddress? address = GetValidatedCurrentIpAddress();
        string? error = GetIpAddressError(address);

        if (error != null || address == null)
        {
            CurrentAddressError = error ?? Localizer.Get("Settings_Common_IpAddresses_Invalid");
            return;
        }

        Addresses.Add(new SelectableNetworkAddress(address.Value));

        ResetCurrentAddress();
    }

    private bool CanAddAddress()
    {
        return !string.IsNullOrWhiteSpace(CurrentAddress);
    }

    [RelayCommand]
    private void RemoveAddress(SelectableNetworkAddress address)
    {
        Addresses.Remove(address);
    }

    [RelayCommand(CanExecute = nameof(CanMoveAddressUp))]
    private void MoveAddressUp(SelectableNetworkAddress address)
    {
        int currentIndex = Addresses.IndexOf(address);
        if (currentIndex > 0)
        {
            Addresses.Move(currentIndex, currentIndex - 1);
        }
    }

    private bool CanMoveAddressUp(SelectableNetworkAddress address)
    {
        int currentIndex = Addresses.IndexOf(address);
        return CanReorder
            && currentIndex > 0;
    }

    [RelayCommand(CanExecute = nameof(CanMoveAddressDown))]
    private void MoveAddressDown(SelectableNetworkAddress address)
    {
        int currentIndex = Addresses.IndexOf(address);
        if (currentIndex >= 0 && currentIndex < Addresses.Count - 1)
        {
            Addresses.Move(currentIndex, currentIndex + 1);
        }
    }

    private bool CanMoveAddressDown(SelectableNetworkAddress address)
    {
        int currentIndex = Addresses.IndexOf(address);
        return CanReorder
            && currentIndex >= 0
            && currentIndex < Addresses.Count - 1;
    }

    private NetworkAddress? GetValidatedCurrentIpAddress()
    {
        return NetworkAddress.TryParse(CurrentAddress, out NetworkAddress address) ? address : null;
    }

    private string? GetIpAddressError(NetworkAddress? address)
    {
        if (address == null)
        {
            return Localizer.Get("Settings_Common_IpAddresses_Invalid");
        }

        if (!IsAddressRangeAuthorized && !address.Value.IsSingleIp)
        {
            return Localizer.Get("Settings_Common_IpAddresses_Invalid");
        }

        if (Addresses.FirstOrDefault(ip => ip.Value.FormattedAddress == address.Value.FormattedAddress) != null)
        {
            return Localizer.Get("Settings_Common_IpAddresses_AlreadyExists");
        }

        return null;
    }

    private void ResetCurrentAddress()
    {
        CurrentAddress = string.Empty;
    }

    private void ResetCurrentAddressError()
    {
        CurrentAddressError = null;
    }

    partial void OnCurrentAddressChanged(string value)
    {
        ResetCurrentAddressError();
    }

    private bool AreAddressesEqual(List<SelectableNetworkAddress> original, IList<SelectableNetworkAddress> current)
    {
        if (original.Count != current.Count)
        {
            return false;
        }

        for (int i = 0; i < original.Count; i++)
        {
            if (original[i].Value.FormattedAddress != current[i].Value.FormattedAddress ||
                original[i].IsSelected != current[i].IsSelected)
            {
                return false;
            }
        }

        return true;
    }
}