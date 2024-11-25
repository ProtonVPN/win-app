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
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Models.Clipboards;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Components;

public partial class ActivePortComponentViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<PortForwardingStatusChangedMessage>,
    IEventMessageReceiver<PortForwardingPortChangedMessage>
{
    private const string PORT_NUMBER_PLACEHOLDER = "-";

    private readonly IPortForwardingManager _portForwardingManager;
    private readonly IClipboardEditor _clipboardEditor;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CopyPortNumberCommand))]
    [NotifyPropertyChangedFor(nameof(HasActivePortNumber))]
    [NotifyPropertyChangedFor(nameof(Header))]
    private int? _activePortNumber;

    public string Header => IsFetchingPort
        ? Localizer.Get("Settings_Connection_PortForwarding_Loading") 
        : ActivePortNumber?.ToString() ?? PORT_NUMBER_PLACEHOLDER;

    public bool HasActivePortNumber => ActivePortNumber.HasValue;

    public bool IsFetchingPort => !HasActivePortNumber && _portForwardingManager.IsFetchingPort;

    public ActivePortComponentViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        IPortForwardingManager portForwardingManager,
        IClipboardEditor clipboardEditor)
        : base(localizationProvider, logger, issueReporter)
    {
        _portForwardingManager = portForwardingManager;
        _clipboardEditor = clipboardEditor;
    }

    [RelayCommand(CanExecute = nameof(CanCopyPortNumber))]
    private async Task CopyPortNumberAsync()
    {
        int? activePortNumber = ActivePortNumber;
        if (activePortNumber is not null)
        {
            await _clipboardEditor.SetTextAsync($"{activePortNumber}");
        }
    }

    private bool CanCopyPortNumber()
    {
        return HasActivePortNumber;
    }

    public void Receive(PortForwardingStatusChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateStatusAndActivePortNumber);
    }

    public void Receive(PortForwardingPortChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateStatusAndActivePortNumber);
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateStatusAndActivePortNumber();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Header));
    }

    private void InvalidateStatusAndActivePortNumber()
    {
        ActivePortNumber = _portForwardingManager.ActivePort;

        OnPropertyChanged(nameof(IsFetchingPort));
        OnPropertyChanged(nameof(Header));
    }
}