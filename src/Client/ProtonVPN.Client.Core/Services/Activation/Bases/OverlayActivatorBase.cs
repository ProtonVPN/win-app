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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Common.UI.Controls.Custom;
using ProtonVPN.Client.Common.UI.Helpers;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Core.Services.Mapping;
using ProtonVPN.Client.Core.Services.Selection;
using System.Threading;

namespace ProtonVPN.Client.Core.Services.Activation.Bases;

public abstract class OverlayActivatorBase<TWindow> : WindowHostActivatorBase<TWindow>, IOverlayActivator
    where TWindow : BaseWindow
{
    protected readonly IOverlayViewMapper OverlayViewMapper;

    private SemaphoreSlim _overlaySemaphore = new(0, 1);

    private ContentDialog? _currentOverlay;

    public bool HasActiveOverlay => _currentOverlay != null;

    protected OverlayActivatorBase(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        IOverlayViewMapper overlayViewMapper)
        : base(logger, uiThreadDispatcher, themeSelector, settings)
    {
        OverlayViewMapper = overlayViewMapper;
    }

    public event EventHandler? OverlayChanged;

    public void CloseCurrentOverlay()
    {
        _currentOverlay?.Hide();
    }

    public OverlayViewModelBase? GetCurrentOverlayContext()
    {
        return _currentOverlay is IContextAware contextAware
            ? contextAware.GetContext() as OverlayViewModelBase
            : null;
    }

    public Task<ContentDialogResult> ShowMessageAsync(MessageDialogParameters parameters)
    {
        MessageContentDialog overlay = CreateMessageOverlay(parameters);

        return ShowOverlayAsync(overlay);
    }

    public async Task<ContentDialogResult> ShowLoadingMessageAsync(MessageDialogParameters parameters, Task loadingTask)
    {
        MessageContentDialog overlay = CreateMessageOverlay(parameters);

        Task<ContentDialogResult> overlayTask = ShowOverlayAsync(overlay);

        await Task.WhenAny(
            overlayTask,
            loadingTask);

        if (!overlayTask.IsCompleted)
        {
            overlay.Hide();
        }
        return ContentDialogResult.None;
    }

    public Task<ContentDialogResult> ShowOverlayAsync(OverlayViewModelBase overlayViewModel, object? parameter = null)
    {
        Type overlayType = OverlayViewMapper.GetViewType(overlayViewModel.GetType());

        return ShowOverlayAsync(overlayType, parameter);
    }

    protected Task<ContentDialogResult> ShowOverlayAsync<TViewModel>(object? parameter = null)
            where TViewModel : OverlayViewModelBase
    {
        Type overlayType = OverlayViewMapper.GetViewType<TViewModel>();

        return ShowOverlayAsync(overlayType, parameter);
    }

    protected override void OnWindowLoaded()
    {
        base.OnWindowLoaded();        
        
        // Host ready, release the semaphore to allow showing overlays
        _overlaySemaphore.Release();
    }

    protected override void RegisterToHostEvents()
    {
        base.RegisterToHostEvents();

        if (Host != null)
        {
            Host.Closed += OnWindowClosed;
        }
    }

    protected override void UnregisterFromHostEvents()
    {
        base.UnregisterFromHostEvents();

        if (Host != null)
        {
            Host.Closed -= OnWindowClosed;
        }
    }

    protected override void OnFlowDirectionChanged()
    {
        base.OnFlowDirectionChanged();

        if (_currentOverlay != null)
        {
            _currentOverlay.FlowDirection = CurrentFlowDirection;
        }
    }

    protected override void OnAppThemeChanged()
    {
        base.OnAppThemeChanged();

        if (_currentOverlay != null)
        {
            _currentOverlay.RequestedTheme = CurrentAppTheme;
        }
    }

    private void OnWindowClosed(object sender, WindowEventArgs e)
    {
        if (!e.Handled)
        {
            Reset();
        }
    }

    private async Task<ContentDialogResult> ShowOverlayAsync(Type overlayType, object? parameter)
    {
        ContentDialog? overlay = Activator.CreateInstance(overlayType) as ContentDialog;
        if (overlay == null)
        {
            Logger.Error<AppLog>($"Type {overlayType} is not recognized as a ContentDialog.");
            return ContentDialogResult.None;
        }

        return await ShowOverlayAsync(overlay, parameter);
    }

    private async Task<ContentDialogResult> ShowOverlayAsync(ContentDialog overlay, object? parameter = null)
    {        
        // Close current overlayViewModel if any, then register the new one. Cannot have two overlays at the same time
        CloseCurrentOverlay();

        await _overlaySemaphore.WaitAsync();

        try
        {
            RegisterOverlay(overlay);

            // Activate host window to bring it to foreground before showing the overlayViewModel
            Host?.Activate();

            IOverlayActivationAware? context = GetCurrentOverlayContext();
            context?.OnShow(parameter);

            ContentDialogResult result = await overlay.ShowAsync();

            return result;
        }
        catch (Exception e)
        {
            Logger.Error<AppLog>($"Error when trying to show message '{overlay}'", e);

            return ContentDialogResult.None;
        }
        finally
        {
            UnregisterCurrentOverlay();

            _overlaySemaphore.Release();
        }
    }

    private void RegisterOverlay(ContentDialog overlay)
    {
        if (Host == null)
        {
            throw new InvalidOperationException("Host window has not been initialized.");
        }

        overlay.XamlRoot = Host.GetXamlRoot();
        overlay.RequestedTheme = CurrentAppTheme;
        overlay.FlowDirection = CurrentFlowDirection;

        _currentOverlay = overlay;

        OverlayChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UnregisterCurrentOverlay()
    {
        _currentOverlay = null;

        OverlayChanged?.Invoke(this, EventArgs.Empty);
    }

    private MessageContentDialog CreateMessageOverlay(MessageDialogParameters parameters)
    {
        MessageContentDialog overlay = new()
        {
            Title = parameters.Title,
            Content = GetMessageContent(parameters),
            IsLoading = parameters.ShowLoadingAnimation,
            IsVerticalLayout = parameters.UseVerticalLayoutForButtons,
            PrimaryButtonText = parameters.PrimaryButtonText,
            SecondaryButtonText = parameters.SecondaryButtonText,
            CloseButtonText = parameters.CloseButtonText
        };

        return overlay;
    }

    private object GetMessageContent(MessageDialogParameters parameters)
    {
        return string.IsNullOrEmpty(parameters.Message)
            ? string.Empty
            : parameters.MessageType switch
            {
                DialogMessageType.RichText => new RichTextBlock
                {
                    Blocks =
                    {
                        RichTextHelper.ParseRichText(parameters.Message, parameters.TrailingInlineButton)
                    }
                },
                _ => parameters.Message,
            };
    }
}