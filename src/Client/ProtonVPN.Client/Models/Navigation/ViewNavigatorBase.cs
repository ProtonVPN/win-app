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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Parameters;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Models.Navigation;

// For more information on navigation between pages see
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
public abstract class ViewNavigatorBase : IViewNavigator, IEventMessageReceiver<ThemeChangedMessage>
{
    private readonly ILogger _logger;
    private readonly IViewMapper _viewMapper;
    private readonly IThemeSelector _themeSelector;

    private readonly SemaphoreSlim _overlaySemaphore = new(1);

    private Window? _window;
    private Frame? _frame;

    private object? _lastParameterUsed;
    private ContentDialog? _activeOverlay;

    [MemberNotNullWhen(true, nameof(Frame), nameof(_frame))]
    public bool CanGoBack => Frame != null && Frame.CanGoBack;

    public bool CanNavigate { get; set; } = true;

    public Window? Window
    {
        get => _window;
        set
        {
            UnregisterWindowEvents();
            _window = value;
            RegisterWindowEvents();
        }
    }

    public Frame? Frame
    {
        get => _frame;
        set
        {
            UnregisterFrameEvents();
            _frame = value;
            RegisterFrameEvents();
        }
    }

    public ViewNavigatorBase(ILogger logger, IViewMapper viewMapper, IThemeSelector themeSelector)
    {
        _logger = logger;
        _viewMapper = viewMapper;
        _themeSelector = themeSelector;

        NavigationTransition = new DrillInNavigationTransitionInfo();
    }

    public event NavigatedEventHandler? Navigated;

    public bool GoBack()
    {
        if (CanGoBack)
        {
            object? vmBeforeNavigation = _frame.GetPageViewModel();
            Frame.GoBack();
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
        Type pageType = _viewMapper.GetPageType(pageKey);

        return NavigateToPage(pageType, parameter, clearNavigation);
    }

    public bool NavigateTo<TPageViewModel>(object? parameter = null, bool clearNavigation = false)
        where TPageViewModel : PageViewModelBase
    {
        Type pageType = _viewMapper.GetPageType<TPageViewModel>();

        return NavigateToPage(pageType, parameter, clearNavigation);
    }

    public Task ShowOverlayAsync(string overlayKey)
    {
        Type overlayType = _viewMapper.GetOverlayType(overlayKey);

        return ShowOverlayAsync(overlayType);
    }

    public Task ShowOverlayAsync<TOverlayViewModel>()
        where TOverlayViewModel : OverlayViewModelBase
    {
        Type overlayType = _viewMapper.GetOverlayType<TOverlayViewModel>();

        return ShowOverlayAsync(overlayType);
    }

    public async Task<ContentDialogResult> ShowMessageAsync(MessageDialogParameters parameters)
    {
        if (Frame == null)
        {
            throw new InvalidOperationException("Frame has not been initialized for this view navigator");
        }

        if (Frame.XamlRoot == null)
        {
            // Xaml root is set only when the element is loaded in the visual tree. If not already set, give some time for loaded event to trigger
            await Task.Delay(500);
        }

        try
        {
            await _overlaySemaphore.WaitAsync();

            _activeOverlay = new ContentDialog
            {
                Title = parameters.Title,
                Content = parameters.Message,
                PrimaryButtonText = parameters.PrimaryButtonText,
                SecondaryButtonText = parameters.SecondaryButtonText,
                CloseButtonText = parameters.CloseButtonText,
                XamlRoot = Frame.XamlRoot,
                RequestedTheme = _themeSelector.GetTheme().Theme,
            };

            ContentDialogResult result = await _activeOverlay.ShowAsync();

            return result;
        }
        finally
        {
            _activeOverlay = null;
            _overlaySemaphore.Release();
        }
    }

    public void CloseOverlay()
    {
        _activeOverlay?.Hide();
    }

    public void Receive(ThemeChangedMessage message)
    {
        if (_activeOverlay != null)
        {
            _activeOverlay.RequestedTheme = _themeSelector.GetTheme().Theme;
        }
    }

    protected NavigationTransitionInfo NavigationTransition { get; set; } 


    private bool NavigateToPage(Type pageType, object? parameter, bool clearNavigation)
    {
        if (!CanNavigate)
        {
            return false;
        }

        if (Frame == null)
        {
            throw new InvalidOperationException("Frame has not been initialized for this view navigator");
        }

        if (Frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed)))
        {
            Frame.Tag = clearNavigation;
            object? vmBeforeNavigation = Frame.GetPageViewModel();
            bool navigated = Frame.Navigate(pageType, parameter, NavigationTransition);
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

    private async Task ShowOverlayAsync(Type overlayType)
    {
        if (Frame == null)
        {
            throw new InvalidOperationException("Frame has not been initialized for this view navigator");
        }

        if (Frame.XamlRoot == null)
        {
            // Xaml root is set only when the element is loaded in the visual tree. If not already set, give some time for loaded event to trigger
            await Task.Delay(500);
        }

        try
        {
            await _overlaySemaphore.WaitAsync();

            _activeOverlay = Activator.CreateInstance(overlayType) as ContentDialog
                ?? throw new InvalidCastException($"Type {overlayType} is not recognized as a ContentDialog.");

            _activeOverlay.XamlRoot = Frame.XamlRoot;
            _activeOverlay.RequestedTheme = _themeSelector.GetTheme().Theme;

            await _activeOverlay.ShowAsync();
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>($"Error when trying to show overlay for '{overlayType}'", e);
            throw;
        }
        finally
        {
            _activeOverlay = null;
            _overlaySemaphore.Release();
        }
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

    private void OnWindowClosed(object sender, WindowEventArgs e)
    {
        CloseOverlay();

        UnregisterWindowEvents();
        UnregisterFrameEvents();
    }

    private void RegisterFrameEvents()
    {
        if (Frame != null)
        {
            Frame.Navigated += OnNavigated;
        }
    }

    private void UnregisterFrameEvents()
    {
        if (Frame != null)
        {
            Frame.Navigated -= OnNavigated;
        }
    }

    private void RegisterWindowEvents()
    {
        if (Window != null)
        {
            Window.Closed += OnWindowClosed;
        }
    }

    private void UnregisterWindowEvents()
    {
        if (Window != null)
        {
            Window.Closed -= OnWindowClosed;
        }
    }
}