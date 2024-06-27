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

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Sidebar.Bases;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Sidebar;

public partial class SidebarAccountViewModel : SidebarInteractiveItemViewModelBase
{
    private readonly ISettings _settings;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IConnectionManager _connectionManager;
    private readonly IOverlayActivator _overlayActivator;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IUrls _urls;
    private readonly IMainWindowActivator _mainWindowActivator;

    private readonly IconElement _icon = new User();

    public override IconElement? Icon => IsSidebarOpened ? null : _icon;

    public override string Header => $"{Username}\n{VpnPlan}";

    public string Username => Settings.Username ?? Settings.UserDisplayName ?? string.Empty;

    public string VpnPlan => Localizer.GetVpnPlanName(Settings.VpnPlan.Title);

    public bool IsPaid => Settings.VpnPlan.IsPaid;

    public override string AutomationId => "Sidebar_Account";

    public SidebarAccountViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IUserAuthenticator userAuthenticator,
        IConnectionManager connectionManager,
        IOverlayActivator overlayActivator,
        IWebAuthenticator webAuthenticator,
        IUrls urls,
        IMainWindowActivator mainWindowActivator)
        : base(localizationProvider, logger, issueReporter, settings)
    {
        _settings = settings;
        _userAuthenticator = userAuthenticator;
        _connectionManager = connectionManager;
        _overlayActivator = overlayActivator;
        _webAuthenticator = webAuthenticator;
        _urls = urls;
        _mainWindowActivator = mainWindowActivator;
    }

    [RelayCommand]
    public async Task GoToMyAccountAsync()
    {
        _urls.NavigateTo(await _webAuthenticator.GetMyAccountUrlAsync());
    }

    [RelayCommand]
    public async Task SignOutAsync()
    {
        ContentDialogResult result = await _overlayActivator.ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = Localizer.GetFormat("Home_Account_SignOut_Confirmation_Title", Username),
                Message = Localizer.GetExitOrSignOutConfirmationMessage(_connectionManager.IsDisconnected, _settings),
                MessageType = DialogMessageType.RichText,
                PrimaryButtonText = Localizer.Get("Home_Account_SignOut"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
            });

        if (result is not ContentDialogResult.Primary) // Cancel sign out
        {
            return;
        }

        await _userAuthenticator.LogoutAsync(LogoutReason.UserAction);
    }

    [RelayCommand]
    public async Task ExitApplicationAsync()
    {
        await _mainWindowActivator.TryExitAsync();
    }

    public override Task<bool> InvokeAsync()
    {
        // Do nothing
        return Task.FromResult(true);
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        base.OnSettingsChanged(propertyName);

        if (propertyName == nameof(ISettings.IsNavigationPaneOpened))
        {
            OnPropertyChanged(nameof(Icon));
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(VpnPlan));
    }
}