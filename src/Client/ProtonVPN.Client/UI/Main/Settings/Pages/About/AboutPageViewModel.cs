/*
 * Copyright (c) 2023 Proton AG
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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Settings.Pages.About.Models;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Update.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.UI.Settings.Pages.Entities;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.About;

public partial class AboutPageViewModel : SettingsPageViewModelBase,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>
{
    private readonly IUpdatesManager _updatesManager;
    private readonly ReleaseViewModelFactory _releaseViewModelFactory;

    [ObservableProperty]
    private string _clientVersion;

    [ObservableProperty]
    private IReadOnlyList<Release> _releases = [];

    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    private bool _isToShowLoadingComponent;

    [ObservableProperty]
    private bool _isToShowErrorComponent;

    public override string Title => Localizer.Get("Settings_About_Title");

    public AboutPageViewModel(
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IUpdatesManager updatesManager,
        ReleaseViewModelFactory releaseViewModelFactory,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager)
        : base(
            requiredReconnectionSettings,
            mainViewNavigator,
            settingsViewNavigator,
            localizer,
            logger,
            issueReporter,
            mainWindowOverlayActivator,
            settings,
            settingsConflictResolver,
            connectionManager)
    {
        _updatesManager = updatesManager;
        _releaseViewModelFactory = releaseViewModelFactory;

        ClientVersion = AssemblyVersion.Get();
    }

    public void Receive(ClientUpdateStateChangedMessage message)
    {
        ExecuteOnUIThread(() => HandleUpdateStateChangedMessage(message));
    }

    private void HandleUpdateStateChangedMessage(ClientUpdateStateChangedMessage message)
    {
        IsUpdateAvailable = message.IsUpdateAvailable;
        if (message.State?.Status is AppUpdateStatus.None or AppUpdateStatus.Ready or AppUpdateStatus.AutoUpdated && message.State?.ReleaseHistory.Count > 0)
        {
            Releases = _releaseViewModelFactory.GetReleases(message.State.ReleaseHistory);
        }

        IsToShowErrorComponent = IsToShowLoadingComponent && message.State?.Status is AppUpdateStatus.CheckFailed;

        if (message.State?.Status is not AppUpdateStatus.Checking && IsToShowLoadingComponent)
        {
            IsToShowLoadingComponent = false;
        }
    }

    public override void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

        StartCheckingForUpdate();
    }

    private void StartCheckingForUpdate()
    {
        if (Releases.Count == 0)
        {
            IsToShowLoadingComponent = true;
        }

        _updatesManager.CheckForUpdate(true);
    }

    [RelayCommand]
    private async Task NavigateToLicensingPageAsync()
    {
        await ParentViewNavigator.NavigateToLicensingViewAsync();
    }

    [RelayCommand]
    public void TryAgain()
    {
        StartCheckingForUpdate();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        Releases = [];
        StartCheckingForUpdate();
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        return [];
    }
}