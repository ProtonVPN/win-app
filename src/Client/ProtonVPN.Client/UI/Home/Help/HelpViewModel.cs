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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Dialogs.Windows;

namespace ProtonVPN.Client.UI.Home.Help;

public partial class HelpViewModel : ViewModelBase
{
    private readonly IDialogActivator _dialogActivator;
    private readonly IReportIssueViewNavigator _reportIssueViewNavigator;

    public HelpViewModel(ILocalizationProvider localizationProvider, IDialogActivator dialogActivator, IReportIssueViewNavigator reportIssueViewNavigator)
        : base(localizationProvider)
    {
        _dialogActivator = dialogActivator;
        _reportIssueViewNavigator = reportIssueViewNavigator;
    }

    [RelayCommand]
    public void OpenReportIssueDialog()
    {
        _dialogActivator.ShowDialog<ReportIssueShellViewModel>();
    }
}