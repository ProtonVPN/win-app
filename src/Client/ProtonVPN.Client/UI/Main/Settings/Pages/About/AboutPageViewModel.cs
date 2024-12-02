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
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Client.UI.Settings.Pages.About.Models;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Update.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.About;

public partial class AboutPageViewModel : SettingsPageViewModelBase,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>
{
    private readonly IUpdatesManager _updatesManager;
    private readonly ReleaseViewModelFactory _releaseViewModelFactory;

    [ObservableProperty]
    private string _clientVersion;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsToShowLoadingComponent))]
    private IReadOnlyList<Release> _releases = [];

    [ObservableProperty]
    private bool _isToShowErrorComponent;

    public override string Title => Localizer.Get("Settings_About_Title");

    public bool IsUpdateAvailable => _updatesManager.IsUpdateAvailable;

    public bool IsToShowLoadingComponent => Releases.Count == 0;

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
        : base(requiredReconnectionSettings,
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
        ExecuteOnUIThread(() =>
        {
            if (message.State?.ReleaseHistory.Count > 0)
            {
                Releases = _releaseViewModelFactory.GetReleases(message.State.ReleaseHistory);
            }

            IsToShowErrorComponent = IsToShowLoadingComponent && message.State?.Status is AppUpdateStatus.CheckFailed;

            OnPropertyChanged(nameof(IsUpdateAvailable));
        });
    }

    public override void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

        StartCheckingForUpdate();
    }

    private void StartCheckingForUpdate()
    {
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