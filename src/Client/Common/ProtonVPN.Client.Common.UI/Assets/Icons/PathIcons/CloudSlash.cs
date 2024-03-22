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

public class CloudSlash : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2.647 2.945a.5.5 0 1 1 .707-.707l1.43 1.431A3.998 3.998 0 0 1 10.464 5H11a4 4 0 0 1 2.35 7.239l.408.409a.5.5 0 0 1-.707.707L2.647 2.944Zm9.983 8.574A3 3 0 0 0 11 6H9.888L9.6 5.5a2.998 2.998 0 0 0-4.09-1.104l7.12 7.124Z m4.204 5.911-.755-.755A3.983 3.983 0 0 0 3 7v.17A3.001 3.001 0 0 0 4 13h7c.094 0 .187-.003.28-.01l-.99-.99H4a2 2 0 0 1-.667-3.886L4 7.878V7c0-.384.072-.751.204-1.089Z";

    protected override string IconGeometry20 { get; }
        = "M3.309 3.682a.625.625 0 1 1 .884-.884L5.98 4.586a4.998 4.998 0 0 1 7.1 1.664h.67a5 5 0 0 1 2.937 9.047l.51.511a.625.625 0 1 1-.884.884L3.31 3.682Zm12.48 10.716A3.75 3.75 0 0 0 13.75 7.5h-1.39l-.362-.624a3.748 3.748 0 0 0-5.111-1.382l8.9 8.904Z m5.255 7.389-.943-.943A4.979 4.979 0 0 0 3.75 8.75v.213A3.752 3.752 0 0 0 5 16.25h8.75c.118 0 .234-.004.35-.012L12.862 15H5a2.5 2.5 0 0 1-.833-4.858L5 9.847V8.75c0-.48.09-.94.255-1.361Z"; 

    protected override string IconGeometry24 { get; }
        = "M3.97 4.418a.75.75 0 0 1 1.061-1.06l2.146 2.146A5.972 5.972 0 0 1 10.5 4.5c2.22 0 4.16 1.207 5.197 3h.803a6 6 0 0 1 3.524 10.857l.613.613a.75.75 0 0 1-1.061 1.06L3.97 4.418Zm14.976 12.86A4.5 4.5 0 0 0 16.5 9h-1.668l-.433-.749a4.498 4.498 0 0 0-6.134-1.658l10.68 10.685Z M6.306 8.867 5.174 7.735A5.974 5.974 0 0 0 4.5 10.5v.256A4.502 4.502 0 0 0 6 19.5h10.5c.141 0 .281-.005.42-.015L15.435 18H6a3 3 0 0 1-1-5.83l1-.353V10.5c0-.576.108-1.127.306-1.633Z"; 

    protected override string IconGeometry32 { get; }
        = "M5.294 5.89a1 1 0 0 1 1.414-1.413l2.86 2.861A7.963 7.963 0 0 1 14 6a7.997 7.997 0 0 1 6.93 4H22a8 8 0 0 1 4.699 14.476l.817.817a1 1 0 0 1-1.415 1.414L5.294 5.891ZM25.26 23.038A6 6 0 0 0 22 12h-2.224l-.578-.999A5.997 5.997 0 0 0 14 8a5.973 5.973 0 0 0-2.98.79l14.241 14.247Z M8.407 11.822 6.9 10.312A7.966 7.966 0 0 0 6 14v.341A6 6 0 0 0 8 26h14c.188 0 .375-.006.56-.02L20.58 24H8a4 4 0 0 1-1.334-7.773L8 15.756V14c0-.768.144-1.503.407-2.178Z";
}