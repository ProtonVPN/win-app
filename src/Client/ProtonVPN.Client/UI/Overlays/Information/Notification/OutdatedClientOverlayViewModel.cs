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
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.UI.Update;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Update.Contracts;

namespace ProtonVPN.Client.UI.Overlays.Information.Notification;

public partial class OutdatedClientOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly UpdateViewModel _updateViewModel;
    private readonly IUpdatesManager _updatesManager;
    private readonly IMainWindowActivator _mainWindowActivator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCheckingForUpdate))]
    [NotifyPropertyChangedFor(nameof(CanUpdate))]
    private AppUpdateStateContract? _lastUpdateState;

    public bool IsCheckingForUpdate => LastUpdateState != null &&
                                       !LastUpdateState.IsReady &&
                                       LastUpdateState?.Status is AppUpdateStatus.Checking or AppUpdateStatus.Downloading;

    public bool CanUpdate => !IsCheckingForUpdate;

    public OutdatedClientOverlayViewModel(
        IUrlsBrowser urlsBrowser,
        UpdateViewModel updateViewModel,
        IUpdatesManager updatesManager,
        IMainWindowActivator mainWindowActivator,
        IMainWindowOverlayActivator overlayActivator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter) : base(overlayActivator, localizer, logger, issueReporter)
    {
        _urlsBrowser = urlsBrowser;
        _updateViewModel = updateViewModel;
        _updatesManager = updatesManager;
        _mainWindowActivator = mainWindowActivator;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        _updatesManager.CheckForUpdate(isManualCheck: true);
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task UpdateAsync()
    {
        if (LastUpdateState != null && LastUpdateState.IsReady)
        {
            await _updateViewModel.UpdateCommand.ExecuteAsync(null);
        }
        else
        {
            _urlsBrowser.BrowseTo(_urlsBrowser.DownloadsPage);
        }

        Exit();
    }

    [RelayCommand]
    private void Exit()
    {
        _mainWindowActivator.Exit();
    }

    public void Receive(ClientUpdateStateChangedMessage message)
    {
        ExecuteOnUIThread(() => LastUpdateState = message.State);
    }
}