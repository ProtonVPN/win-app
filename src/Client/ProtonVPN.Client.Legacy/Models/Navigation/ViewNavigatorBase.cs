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
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.Legacy.Helpers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Legacy.Models.Navigation;

// For more information on navigation between pages see
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
public abstract class ViewNavigatorBase : IViewNavigator
{
    private readonly ILogger _logger;
    private readonly IViewMapper _viewMapper;

    private Window? _window;
    private Frame? _frame;

    private object? _lastParameterUsed;

    private bool _isRegisteredToFrameEvents;

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

    protected ViewNavigatorBase(
        ILogger logger,
        IViewMapper viewMapper)
    {
        _logger = logger;
        _viewMapper = viewMapper;

        NavigationTransition = new DrillInNavigationTransitionInfo();
    }

    public event NavigatedEventHandler? Navigated;

    public void CloseCurrentWindow()
    {
        _window?.Close();
    }

    public void ResetContent()
    {
        if (_frame?.Content != null)
        {
            _frame.Content = null;
        }
    }

    public async Task<bool> GoBackAsync(bool forceNavigation = false)
    {
        if (CanGoBack)
        {
            INavigationAware? currentPage = Frame.GetPageViewModel() as INavigationAware;

            if (currentPage != null && !forceNavigation)
            {
                bool shouldContinueNavigation = await currentPage.OnNavigatingFromAsync();
                if (!shouldContinueNavigation)
                {
                    return false;
                }
            }

            Frame.GoBack();

            currentPage?.OnNavigatedFrom();

            return true;
        }

        return false;
    }

    public Task<bool> NavigateToAsync<TPageViewModel>(object? parameter = null, bool clearNavigation = false, bool forceNavigation = false)
        where TPageViewModel : PageViewModelBase
    {
        Type pageType = _viewMapper.GetPageType<TPageViewModel>();

        return NavigateToPageAsync(pageType, parameter, clearNavigation, forceNavigation);
    }

    private async Task<bool> NavigateToPageAsync(Type pageType, object? parameter, bool clearNavigation, bool forceNavigation)
    {
        if (!CanNavigate || _frame == null)
        {
            return false;
        }

        if (Frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed)))
        {
            Frame.Tag = clearNavigation;

            INavigationAware? currentPage = Frame.GetPageViewModel() as INavigationAware;

            if (currentPage != null && !forceNavigation)
            {
                bool shouldContinueNavigation = await currentPage.OnNavigatingFromAsync();
                if (!shouldContinueNavigation)
                {
                    return false;
                }
            }

            bool navigated = Frame.Navigate(pageType, parameter, NavigationTransition);

            if (navigated)
            {
                _lastParameterUsed = parameter;
                currentPage?.OnNavigatedFrom();
            }

            return navigated;
        }

        return false;
    }

    private void OnNavigating(object sender, NavigatingCancelEventArgs e)
    {
        if (sender is Frame frame)
        {
            bool isBackNavigation = e.NavigationMode == NavigationMode.Back;

            if (_viewMapper.GetPageViewModel(e.SourcePageType) is INavigationAware targetPage)
            {
                bool shouldContinueNavigation = targetPage.OnNavigatingTo(e.Parameter, isBackNavigation);
                if (!shouldContinueNavigation)
                {
                    e.Cancel = true;
                }
            }
        }
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            bool isBackNavigation = e.NavigationMode == NavigationMode.Back;

            bool clearNavigation = (bool)frame.Tag;
            if (clearNavigation)
            {
                frame.BackStack.Clear();
            }

            if (frame.GetPageViewModel() is INavigationAware targetPage)
            {
                targetPage?.OnNavigatedTo(e.Parameter, isBackNavigation);
            }

            Navigated?.Invoke(sender, e);
        }
    }

    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        _logger.Error<AppLog>($"Navigation to the page '{e.SourcePageType}' has failed.", e.Exception);
    }

    private void OnNavigationStopped(object sender, NavigationEventArgs e)
    {
        _logger.Info<AppLog>($"Navigation to the page '{e.SourcePageType}' was stopped.");
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
            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;
            _frame.NavigationFailed += OnNavigationFailed;
            _frame.NavigationStopped += OnNavigationStopped;

            _isRegisteredToFrameEvents = true;
        }
    }

    private void UnregisterFrameEvents()
    {
        if (_frame != null && _isRegisteredToFrameEvents)
        {
            _frame.Navigating -= OnNavigating;
            _frame.Navigated -= OnNavigated;
            _frame.NavigationFailed -= OnNavigationFailed;
            _frame.NavigationStopped -= OnNavigationStopped;

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