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

namespace ProtonVPN.Client.Legacy.Models;

public class ActiveDialog
{
    public Window Window { get; }
    public WindowStatus Status { get; set; }

    public ActiveDialog(Window window)
    {
        Window = window;
        Status = WindowStatus.Active;
    }

    public void Show()
    {
        Window.Show();
        Status = WindowStatus.Active;
    }

    public void Activate()
    {
        Window.Activate();
        Status = WindowStatus.Active;
    }

    public void Close()
    {
        Window.Close();
        Status = WindowStatus.Closed;
    }

    public void Hide()
    {
        Window.Hide();
        Status = WindowStatus.Hidden;
    }

    public void FakeClose()
    {
        // Dialog will be considered closed but it is actually hidden to keep the instance alive
        Window.Hide();
        Status = WindowStatus.Closed;
    }
}