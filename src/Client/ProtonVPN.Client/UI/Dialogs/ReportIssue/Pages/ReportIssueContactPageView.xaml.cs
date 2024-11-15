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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Core.Bases;

namespace ProtonVPN.Client.UI.Dialogs.ReportIssue.Pages;

public sealed partial class ReportIssueContactPageView : IContextAware
{
    public ReportIssueContactPageViewModel ViewModel { get; }

    public ReportIssueContactPageView()
    {
        ViewModel = App.GetService<ReportIssueContactPageViewModel>();

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        NoLogsWarningInfoBar.RegisterPropertyChangedCallback(InfoBar.IsOpenProperty, OnInfoBarIsOpenChanged);
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Activate();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
    }

    private void OnInfoBarIsOpenChanged(DependencyObject sender, DependencyProperty dp)
    {
        if (NoLogsWarningInfoBar.IsOpen)
        {
            NoLogsWarningInfoBar.SizeChanged += OnNoLogsWarningInfoBarSizeChanged;
        }
    }

    private void OnNoLogsWarningInfoBarSizeChanged(object sender, SizeChangedEventArgs e)
    {
        NoLogsWarningInfoBar.SizeChanged -= OnNoLogsWarningInfoBarSizeChanged;
        NoLogsWarningInfoBar.StartBringIntoView();
    }
}