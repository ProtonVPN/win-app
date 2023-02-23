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

using System;
using System.Windows.Forms;

namespace ProtonVPN.Windows
{
    public class TrayIconMouse
    {
        private readonly Timer _doubleClickTimer = new Timer();

        private int _milliseconds;
        private bool _isFirstClick = true;
        private bool _isDoubleClick;
        private MouseButtons _lastButton;

        public TrayIconMouse()
        {
            _doubleClickTimer.Tick += doubleClickTimer_Tick;
        }

        public event MouseEventHandler OnMouseEvent;

        public void TrayIconClick(object sender, MouseEventArgs e)
        {
            _lastButton = e.Button;

            if (_isFirstClick)
            {
                _isFirstClick = false;
                _doubleClickTimer.Start();
            }
            else
            {
                if (_milliseconds < SystemInformation.DoubleClickTime)
                {
                    _isDoubleClick = true;
                }
            }
        }

        private void doubleClickTimer_Tick(object sender, EventArgs e)
        {
            _milliseconds += 600;

            if (_milliseconds >= SystemInformation.DoubleClickTime)
            {
                _doubleClickTimer.Stop();

                if (_isDoubleClick)
                {
                    OnMouseEvent?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 0));
                }
                else
                {
                    if ((_lastButton & MouseButtons.Left) != 0)
                    {
                        OnMouseEvent?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    }

                    if ((_lastButton & MouseButtons.Right) != 0)
                    {
                        OnMouseEvent?.Invoke(this, new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0));
                    }
                }

                _isFirstClick = true;
                _isDoubleClick = false;
                _milliseconds = 0;
            }
        }
    }
}
