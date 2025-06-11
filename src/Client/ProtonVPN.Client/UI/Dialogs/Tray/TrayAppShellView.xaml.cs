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

using Microsoft.UI.Xaml;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Services.Navigation;

namespace ProtonVPN.Client.UI.Dialogs.Tray;

public sealed partial class TrayAppShellView : IContextAware
{
    public TrayAppShellViewModel ViewModel { get; }

    public TrayAppViewNavigator Navigator { get; }

    public TrayAppShellView()
    {
        ViewModel = App.GetService<TrayAppShellViewModel>();
        Navigator = App.GetService<TrayAppViewNavigator>();

        InitializeComponent();

        Navigator.Initialize(TrayAppNavigationFrame);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Navigator.Load();
        ViewModel.Activate();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
        Navigator.Unload();

        Navigator.Reset();
    }
}