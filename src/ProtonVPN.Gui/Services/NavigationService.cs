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

using System.Diagnostics.CodeAnalysis;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.Contracts.ViewModels;
using ProtonVPN.Gui.Helpers;

namespace ProtonVPN.Gui.Services;

// For more information on navigation between pages see
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
public class NavigationService : INavigationService
{
    private readonly IPageService _pageService;
    private Frame? _frame;
    private object? _lastParameterUsed;

    public NavigationService(IPageService pageService)
    {
        _pageService = pageService;
    }

    public event NavigatedEventHandler? Navigated;

    [MemberNotNullWhen(true, nameof(Frame), nameof(_frame))]
    public bool CanGoBack => Frame != null && Frame.CanGoBack;

    public Frame? Frame
    {
        get
        {
            if (_frame == null)
            {
                _frame = App.MainWindow.Content as Frame;
                RegisterFrameEvents();
            }

            return _frame;
        }

        set
        {
            UnregisterFrameEvents();
            _frame = value;
            RegisterFrameEvents();
        }
    }

    public bool GoBack()
    {
        if (CanGoBack)
        {
            object? vmBeforeNavigation = _frame.GetPageViewModel();
            _frame.GoBack();
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }

            return true;
        }

        return false;
    }

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        Type pageType = _pageService.GetPageType(pageKey);

        if (_frame != null && (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed))))
        {
            _frame.Tag = clearNavigation;
            object? vmBeforeNavigation = _frame.GetPageViewModel();
            bool navigated = _frame.Navigate(pageType, parameter);
            if (navigated)
            {
                _lastParameterUsed = parameter;
                if (vmBeforeNavigation is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }
            }

            return navigated;
        }

        return false;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            bool clearNavigation = (bool)frame.Tag;
            if (clearNavigation)
            {
                frame.BackStack.Clear();
            }

            if (frame.GetPageViewModel() is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.Parameter);
            }

            Navigated?.Invoke(sender, e);
        }
    }

    private void RegisterFrameEvents()
    {
        if (_frame != null)
        {
            _frame.Navigated += OnNavigated;
        }
    }

    private void UnregisterFrameEvents()
    {
        if (_frame != null)
        {
            _frame.Navigated -= OnNavigated;
        }
    }
}