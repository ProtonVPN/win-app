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
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Legacy.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Legacy.Models.Activation.Custom;
using ProtonVPN.Client.Legacy.Models.Clipboards;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Legacy.UI.Features.PortForwarding;
using ProtonVPN.Client.Legacy.UI.Sidebar.Bases;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.UI.Sidebar;

public partial class SidebarPortForwardingViewModel : SidebarFeatureNavigationItemViewModelBase<PortForwardingViewModel>,
    IEventMessageReceiver<PortForwardingPortChanged>,
    IEventMessageReceiver<PortForwardingStatusChanged>
{
    private readonly IConnectionManager _connectionManager;
    private readonly IPortForwardingManager _portForwardingManager;
    private readonly IClipboardEditor _clipboardEditor;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CopyPortNumberCommand))]
    [NotifyPropertyChangedFor(nameof(HasActivePortNumber))]
    [NotifyPropertyChangedFor(nameof(Status))]
    private int? _activePortNumber;

    public bool HasActivePortNumber => ActivePortNumber.HasValue;

    public override string Header => Localizer.Get("Settings_Connection_PortForwarding");

    public override bool RequiresPaidAccess => true;

    protected override ModalSources UpsellModalSource => ModalSources.PortForwarding;

    public override string AutomationId => "Sidebar_Features_PortForwarding";

    public SidebarPortForwardingViewModel(
        IMainViewNavigator mainViewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IUpsellCarouselDialogActivator upsellCarouselDialogActivator,
        IConnectionManager connectionManager,
        IPortForwardingManager portForwardingManager,
        IClipboardEditor clipboardEditor)
        : base(mainViewNavigator, localizationProvider, logger, issueReporter, settings, upsellCarouselDialogActivator)
    {
        _connectionManager = connectionManager;
        _portForwardingManager = portForwardingManager;
        _clipboardEditor = clipboardEditor;
    }

    public void Receive(PortForwardingStatusChanged message)
    {
        ExecuteOnUIThread(InvalidateActivePort);
    }

    public void Receive(PortForwardingPortChanged message)
    {
        ExecuteOnUIThread(InvalidateActivePort);
    }

    [RelayCommand(CanExecute = nameof(CanCopyPortNumber))]
    public async Task CopyPortNumberAsync()
    {
        int? activePortNumber = ActivePortNumber;
        if (activePortNumber is not null)
        {
            await _clipboardEditor.SetTextAsync($"{activePortNumber}");
        }
    }

    public bool CanCopyPortNumber()
    {
        return HasActivePortNumber;
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        base.OnSettingsChanged(propertyName);

        switch (propertyName)
        {
            case nameof(ISettings.IsPortForwardingEnabled):
                ExecuteOnUIThread(InvalidateAllProperties);
                break;

            default:
                break;
        }
    }

    protected override string GetFeatureStatus()
    {
        if (!_connectionManager.IsConnected || !Settings.IsPortForwardingEnabled)
        {
            return Localizer.GetToggleValue(Settings.IsPortForwardingEnabled);
        }

        return ActivePortNumber?.ToString()
            ?? (_portForwardingManager.IsFetchingPort
                ? Localizer.Get("Settings_Connection_PortForwarding_Loading")
                : Localizer.GetToggleValue(Settings.IsPortForwardingEnabled));
    }

    protected override ImageSource GetFeatureIconSource()
    {
        return ResourceHelper.GetIllustration(
            Settings.IsPortForwardingEnabled
                ? "PortForwardingOnIllustrationSource"
                : "PortForwardingOffIllustrationSource");
    }

    private void InvalidateActivePort()
    {
        ActivePortNumber = _portForwardingManager.ActivePort;
    }
}