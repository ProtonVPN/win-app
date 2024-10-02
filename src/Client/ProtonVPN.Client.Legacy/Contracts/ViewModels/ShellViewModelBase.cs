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
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Legacy.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.Contracts.ViewModels;

public abstract partial class ShellViewModelBase<TViewNavigator> : PageViewModelBase<TViewNavigator>
    where TViewNavigator : IViewNavigator
{
    public override bool IsBackEnabled => ViewNavigator.CanGoBack;

    public PageViewModelBase? CurrentPage => ViewNavigator?.Frame?.GetPageViewModel() as PageViewModelBase;

    protected ShellViewModelBase(TViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator, localizationProvider, logger, issueReporter)
    {
        ViewNavigator.Navigated += OnNavigated;
    }

    public void InitializeViewNavigator(Window window, Frame frame)
    {
        ViewNavigator.Window = window;
        ViewNavigator.Frame = frame;

        OnViewNavigatorInitialized();
    }

    protected virtual void OnViewNavigatorInitialized()
    { }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        ExecuteOnUIThread(OnNavigated);
    }

    protected virtual void OnNavigated()
    {
        OnPropertyChanged(nameof(IsBackEnabled));
        OnPropertyChanged(nameof(CurrentPage));
    }
}