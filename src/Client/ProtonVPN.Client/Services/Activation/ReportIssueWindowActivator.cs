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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Activation.Bases;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Contracts.Services.Selection;
using ProtonVPN.Client.UI.Dialogs.ReportIssue;
using ProtonVPN.Client.UI.Dialogs.ReportIssue.Pages;
using ProtonVPN.Client.Common.Messages;

namespace ProtonVPN.Client.Services.Activation;

public class ReportIssueWindowActivator : WindowActivatorBase<ReportIssueWindow>, IReportIssueWindowActivator,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>,
    IEventMessageReceiver<ApplicationStoppedMessage>
{
    private readonly IReportIssueWindowOverlayActivator _reportIssueOverlayActivator;
    private readonly IReportIssueViewNavigator _reportIssueViewNavigator;

    public override string WindowTitle => Localizer.Get("Dialogs_ReportIssue_Title");

    public ReportIssueWindowActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector,
        IReportIssueWindowOverlayActivator reportIssueOverlayActivator,
        IReportIssueViewNavigator reportIssueViewNavigator)
        : base(logger, uiThreadDispatcher, themeSelector, settings, localizer, iconSelector)
    {
        _reportIssueOverlayActivator = reportIssueOverlayActivator;
        _reportIssueViewNavigator = reportIssueViewNavigator;
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        if (Host != null)
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

    public void Receive(ApplicationStoppedMessage message)
    {
        Exit();
    }

    protected override void OnWindowClosing(WindowEventArgs e)
    {
        base.OnWindowClosing(e);

        if (_reportIssueViewNavigator.GetCurrentPageContext() is not ReportIssueResultPageViewModel reportIssueResult || !reportIssueResult.IsReportSent)
        {
            e.Handled = true;
        }
    }

    protected override async void OnWindowCloseAborted()
    {
        base.OnWindowCloseAborted();

        // POC just to show how to handle closing event and trigger overlays on dialogs
        ContentDialogResult result = await _reportIssueOverlayActivator.ShowMessageAsync(new()
        {
            Title = "Report not sent",
            Message = "You haven't sent the report yet. Are you sure you want to close this window?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "No"
        });
        if (result == ContentDialogResult.Primary)
        {
            Exit();
        }
    }
}