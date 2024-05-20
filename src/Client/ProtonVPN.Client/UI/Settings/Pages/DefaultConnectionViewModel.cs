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
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.UI.Settings.Pages.Entities;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Settings.Pages;

public partial class DefaultConnectionViewModel : SettingsPageViewModelBase
{
    [ObservableProperty]
    [property: SettingName(nameof(ISettings.DefaultConnection))]
    [NotifyPropertyChangedFor(nameof(IsFastestConnection))]
    [NotifyPropertyChangedFor(nameof(IsLastConnection))]
    private DefaultConnection _currentDefaultConnection;

    public override string Title => Localizer.Get("Settings_Connection_Default");

    public bool IsFastestConnection
    {
        get => IsDefaultConnection(DefaultConnection.Fastest);
        set => SetDefaultConnection(value, DefaultConnection.Fastest);
    }

    public bool IsLastConnection
    {
        get => IsDefaultConnection(DefaultConnection.Last);
        set => SetDefaultConnection(value, DefaultConnection.Last);
    }

    public DefaultConnectionViewModel(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IOverlayActivator overlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator,
               localizationProvider,
               overlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               logger,
               issueReporter)
    { }

    protected override void SaveSettings()
    {
        Settings.DefaultConnection = CurrentDefaultConnection;
    }

    protected override void RetrieveSettings()
    {
        CurrentDefaultConnection = Settings.DefaultConnection;
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        yield return new(nameof(ISettings.DefaultConnection), CurrentDefaultConnection, Settings.DefaultConnection != CurrentDefaultConnection);
    }

    private bool IsDefaultConnection(DefaultConnection defaultConnection)
    {
        return CurrentDefaultConnection == defaultConnection;
    }

    private void SetDefaultConnection(bool value, DefaultConnection defaultConnection)
    {
        if (value)
        {
            CurrentDefaultConnection = defaultConnection;
        }
    }
}