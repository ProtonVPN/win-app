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

using Microsoft.UI.Xaml;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.Messages;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Activation.Bases;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Dialogs.ReportIssue;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Services.Activation;

public class ReportIssueWindowActivator : WindowActivatorBase<ReportIssueWindow>, IReportIssueWindowActivator,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>,
    IEventMessageReceiver<ApplicationStoppedMessage>
{
    public override string WindowTitle => Localizer.Get("Dialogs_ReportIssue_Title");

    public ReportIssueWindowActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector)
        : base(logger, uiThreadDispatcher, themeSelector, settings, localizer, iconSelector)
    {
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        if (Host != null && Host.Visible)
        {
            if (message.IsMainWindowVisible)
            {
                Activate();
            }
            else
            {
                Hide();
            }
        }
    }

    protected override void OnWindowClosing(WindowEventArgs e)
    {
        base.OnWindowClosing(e);

        e.Handled = true;
        Hide();
    }

    public void Receive(ApplicationStoppedMessage message)
    {
        Exit();
    }
}