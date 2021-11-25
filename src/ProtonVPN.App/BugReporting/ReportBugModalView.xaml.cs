/*
 * Copyright (c) 2021 Proton Technologies AG
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

using WpfScreenHelper;

namespace ProtonVPN.BugReporting
{
    public partial class ReportBugModalView
    {
        public ReportBugModalView()
        {
            InitializeComponent();
            SizeChanged += ReportBugModalView_SizeChanged;
        }

        private void ReportBugModalView_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (e.PreviousSize == e.NewSize)
            {
                return;
            }

            Screen screen = Screen.FromWindow(this);
            if (screen != null)
            {
                Top = screen.WorkingArea.Top + (screen.WorkingArea.Height - ActualHeight) / 2;
            }
        }
    }
}