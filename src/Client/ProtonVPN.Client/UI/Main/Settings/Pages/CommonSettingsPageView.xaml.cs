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
using ProtonVPN.Client.Core.Bases;

namespace ProtonVPN.Client.UI.Main.Settings.Pages;

public sealed partial class CommonSettingsPageView : IContextAware
{
    private Control? _lastFocusedElement;

    public CommonSettingsPageViewModel ViewModel { get; }

    public CommonSettingsPageView()
    {
        ViewModel = App.GetService<CommonSettingsPageViewModel>();

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        ViewModel.ResetContentScrollRequested += OnResetContentScrollRequested;
        
        cbLanguage.GotFocus += OnLanguageComboBoxGotFocus;
        cbLanguage.SelectionChanged += OnLanguageSelectionChanged;
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

    private void OnResetContentScrollRequested(object? sender, EventArgs e)
    {
        PageContentHost.ResetContentScroll();
    }

    private void OnLanguageComboBoxGotFocus(object sender, RoutedEventArgs e)
    {
        _lastFocusedElement = sender as Control;
    }

    private void OnLanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cbLanguage.FocusState != FocusState.Unfocused)
        {
            _lastFocusedElement = cbLanguage;
            
            DispatcherQueue.TryEnqueue(RestoreFocusToLanguageComboBox);
        }
    }

    private void RestoreFocusToLanguageComboBox()
    {
        if (_lastFocusedElement == cbLanguage && cbLanguage.IsLoaded)
        {
            cbLanguage.Focus(FocusState.Keyboard);
            _lastFocusedElement = null;
        }
    }
}