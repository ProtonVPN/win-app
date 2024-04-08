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

public class Bolt : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M5.797 15.957a.5.5 0 0 1-.286-.562L6.881 9H3.5a.5.5 0 0 1-.4-.8l6-8a.5.5 0 0 1 .893.382L9.09 6h3.41a.5.5 0 0 1 .405.793l-6.5 9a.5.5 0 0 1-.608.164ZM11.522 7H8.5a.5.5 0 0 1-.493-.582L8.67 2.44 4.5 8h3a.5.5 0 0 1 .489.605l-1.002 4.674L11.522 7Z";

    protected override string IconGeometry20 { get; }
        = ""; 

    protected override string IconGeometry24 { get; }
        = "M8.64691 22.4117C8.33807 22.2469 8.18431 21.8903 8.27642 21.5526L10.2681 14.25L5.24999 14.25C4.95635 14.25 4.68972 14.0786 4.56775 13.8115C4.44578 13.5444 4.49091 13.2307 4.68322 13.0088L14.4332 1.75877C14.6625 1.49424 15.0442 1.42348 15.3531 1.58828C15.6619 1.75307 15.8157 2.10959 15.7236 2.4473L13.7319 9.74997L18.75 9.74997C19.0436 9.74997 19.3103 9.92133 19.4322 10.1884C19.5542 10.4555 19.5091 10.7693 19.3168 10.9912L9.56676 22.2412C9.3375 22.5057 8.95574 22.5764 8.64691 22.4117ZM17.1075 11.25L12.75 11.25C12.5162 11.25 12.2958 11.141 12.154 10.9552C12.0121 10.7694 11.9649 10.5281 12.0264 10.3026L13.4099 5.22984L6.89246 12.75L11.25 12.75C11.4837 12.75 11.7041 12.859 11.846 13.0447C11.9879 13.2305 12.0351 13.4718 11.9736 13.6973L10.5901 18.7701L17.1075 11.25Z"; 

    protected override string IconGeometry32 { get; }
        = "";
}