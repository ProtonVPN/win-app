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
using ProtonVPN.Client.Core.Bases;

namespace ProtonVPN.Client.UI.Main.Profiles;

public sealed partial class ProfilePageView : IContextAware
{
    public ProfilePageViewModel ViewModel { get; }

    public ProfilePageView()
    {
        ViewModel = App.GetService<ProfilePageViewModel>();

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        ViewModel.ResetContentScrollRequested += OnResetContentScrollRequested;
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

    private void OnProfileNameTextBoxLoaded(object sender, RoutedEventArgs e)
    {
        ProfileNameTextBox.Focus(FocusState.Programmatic);
        ProfileNameTextBox.SelectAll();
    }

    private void OnResetContentScrollRequested(object? sender, EventArgs e)
    {
        PageContentHost.ResetContentScroll();
    }

    private void OnSectionExpanded(object sender, EventArgs e)
    {
        BringElementIntoViewAsync(sender as FrameworkElement);
    }

    private async Task BringElementIntoViewAsync(FrameworkElement? element)
    {
        if (element == null)
        {
            return;
        }

        await Task.Delay(100);

        element.StartBringIntoView(new BringIntoViewOptions()
        {
            AnimationDesired = true,
            TargetRect = new Windows.Foundation.Rect(0, 0, element.ActualWidth, element.ActualHeight)
        });
    }
}