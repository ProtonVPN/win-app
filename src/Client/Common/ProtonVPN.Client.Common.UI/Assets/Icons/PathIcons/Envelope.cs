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

public class Envelope : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M2 6.217V10.8c0 .577 0 .949.024 1.232.022.272.06.373.085.422a1 1 0 0 0 .437.437c.05.025.15.063.422.085C3.25 13 3.623 13 4.2 13h7.6c.577 0 .949 0 1.232-.024.272-.022.373-.06.422-.085a1 1 0 0 0 .437-.437c.025-.05.063-.15.085-.422C14 11.75 14 11.377 14 10.8V6.217L8.786 9.426a1.5 1.5 0 0 1-1.572 0L2 6.217Zm11.982-1.163-5.72 3.52a.5.5 0 0 1-.524 0l-5.72-3.52.006-.086c.022-.272.06-.373.085-.422a1 1 0 0 1 .437-.437c.05-.025.15-.063.422-.085C3.25 4 3.623 4 4.2 4h7.6c.577 0 .949 0 1.232.024.272.022.373.06.422.085a1 1 0 0 1 .437.437c.025.05.063.15.085.422l.006.086ZM1.218 4.092C1 4.52 1 5.08 1 6.2v4.6c0 1.12 0 1.68.218 2.108a2 2 0 0 0 .874.874C2.52 14 3.08 14 4.2 14h7.6c1.12 0 1.68 0 2.108-.218a2 2 0 0 0 .874-.874C15 12.48 15 11.92 15 10.8V6.2c0-1.12 0-1.68-.218-2.108a2 2 0 0 0-.874-.874C13.48 3 12.92 3 11.8 3H4.2c-1.12 0-1.68 0-2.108.218a2 2 0 0 0-.874.874Z";
}