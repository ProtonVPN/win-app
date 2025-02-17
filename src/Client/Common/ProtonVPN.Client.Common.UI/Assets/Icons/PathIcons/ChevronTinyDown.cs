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

public class ChevronTinyDown : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M4.14645 5.89645C4.34171 5.70118 4.65829 5.70118 4.85355 5.89645L8 9.04289L11.1464 5.89645C11.3417 5.70118 11.6583 5.70118 11.8536 5.89645C12.0488 6.09171 12.0488 6.40829 11.8536 6.60355L8.35355 10.1036C8.25979 10.1973 8.13261 10.25 8 10.25C7.86739 10.25 7.74021 10.1973 7.64645 10.1036L4.14645 6.60355C3.95118 6.40829 3.95118 6.09171 4.14645 5.89645Z";

    protected override string IconGeometry20 { get; }
        = "M5.18306 7.18306C5.42714 6.93898 5.82286 6.93898 6.06694 7.18306L10 11.1161L13.9331 7.18306C14.1771 6.93898 14.5729 6.93898 14.8169 7.18306C15.061 7.42714 15.061 7.82286 14.8169 8.06694L10.4419 12.4419C10.3247 12.5592 10.1658 12.625 10 12.625C9.83424 12.625 9.67527 12.5592 9.55806 12.4419L5.18306 8.06694C4.93898 7.82286 4.93898 7.42714 5.18306 7.18306Z";

    protected override string IconGeometry24 { get; }
        = "M6.21967 9.21967C6.51256 8.92678 6.98744 8.92678 7.28033 9.21967L12 13.9393L16.7197 9.21967C17.0126 8.92678 17.4874 8.92678 17.7803 9.21967C18.0732 9.51256 18.0732 9.98744 17.7803 10.2803L12.5303 15.5303C12.3897 15.671 12.1989 15.75 12 15.75C11.8011 15.75 11.6103 15.671 11.4697 15.5303L6.21967 10.2803C5.92678 9.98744 5.92678 9.51256 6.21967 9.21967Z";

    protected override string IconGeometry32 { get; }
        = "M8.29289 12.2929C8.68342 11.9024 9.31658 11.9024 9.70711 12.2929L16 18.5858L22.2929 12.2929C22.6834 11.9024 23.3166 11.9024 23.7071 12.2929C24.0976 12.6834 24.0976 13.3166 23.7071 13.7071L16.7071 20.7071C16.5196 20.8946 16.2652 21 16 21C15.7348 21 15.4804 20.8946 15.2929 20.7071L8.29289 13.7071C7.90237 13.3166 7.90237 12.6834 8.29289 12.2929Z";
}