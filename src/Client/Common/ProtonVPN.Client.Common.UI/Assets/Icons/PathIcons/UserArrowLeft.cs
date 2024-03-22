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

public class UserArrowLeft : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M10 5a2 2 0 1 1-4 0 2 2 0 0 1 4 0Zm1 0a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm-6.697 5.6a1.5 1.5 0 0 1 1.2-.6H9V9H5.503a2.5 2.5 0 0 0-2 1l-1.2 1.6c-.742.989-.036 2.4 1.2 2.4H9v-1H3.503a.5.5 0 0 1-.4-.8l1.2-1.6ZM15 11.5a.5.5 0 0 0-.5-.5h-3.793l1.147-1.146a.5.5 0 0 0-.708-.708l-1.858 1.859a.7.7 0 0 0 0 .99l1.858 1.859a.5.5 0 0 0 .708-.708L10.707 12H14.5a.5.5 0 0 0 .5-.5Z";

    protected override string IconGeometry20 { get; }
        = "M12.5 6.25a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0Zm1.25 0a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0Zm-8.372 7c.355-.472.91-.75 1.5-.75h4.372v-1.25H6.878c-.983 0-1.91.463-2.5 1.25l-1.5 2c-.927 1.236-.045 3 1.5 3h6.872v-1.25H4.378a.625.625 0 0 1-.5-1l1.5-2Zm13.372 1.125a.625.625 0 0 0-.625-.625h-4.741l1.433-1.433a.625.625 0 1 0-.884-.884l-2.323 2.323a.875.875 0 0 0 0 1.238l2.323 2.323a.625.625 0 1 0 .884-.884L13.384 15h4.741c.345 0 .625-.28.625-.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M15 7.5a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm1.5 0a4.5 4.5 0 1 1-9 0 4.5 4.5 0 0 1 9 0ZM6.454 15.9a2.25 2.25 0 0 1 1.8-.9H13.5v-1.5H8.254a3.75 3.75 0 0 0-3 1.5l-1.8 2.4C2.342 18.883 3.4 21 5.254 21H13.5v-1.5H5.254a.75.75 0 0 1-.6-1.2l1.8-2.4ZM22.5 17.25a.75.75 0 0 0-.75-.75h-5.69l1.72-1.72a.75.75 0 1 0-1.06-1.06l-2.788 2.787a1.05 1.05 0 0 0 0 1.485l2.788 2.788a.75.75 0 1 0 1.06-1.06L16.06 18h5.69a.75.75 0 0 0 .75-.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M20 10a4 4 0 1 1-8 0 4 4 0 0 1 8 0Zm2 0a6 6 0 1 1-12 0 6 6 0 0 1 12 0ZM8.605 21.2a3 3 0 0 1 2.4-1.2H18v-2h-6.995a5 5 0 0 0-4 2l-2.4 3.2c-1.483 1.978-.072 4.8 2.4 4.8H18v-2H7.005a1 1 0 0 1-.8-1.6l2.4-3.2ZM30 23a1 1 0 0 0-1-1h-7.586l2.293-2.293a1 1 0 0 0-1.414-1.414l-3.717 3.717a1.4 1.4 0 0 0 0 1.98l3.717 3.717a1 1 0 0 0 1.414-1.414L21.414 24H29a1 1 0 0 0 1-1Z";
}