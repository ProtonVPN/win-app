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
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Services.PortForwarding;

namespace ProtonVPN.Client.UI.Main.Components;

public partial class ActivePortComponentViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<PortForwardingStatusChangedMessage>,
    IEventMessageReceiver<PortForwardingPortChangedMessage>
{
    private const string PORT_NUMBER_PLACEHOLDER = "-";

    private readonly IPortForwardingManager _portForwardingManager;
    private readonly IPortForwardingClipboardService _portForwardingClipboardService;

    private readonly IDispatcherTimer _timer;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CopyPortNumberCommand))]
    [NotifyPropertyChangedFor(nameof(HasActivePortNumber))]
    [NotifyPropertyChangedFor(nameof(Header))]
    private int? _activePortNumber;

    [ObservableProperty]
    private string _lastPortChangeTagline = string.Empty;

    public string Header => IsFetchingPort
        ? Localizer.Get("Settings_Connection_PortForwarding_Loading") 
        : ActivePortNumber?.ToString() ?? PORT_NUMBER_PLACEHOLDER;

    public bool HasActivePortNumber => ActivePortNumber.HasValue;

    public bool IsFetchingPort => !HasActivePortNumber && _portForwardingManager.IsFetchingPort;

    public ActivePortComponentViewModel(
        IPortForwardingManager portForwardingManager,
        IPortForwardingClipboardService portForwardingClipboardService,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _portForwardingManager = portForwardingManager;
        _portForwardingClipboardService = portForwardingClipboardService;

        _timer = UIThreadDispatcher.GetTimer(TimeSpan.FromSeconds(1));
        _timer.Tick += OnTimerTick;
    }

    private void OnTimerTick(object? sender, object e)
    {
        InvalidateLastPortChangeTagline();
        InvalidateTimerInterval();
    }

    [RelayCommand(CanExecute = nameof(CanCopyPortNumber))]
    private async Task CopyPortNumberAsync()
    {
        int? activePortNumber = ActivePortNumber;
        if (activePortNumber is not null)
        {
            await _portForwardingClipboardService.CopyActivePortToClipboardAsync();
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

        InvalidateTimer();
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        InvalidateTimer();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Header));
    }

    private void InvalidateStatusAndActivePortNumber()
    {
        ActivePortNumber = _portForwardingManager.ActivePort;

        InvalidateTimer();
        InvalidateLastPortChangeTagline();

        OnPropertyChanged(nameof(IsFetchingPort));
        OnPropertyChanged(nameof(Header));
    }

    private void InvalidateTimer()
    {
        if (IsActive && HasActivePortNumber)
        {
            if (!_timer.IsEnabled)
            {
                InvalidateTimerInterval();
                _timer.Start();
            }
        }
        else
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }
        }
    }

    private void InvalidateTimerInterval()
    {
        DateTime? lastPortChangeTimeUtc = _portForwardingManager.LastPortChangeTimeUtc;
        if (lastPortChangeTimeUtc.HasValue)
        {
            TimeSpan elapsedTime = DateTime.UtcNow - lastPortChangeTimeUtc.Value;
            _timer.Interval = elapsedTime switch
            {
                TimeSpan when elapsedTime < TimeSpan.FromMinutes(1) => TimeSpan.FromSeconds(1),
                TimeSpan when elapsedTime < TimeSpan.FromHours(1) => TimeSpan.FromSeconds(20),
                TimeSpan when elapsedTime < TimeSpan.FromDays(1) => TimeSpan.FromMinutes(20),
                _ => TimeSpan.FromHours(1)
            };
        }
        else
        {
             _timer.Interval = TimeSpan.FromSeconds(1);
        }
    }

    private void InvalidateLastPortChangeTagline()
    {
        DateTime? lastPortChangeTimeUtc = _portForwardingManager.LastPortChangeTimeUtc;

        if (lastPortChangeTimeUtc.HasValue)
        {
            LastPortChangeTagline = Localizer.GetFormat("Settings_Connection_PortForwarding_LastChanged", Localizer.GetFormattedElapsedTime(lastPortChangeTimeUtc.Value));
        }
        else
        {
            LastPortChangeTagline = string.Empty;
        }
    }
}