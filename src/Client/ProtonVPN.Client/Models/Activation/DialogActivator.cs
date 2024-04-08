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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Models.Activation;

public class DialogActivator : WindowActivatorBase, IDialogActivator
{
    private readonly IViewMapper _viewMapper;
    private readonly IOverlayActivator _overlayActivator;
    private readonly ISettings _settings;

    private bool _handleClosedEvents = true;

    private List<ActiveDialog> _activeDialogs = new();

    public DialogActivator(ILogger logger, IViewMapper viewMapper, IThemeSelector themeSelector, IOverlayActivator overlayActivator, ISettings settings)
        : base(logger, themeSelector)
    {
        _viewMapper = viewMapper;
        _overlayActivator = overlayActivator;
        _settings = settings;
    }

    public void ShowDialog<TPageViewModel>()
        where TPageViewModel : PageViewModelBase
    {
        Type dialogType = _viewMapper.GetDialogType<TPageViewModel>();

        ShowDialog(dialogType);
    }

    public void CloseDialog<TPageViewModel>()
        where TPageViewModel : PageViewModelBase
    {
        Type dialogType = _viewMapper.GetDialogType<TPageViewModel>();

        CloseDialog(dialogType);
    }

    public void CloseAllDialogs()
    {
        try
        {
            _handleClosedEvents = false;

            foreach (ActiveDialog dialog in _activeDialogs.ToList())
            {
                dialog.Close();
            }
        }
        finally
        {
            _handleClosedEvents = true;
        }
    }

    public void HideAllDialogs()
    {
        foreach (ActiveDialog dialog in _activeDialogs.Where(d => d.Status is WindowStatus.Active).ToList())
        {
            dialog.Hide();
        }
    }

    public void ActivateAllDialogs()
    {
        foreach (ActiveDialog dialog in _activeDialogs.Where(d => d.Status is not WindowStatus.Closed).ToList())
        {
            dialog.Activate();
        }
    }

    protected override void OnThemeChanged(ElementTheme theme)
    {
        foreach (ActiveDialog dialog in _activeDialogs.ToList())
        {
            dialog.Window.ApplyTheme(theme);
        }
    }

    protected override void OnLanguageChanged(string language)
    {
        foreach (ActiveDialog dialog in _activeDialogs.ToList())
        {
            dialog.Window.ApplyFlowDirection(language);
        }
    }

    private ActiveDialog? GetDialog(Type dialogType)
    {
        return _activeDialogs.FirstOrDefault(d => d.Window.GetType() == dialogType);
    }

    private void ShowDialog(Type dialogType)
    {
        try
        {
            ActiveDialog? dialog = GetDialog(dialogType);
            if (dialog == null)
            {
                Window window = Activator.CreateInstance(dialogType) as Window
                    ?? throw new InvalidCastException($"Type {dialogType} is not recognized as a Window.");

                window.ApplyTheme(ThemeSelector.GetTheme().Theme);
                window.ApplyFlowDirection(_settings.Language);
                window.CenterOnScreen();

                dialog = new(window);

                RegisterDialog(dialog);

                dialog.Show();
            }

            dialog.Activate();
        }
        catch (Exception e)
        {
            Logger.Error<AppLog>($"Error when trying to show dialog '{dialogType}'", e);
            throw;
        }
    }

    private void CloseDialog(Type dialogType)
    {
        try
        {
            ActiveDialog? dialog = GetDialog(dialogType);
            if (dialog == null)
            {
                Logger.Info<AppLog>($"Dialog '{dialogType}' cannot be closed as it is not currently active");
                return;
            }

            dialog.FakeClose();
        }
        catch (Exception e)
        {
            Logger.Error<AppLog>($"Error when trying to close dialog '{dialogType}'", e);
            throw;
        }
    }

    private void RegisterDialog(ActiveDialog dialog)
    {
        dialog.Window.Closed += OnDialogClosed;

        _activeDialogs.Add(dialog);
    }

    private void UnregisterDialog(ActiveDialog dialog)
    {
        dialog.Window.Closed -= OnDialogClosed;

        _activeDialogs.Remove(dialog);
    }

    private void OnDialogClosed(object sender, WindowEventArgs args)
    {
        if (sender is Window window)
        {
            ActiveDialog? dialog = _activeDialogs.FirstOrDefault(d => d.Window == window);
            if (dialog != null)
            {
                if (_handleClosedEvents)
                {
                    // Hide window instead
                    args.Handled = true;
                    dialog.FakeClose();
                    return;
                }

                UnregisterDialog(dialog);
            }
        }
    }
}
