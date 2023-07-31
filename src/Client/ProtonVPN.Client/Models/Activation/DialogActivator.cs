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
using ProtonVPN.Client.Contracts;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Models.Activation;

public class DialogActivator : IDialogActivator, IEventMessageReceiver<ThemeChangedMessage>, IEventMessageReceiver<MainWindowClosedMessage>
{
    private readonly ILogger _logger;
    private readonly IViewMapper _viewMapper;
    private readonly IThemeSelector _themeSelector;

    private List<Window> _activeDialogs = new();

    public DialogActivator(ILogger logger, IViewMapper viewMapper, IThemeSelector themeSelector)
    {
        _logger = logger;
        _viewMapper = viewMapper;
        _themeSelector = themeSelector;
    }

    public void ShowDialog<TPageViewModel>()
        where TPageViewModel : PageViewModelBase
    {
        Type dialogType = _viewMapper.GetDialogType<TPageViewModel>();

        Type pageType = _viewMapper.GetPageType<TPageViewModel>();

        ShowDialog(dialogType, pageType);
    }

    public void ShowDialog(string shellKey)
    {
        Type dialogType = _viewMapper.GetDialogType(shellKey);

        Type pageType = _viewMapper.GetPageType(shellKey);

        ShowDialog(dialogType, pageType);
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

    public void Receive(ThemeChangedMessage message)
    {
        ElementTheme theme = _themeSelector.GetTheme().Theme;

        foreach (Window dialog in _activeDialogs.ToList())
        {
            if (dialog.Content is FrameworkElement element)
            {
                element.RequestedTheme = theme;
            }
        }
    }

    public void Receive(MainWindowClosedMessage message)
    {
        foreach (Window dialog in _activeDialogs.ToList())
        {
            dialog.Close();
        }
    }

    private void ShowDialog(Type dialogType, Type pageType)
    {
        try
        {
            Window? dialog = _activeDialogs.FirstOrDefault(d => d.GetType() == dialogType);
            if (dialog == null)
            {
                dialog = Activator.CreateInstance(dialogType) as Window
                    ?? throw new InvalidCastException($"Type {dialogType} is not recognized as a Window.");

                dialog.Closed += OnDialogClosed;
                dialog.Activated += OnDialogActivated;

                Page page = Activator.CreateInstance(pageType) as Page
                        ?? throw new InvalidCastException($"Type {pageType} is not recognized as a Page.");

                dialog.Content = page;

                if (page is IShellPage shellPage)
                {
                    shellPage.Initialize(dialog);
                }

                _activeDialogs.Add(dialog);
            }

            dialog.Activate();
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>($"Error when trying to show dialog '{dialogType}' and page '{pageType}'", e);
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
                _logger.Info<AppLog>($"Dialog '{dialogType}' is not currently active");
                return;
            }

            dialog.Close();
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>($"Error when trying to close dialog '{dialogType}'", e);
            throw;
        }
    }

    private void OnDialogActivated(object sender, WindowActivatedEventArgs args)
    {
        if (sender is Window dialog)
        {
            dialog.Activated -= OnDialogActivated;

            if (dialog.Content is FrameworkElement element)
            {
                element.RequestedTheme = _themeSelector.GetTheme().Theme;
            }
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