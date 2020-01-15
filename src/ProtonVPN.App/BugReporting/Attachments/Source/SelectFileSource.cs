/*
 * Copyright (c) 2020 Proton Technologies AG
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

using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProtonVPN.BugReporting.Attachments.Source
{
    internal class SelectFileSource : IEnumerable<string>
    {
        public IEnumerator<string> GetEnumerator()
        {
            return FileNames().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<string> FileNames()
        {
            var dialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "All files (*.*)|*.*",
                Multiselect = true
            };

            return dialog.ShowDialog() == true ? dialog.FileNames : Enumerable.Empty<string>();
        }
    }
}
