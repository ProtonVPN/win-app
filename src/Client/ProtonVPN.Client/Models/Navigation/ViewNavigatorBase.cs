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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Models.Navigation;

// For more information on navigation between pages see
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
public abstract class ViewNavigatorBase : IViewNavigator
{
    private readonly ILogger _logger;
    private readonly IViewMapper _viewMapper;
    private Window? _window;
    private Frame? _frame;
    private object? _lastParameterUsed;

    private bool _isRegisteredToFrameEvents = false;

    [MemberNotNullWhen(true, nameof(Frame), nameof(_frame))]
    public bool CanGoBack => Frame != null && Frame.CanGoBack;

    public bool CanNavigate { get; set; } = true;

    public Window Window
    {
        get => _window ?? throw new InvalidOperationException("The window associated to this view navigator has not been set yet.");
        set
        {
            UnregisterWindowEvents();
            _window = value;
            RegisterWindowEvents();
        }
    }

    public Frame Frame
    {
        get => _frame ?? throw new InvalidOperationException("The frame associated to this view navigator has not been set yet.");
        set
        {
            UnregisterFrameEvents();
            _frame = value;
            RegisterFrameEvents();
        }
    }

    protected NavigationTransitionInfo NavigationTransition { get; set; }

    public ViewNavigatorBase(ILogger logger, IViewMapper viewMapper)
    {
        _logger = logger;
        _viewMapper = viewMapper;

        NavigationTransition = new DrillInNavigationTransitionInfo();
    }

    public event NavigatedEventHandler? Navigated;

    public async Task<bool> GoBackAsync()
    {
        if (CanGoBack)
        {
            INavigationAware? vmBeforeNavigation = Frame.GetPageViewModel() as INavigationAware;

            if (vmBeforeNavigation != null)
            {
                bool continueNavigation = await vmBeforeNavigation.OnNavigatingFromAsync();
                if (!continueNavigation)
                {
                    return false;
                }
            }

            Frame.GoBack();

            vmBeforeNavigation?.OnNavigatedFrom();

            return true;
        }

        return false;
    }

    public Task<bool> NavigateToAsync(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        Type pageType = _viewMapper.GetPageType(pageKey);

        return NavigateToPageAsync(pageType, parameter, clearNavigation);
    }

    public Task<bool> NavigateToAsync<TPageViewModel>(object? parameter = null, bool clearNavigation = false)
        where TPageViewModel : PageViewModelBase
    {
        Type pageType = _viewMapper.GetPageType<TPageViewModel>();

        return NavigateToPageAsync(pageType, parameter, clearNavigation);
    }

    public void CloseCurrentWindow()
    {
        _window?.Close();
    }

    private async Task<bool> NavigateToPageAsync(Type pageType, object? parameter, bool clearNavigation)
    {
        if (!CanNavigate)
        {
            return false;
        }

        if (Frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed)))
        {
            Frame.Tag = clearNavigation;

            INavigationAware? vmBeforeNavigation = Frame.GetPageViewModel() as INavigationAware;

            if (vmBeforeNavigation != null)
            {
                bool continueNavigation = await vmBeforeNavigation.OnNavigatingFromAsync();
                if (!continueNavigation)
                {
                    return false;
                }
            }

            bool navigated = Frame.Navigate(pageType, parameter, NavigationTransition);

            if (navigated)
            {
                _lastParameterUsed = parameter;
                vmBeforeNavigation?.OnNavigatedFrom();
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

    private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
    {
        RegisterFrameEvents();
    }

    private void OnWindowClosed(object sender, WindowEventArgs e)
    {
        UnregisterFrameEvents();
    }

    private void RegisterFrameEvents()
    {
        if (_frame != null && !_isRegisteredToFrameEvents)
        {
            _frame.Navigated += OnNavigated;

            _isRegisteredToFrameEvents = true;
        }
    }

    private void UnregisterFrameEvents()
    {
        if (_frame != null && _isRegisteredToFrameEvents)
        {
            _frame.Navigated -= OnNavigated;

            _isRegisteredToFrameEvents = false;
        }
    }

    private void RegisterWindowEvents()
    {
        if (_window != null)
        {
            _window.Activated += OnWindowActivated;
            _window.Closed += OnWindowClosed;
        }
    }

    private void UnregisterWindowEvents()
    {
        if (_window != null)
        {
            _window.Activated -= OnWindowActivated;
            _window.Closed -= OnWindowClosed;
        }
    }
}