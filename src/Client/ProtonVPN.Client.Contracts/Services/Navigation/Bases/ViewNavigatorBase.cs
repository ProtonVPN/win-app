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

using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Client.Contracts.Bases;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Services.Activation.Bases;
using ProtonVPN.Client.Contracts.Services.Mapping;

namespace ProtonVPN.Client.Contracts.Services.Navigation.Bases;

public abstract class ViewNavigatorBase : FrameActivatorBase, IViewNavigator
{
    private readonly IPageViewMapper _pageViewMapper;

    private object? _lastParameterUsed;

    public virtual FrameLoadedBehavior LoadBehavior { get; protected set; } = FrameLoadedBehavior.NavigateToDefaultView;

    public virtual FrameUnloadedBehavior UnloadBehavior { get; protected set; } = FrameUnloadedBehavior.DoNothing;

    public virtual bool IsNavigationStackEnabled => false;

    public bool CanGoBack => Host != null && Host.CanGoBack;

    public bool CanNavigate { get; set; } = true;

    protected virtual NavigationTransitionInfo ForwardTransitionInfo { get; } = new DrillInNavigationTransitionInfo();

    protected ViewNavigatorBase(
        ILogger logger,
        IPageViewMapper pageViewMapper)
        : base(logger)
    {
        _pageViewMapper = pageViewMapper;
    }

    public event NavigatedEventHandler? Navigated;

    public PageViewModelBase? GetCurrentPageContext()
    {
        return Host?.Content is IContextAware contextAware
            ? contextAware.GetContext() as PageViewModelBase
            : null;
    }

    public async Task<bool> GoBackAsync(bool forceNavigation = false)
    {
        if (Host == null)
        {
            Logger.Error<AppLog>("Frame has not been initialized");
            return false;
        }

        if (!CanGoBack)
        {
            Logger.Error<AppLog>("Backward navigation on that frame has been disabled");
            return false;
        }

        INavigationAware? context = GetCurrentPageContext();
        if (context != null && !forceNavigation)
        {
            bool shouldContinueNavigation = await context.CanNavigateFromAsync();
            if (!shouldContinueNavigation)
            {
                return false;
            }
        }

        Host.GoBack();

        context?.OnNavigatedFrom();

        return true;
    }

    public abstract Task<bool> NavigateToDefaultAsync();

    public void ClearFrameContent()
    {
        ClearBackStack();

        if (Host?.Content != null)
        {
            Host.Content = null;
        }
    }

    public Task<bool> NavigateToAsync(PageViewModelBase pageViewModel, object? parameter = null, bool clearNavigation = false, bool forceNavigation = false)
    {
        Type pageType = _pageViewMapper.GetViewType(pageViewModel.GetType());

        return NavigateToAsync(pageType, parameter, clearNavigation, forceNavigation);
    }

    protected Task<bool> NavigateToAsync<TPageViewModel>(object? parameter = null, bool clearNavigation = false, bool forceNavigation = false)
        where TPageViewModel : PageViewModelBase
    {
        Type pageType = _pageViewMapper.GetViewType<TPageViewModel>();

        return NavigateToAsync(pageType, parameter, clearNavigation, forceNavigation);
    }

    protected void ClearBackStack()
    {
        Host?.BackStack.Clear();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        TriggerLoadBehavior();
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();

        TriggerLoadBehavior();
    }

    protected override void OnUnloaded()
    {
        base.OnUnloaded();

        TriggerUnloadBehavior();
    }

    protected override void OnFrameNavigated(NavigationEventArgs e)
    {
        bool isBackNavigation = e.NavigationMode == NavigationMode.Back;

        bool clearNavigation = (bool)(Host?.Tag ?? false);
        if (clearNavigation)
        {
            ClearBackStack();
        }

        if (GetCurrentPageContext() is INavigationAware context)
        {
            context?.OnNavigatedTo(e.Parameter, isBackNavigation);
        }

        Navigated?.Invoke(this, e);
    }

    private async Task<bool> NavigateToAsync(Type pageType, object? parameter = null, bool clearNavigation = false, bool forceNavigation = false)
    {
        if (Host == null)
        {
            Logger.Error<AppLog>("Frame has not been initialized");
            return false;
        }

        if (!CanNavigate)
        {
            Logger.Error<AppLog>("Navigation on that frame has been disabled");
            return false;
        }

        if (Host.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed)))
        {
            Host.Tag = clearNavigation;

            INavigationAware? context = GetCurrentPageContext();
            if (context != null && !forceNavigation)
            {
                bool shouldContinueNavigation = await context.CanNavigateFromAsync();
                if (!shouldContinueNavigation)
                {
                    return false;
                }
            }

            bool navigated = Host.Navigate(pageType, parameter, ForwardTransitionInfo);

            if (navigated)
            {
                _lastParameterUsed = parameter;
                context?.OnNavigatedFrom();
            }

            return navigated;
        }

        return true;
    }

    private void TriggerLoadBehavior()
    {
        switch (LoadBehavior)
        {
            case FrameLoadedBehavior.NavigateToDefaultView:
            case FrameLoadedBehavior.NavigateToDefaultViewIfEmpty when Host?.Content == null:
                NavigateToDefaultAsync();
                break;
        }
    }

    private void TriggerUnloadBehavior()
    {
        switch (UnloadBehavior)
        {
            case FrameUnloadedBehavior.ClearFrameContent when Host != null:
                ClearFrameContent();
                break;
        }
    }
}