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
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Models.Activation;

public class DialogActivator : WindowActivatorBase, IDialogActivator
{
    private readonly IViewMapper _viewMapper;

    private List<Window> _activeDialogs = new();

    public DialogActivator(ILogger logger, IViewMapper viewMapper, IThemeSelector themeSelector)
        : base(logger, themeSelector)
    {
        _viewMapper = viewMapper;
    }

    public void ShowDialog<TPageViewModel>()
        where TPageViewModel : PageViewModelBase
    {
        Type dialogType = _viewMapper.GetDialogType<TPageViewModel>();

        ShowDialog(dialogType);
    }

    public void ShowDialog(string shellKey)
    {
        Type dialogType = _viewMapper.GetDialogType(shellKey);

        ShowDialog(dialogType);
    }

    public void CloseDialog<TPageViewModel>()
        where TPageViewModel : PageViewModelBase
    {
        Type dialogType = _viewMapper.GetDialogType<TPageViewModel>();

        CloseDialog(dialogType);
    }

    public void CloseDialog(string shellKey)
    {
        Type dialogType = _viewMapper.GetDialogType(shellKey);

        CloseDialog(dialogType);
    }

    public void CloseAll()
    {
        foreach (Window dialog in _activeDialogs.ToList())
        {
            dialog.Close();
        }
    }

    protected override void OnThemeChanged(ElementTheme theme)
    {
        foreach (Window dialog in _activeDialogs.ToList())
        {
            dialog.ApplyTheme(theme);
        }
    }

    private void ShowDialog(Type dialogType)
    {
        try
        {
            Window? dialog = _activeDialogs.FirstOrDefault(d => d.GetType() == dialogType);
            if (dialog == null)
            {
                dialog = Activator.CreateInstance(dialogType) as Window
                    ?? throw new InvalidCastException($"Type {dialogType} is not recognized as a Window.");

                dialog.Closed += OnDialogClosed;

                dialog.ApplyTheme(ThemeSelector.GetTheme().Theme);
                dialog.CenterOnScreen();

                _activeDialogs.Add(dialog);
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
            Window? dialog = _activeDialogs.FirstOrDefault(d => d.GetType() == dialogType);
            if (dialog == null)
            {
                Logger.Info<AppLog>($"Dialog '{dialogType}' is not currently active");
                return;
            }

            dialog.Close();
        }
        catch (Exception e)
        {
            Logger.Error<AppLog>($"Error when trying to close dialog '{dialogType}'", e);
            throw;
        }
    }

    private void OnDialogClosed(object sender, WindowEventArgs args)
    {
        if (sender is Window dialog)
        {
            dialog.Closed -= OnDialogClosed;

            _activeDialogs.Remove(dialog);
        }
    }
}