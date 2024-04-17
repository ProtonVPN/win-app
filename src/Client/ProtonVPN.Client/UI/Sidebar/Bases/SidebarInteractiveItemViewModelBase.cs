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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Sidebar.Bases;

public abstract class SidebarInteractiveItemViewModelBase : SidebarHeaderViewModelBase,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>
{
    public abstract IconElement? Icon { get; }

    public virtual string? Status { get; }

    public virtual bool RequiresPaidAccess => false;

    public string? ToolTip => IsSidebarOpened ? null : Header;

    public bool IsRestricted => RequiresPaidAccess && !Settings.VpnPlan.IsPaid;

    public bool IsSidebarOpened => Settings.IsNavigationPaneOpened;

    public ISettings Settings { get; }

    protected SidebarInteractiveItemViewModelBase(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings)
        : base(localizationProvider,
               logger,
               issueReporter)
    {
        Settings = settings;
    }

    public abstract Task<bool> InvokeAsync();

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() => OnSettingsChanged(message.PropertyName));
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    protected virtual void OnSettingsChanged(string propertyName)
    {
        if (propertyName == nameof(ISettings.IsNavigationPaneOpened))
        {
            OnPropertyChanged(nameof(IsSidebarOpened));
            OnPropertyChanged(nameof(ToolTip));
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(ToolTip));
    }
}