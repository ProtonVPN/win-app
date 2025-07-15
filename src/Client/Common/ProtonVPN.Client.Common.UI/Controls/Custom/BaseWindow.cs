/*
 * Copyright (c) 2025 Proton AG
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
using Microsoft.UI.Xaml;
using WinUIEx;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class BaseWindow : WindowEx
{
    public bool IsLoaded { get; private set; }

    public BaseWindow()
    {
        Activated += OnActivated;
    }

    public event EventHandler? Loaded;

    protected virtual void OnActivated(object sender, WindowActivatedEventArgs e)
    {
        if (!IsLoaded && Content is FrameworkElement rootElement)
        {
            rootElement.Loaded -= OnRootLoaded;
            rootElement.Loaded += OnRootLoaded;
        }
    }

    protected virtual void OnLoaded()
    { }

    private void OnRootLoaded(object sender, RoutedEventArgs e)
    {
        IsLoaded = true;

        OnLoaded();

        Loaded?.Invoke(this, EventArgs.Empty);

        if (sender is FrameworkElement rootElement)
        {
            rootElement.Loaded -= OnRootLoaded;
        }
    }
}