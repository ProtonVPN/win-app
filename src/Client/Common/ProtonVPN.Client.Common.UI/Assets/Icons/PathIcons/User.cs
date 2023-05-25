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

using ProtonVPN.Client.Common.UI.Assets.Icons.Base;

namespace ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;

public class User : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M10 5a2 2 0 1 1-4 0 2 2 0 0 1 4 0Zm1 0a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm-7.145 5.98A2.092 2.092 0 0 1 5.641 10h4.718c.74 0 1.416.38 1.786.98l.782 1.272c.182.296-.01.748-.467.748H3.54c-.457 0-.649-.452-.467-.748l.782-1.272Zm-.851-.524A3.092 3.092 0 0 1 5.64 9h4.718a3.09 3.09 0 0 1 2.637 1.457l.782 1.271c.615 1-.123 2.272-1.318 2.272H3.54c-1.195 0-1.933-1.272-1.319-2.272l.783-1.271Z";
}