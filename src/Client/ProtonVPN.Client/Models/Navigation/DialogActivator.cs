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

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Models.Parameters;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Dialogs.Overlays;

namespace ProtonVPN.Client.Models.Navigation;

public class DialogActivator : IDialogActivator
{
    private readonly IThemeSelector _themeSelector;

    private readonly Dictionary<string, Type> _dialogs = new();
    private ContentDialog? _dialog;

    private readonly SemaphoreSlim _dialogSemaphore = new(1);

    public DialogActivator(IThemeSelector themeSelector)
    {
        _themeSelector = themeSelector;

        Configure<LatencyOverlayViewModel, LatencyOverlayDialog>();
        Configure<ProtocolOverlayViewModel, ProtocolOverlayDialog>();
        Configure<ServerLoadOverlayViewModel, ServerLoadOverlayDialog>();
    }

    public async Task ShowAsync(string dialogKey)
    {
        try
        {
            await _dialogSemaphore.WaitAsync();

            Type dialogType = GetDialogType(dialogKey);

            _dialog = Activator.CreateInstance(dialogType) as ContentDialog;
            if (_dialog != null)
            {
                _dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
                _dialog.RequestedTheme = _themeSelector.GetTheme().Theme;

                await _dialog.ShowAsync();
            }
        }
        finally
        {
            _dialog = null;
            _dialogSemaphore.Release();
        }
    }

    public async Task<ContentDialogResult> ShowMessageAsync(MessageDialogParameters parameters)
    {
        try
        {
            await _dialogSemaphore.WaitAsync();

            _dialog = new ContentDialog
            {
                Title = parameters.Title,
                Content = parameters.Message,
                PrimaryButtonText = parameters.PrimaryButtonText,
                SecondaryButtonText = parameters.SecondaryButtonText,
                CloseButtonText = parameters.CloseButtonText,
                XamlRoot = App.MainWindow.Content.XamlRoot,
                RequestedTheme = _themeSelector.GetTheme().Theme,
            };

            ContentDialogResult result = await _dialog.ShowAsync();

            return result;
        }
        finally
        {
            _dialog = null;
            _dialogSemaphore.Release();
        }
    }

    public void Close()
    {
        _dialog?.Hide();
    }

    private Type GetDialogType(string key)
    {
        Type? dialogType;
        lock (_dialogs)
        {
            if (!_dialogs.TryGetValue(key, out dialogType))
            {
                throw new ArgumentException($"Dialog not found: {key}. Did you forget to call DialogActivator.Configure?");
            }
        }

        return dialogType;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : ContentDialog
    {
        lock (_dialogs)
        {
            string key = typeof(VM).FullName!;
            if (_dialogs.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in DialogActivator");
            }

            Type type = typeof(V);
            if (_dialogs.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_dialogs.First(p => p.Value == type).Key}");
            }

            _dialogs.Add(key, type);
        }
    }
}