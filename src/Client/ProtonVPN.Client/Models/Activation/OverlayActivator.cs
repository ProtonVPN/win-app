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
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Models.Activation;

public class OverlayActivator : WindowActivatorBase, IOverlayActivator
{
    private readonly IViewMapper _viewMapper;

    private MainWindow? _mainWindow;

    private List<ActiveOverlay> _activeOverlays = new();

    public OverlayActivator(ILogger logger, IViewMapper viewMapper, IThemeSelector themeSelector)
        : base(logger, themeSelector)
    {
        _viewMapper = viewMapper;
    }

    public void Initialize(MainWindow window)
    {
        if (_mainWindow != null)
        {
            Logger.Error<AppLog>("Overlay Activator has already been initialized.");
            return;
        }

        _mainWindow = window;
    }

    public async Task<ContentDialogResult> ShowMessageAsync(MessageDialogParameters parameters, Window? rootWindow = null)
    {
        try
        {
            rootWindow ??= _mainWindow ?? throw new InvalidOperationException("Cannot show message overlay, root window is undefined.");

            ContentDialog dialog = new()
            {
                Title = parameters.Title,
                Content = parameters.Message,
                PrimaryButtonText = parameters.PrimaryButtonText,
                SecondaryButtonText = parameters.SecondaryButtonText,
                CloseButtonText = parameters.CloseButtonText,
                XamlRoot = rootWindow.GetXamlRoot(),
                RequestedTheme = ThemeSelector.GetTheme().Theme,
                Style = parameters.UseVerticalLayoutForButtons
                    ? ResourceHelper.GetContentDialogStyle("VerticalMessageContentDialogStyle")
                    : ResourceHelper.GetContentDialogStyle("MessageContentDialogStyle")
            };

            ActiveOverlay overlay = new(rootWindow, dialog);

            RegisterOverlay(overlay);

            ContentDialogResult result = await overlay.Dialog.ShowAsync();

            return result;
        }
        catch (Exception e)
        {
            Logger.Error<AppLog>($"Error when trying to show message '{parameters.Title}'", e);
            throw;
        }
    }

    public Task ShowOverlayAsync<TOverlayViewModel>(Window? rootWindow = null)
        where TOverlayViewModel : OverlayViewModelBase
    {
        Type overlayType = _viewMapper.GetOverlayType<TOverlayViewModel>();

        return ShowOverlayAsync(overlayType, rootWindow);
    }

    public Task ShowOverlayAsync(string overlayKey, Window? rootWindow = null)
    {
        Type overlayType = _viewMapper.GetOverlayType(overlayKey);

        return ShowOverlayAsync(overlayType, rootWindow);
    }

    public void CloseOverlay<TOverlayViewModel>(Window? rootWindow = null)
        where TOverlayViewModel : OverlayViewModelBase
    {
        Type overlayType = _viewMapper.GetOverlayType<TOverlayViewModel>();

        CloseOverlay(overlayType, rootWindow);
    }

    public void CloseOverlay(string overlayKey, Window? rootWindow = null)
    {
        Type overlayType = _viewMapper.GetOverlayType(overlayKey);

        CloseOverlay(overlayType, rootWindow);
    }

    public void CloseAllOverlays(Window? rootWindow = null)
    {
        rootWindow ??= _mainWindow ?? throw new InvalidOperationException("Cannot close overlay, root window is undefined.");

        foreach (ContentDialog overlay in _activeOverlays.Where(o => o.BelongsTo(rootWindow)).Select(o => o.Dialog).ToList())
        {
            overlay.Hide();
        }
    }

    protected override void OnThemeChanged(ElementTheme theme)
    {
        foreach (ContentDialog overlay in _activeOverlays.Select(o => o.Dialog).ToList())
        {
            overlay.RequestedTheme = theme;
        }
    }

    private async Task ShowOverlayAsync(Type overlayType, Window? rootWindow)
    {
        try
        {
            rootWindow ??= _mainWindow ?? throw new InvalidOperationException("Cannot show overlay, root window is undefined.");

            ActiveOverlay? overlay = _activeOverlays.FirstOrDefault(o => o.BelongsTo(rootWindow) && o.IsOfType(overlayType));
            if (overlay == null)
            {
                ContentDialog dialog = Activator.CreateInstance(overlayType) as ContentDialog
                    ?? throw new InvalidCastException($"Type {overlayType} is not recognized as a ContentDialog.");

                dialog.XamlRoot = rootWindow.GetXamlRoot();
                dialog.RequestedTheme = ThemeSelector.GetTheme().Theme;

                overlay = new(rootWindow, dialog);

                RegisterOverlay(overlay);
            }

            await overlay.Dialog.ShowAsync();
        }
        catch (Exception e)
        {
            Logger.Error<AppLog>($"Error when trying to show overlay for '{overlayType}'", e);
            throw;
        }
    }

    private void CloseOverlay(Type overlayType, Window? rootWindow)
    {
        try
        {
            rootWindow ??= _mainWindow ?? throw new InvalidOperationException("Cannot close overlay, root window is undefined.");

            ActiveOverlay? overlay = _activeOverlays.FirstOrDefault(o => o.BelongsTo(rootWindow) && o.IsOfType(overlayType));
            if (overlay == null)
            {
                Logger.Info<AppLog>($"Overlay '{overlayType}' is not currently active");
                return;
            }

            overlay.Dialog.Hide();
        }
        catch (Exception e)
        {
            Logger.Error<AppLog>($"Error when trying to close overlay '{overlayType}'", e);
            throw;
        }
    }

    private void RegisterOverlay(ActiveOverlay overlay)
    {
        overlay.Dialog.Closed += OnOverlayClosed;

        _activeOverlays.Add(overlay);
    }

    private void UnregisterOverlay(ActiveOverlay? overlay)
    {
        if (overlay is not null)
        {
            overlay.Dialog.Closed -= OnOverlayClosed;

            _activeOverlays.Remove(overlay);
        }
    }

    private void OnOverlayClosed(ContentDialog overlay, ContentDialogClosedEventArgs args)
    {
        if (overlay is not null)
        {
            UnregisterOverlay(_activeOverlays.FirstOrDefault(o => o.Dialog == overlay));
        }
    }
}