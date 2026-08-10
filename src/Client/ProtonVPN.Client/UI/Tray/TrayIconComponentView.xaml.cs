/*
 * Copyright (c) 2026 Proton AG
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

using ProtonVPN.Client.Core.Bases;

namespace ProtonVPN.Client.UI.Tray;

public sealed partial class TrayIconComponentView : IContextAware
{
    public TrayIconComponentViewModel ViewModel { get; }

    public TrayIconComponentView()
    {
        ViewModel = App.GetService<TrayIconComponentViewModel>();

        InitializeComponent();
    }

    public void DisposeTrayIcon()
    {
        // Messages received while the app is shutting down keep invalidating the icon source.
        // H.NotifyIcon applies those updates asynchronously (async void), so a late update would
        // throw ObjectDisposedException on the dispatcher. Detach the bindings before disposing.
        Bindings.StopTracking();

        TrayIcon.Dispose();
    }

    public object GetContext()
    {
        return ViewModel;
    }
}