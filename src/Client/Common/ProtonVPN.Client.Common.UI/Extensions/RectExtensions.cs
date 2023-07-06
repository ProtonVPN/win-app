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

using Windows.Foundation;

namespace ProtonVPN.Client.Common.UI.Extensions;

public static class RectExtensions
{
    public static bool Contains(this Rect rect1, Rect rect2)
    {
        return rect1.Contains(new Point(rect2.Left, rect2.Top))
            && rect1.Contains(new Point(rect2.Left, rect2.Bottom))
            && rect1.Contains(new Point(rect2.Right, rect2.Bottom))
            && rect1.Contains(new Point(rect2.Right, rect2.Top));
    }
}