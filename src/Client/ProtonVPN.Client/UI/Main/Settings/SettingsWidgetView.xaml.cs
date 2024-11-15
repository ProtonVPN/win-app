/*
 * Copyright (c) 2024 Proton AG
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
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Client.Core.Bases;

namespace ProtonVPN.Client.UI.Main.Settings;

public sealed partial class SettingsWidgetView : IContextAware
{
    private const int COMMA_KEY = 188;

    public SettingsWidgetViewModel ViewModel { get; }

    public SettingsWidgetView()
    {
        ViewModel = App.GetService<SettingsWidgetViewModel>();

        InitializeComponent();

        Loaded += OnSettingsWidgetLoaded;
        Unloaded += OnSettingsWidgetUnloaded;
    }

    private void OnSettingsWidgetLoaded(object sender, RoutedEventArgs e)
    {
        SettingsButton.KeyboardAccelerators.Add(
            new KeyboardAccelerator()
            {
                Modifiers = Windows.System.VirtualKeyModifiers.Control,
                Key = (Windows.System.VirtualKey)COMMA_KEY
            });
    }

    private void OnSettingsWidgetUnloaded(object sender, RoutedEventArgs e)
    {
        SettingsButton.KeyboardAccelerators.Clear();
    }

    public object GetContext()
    {
        return ViewModel;
    }
}