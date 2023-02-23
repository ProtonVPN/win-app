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
using System.Drawing;
using System.IO;
using System.Windows;

namespace ProtonVPN.Windows
{
    internal class ResourceIcon
    {
        private readonly string _iconPath;
        private const string BasePath = "pack://application:,,,/ProtonVPN;component/";

        private Icon _cachedIcon;
        private Icon _cachedSizedIcon;

        public ResourceIcon(string iconPath)
        {
            _iconPath = iconPath;
        }

        public Icon Value()
        {
            return _cachedIcon ?? (_cachedIcon = new Icon(GetIconStream()));
        }

        public Icon Value(System.Drawing.Size size)
        {
            return _cachedSizedIcon ?? (_cachedSizedIcon = new Icon(GetIconStream(), size));
        }

        public Stream GetIconStream()
        {
            return Application.GetResourceStream(new Uri(Path.Combine(BasePath, _iconPath)))?.Stream;
        }
    }
}
