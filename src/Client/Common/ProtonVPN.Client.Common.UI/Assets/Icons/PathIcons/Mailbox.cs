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

public class Mailbox : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M4.5 2A4.5 4.5 0 0 0 0 6.5v6.9a.6.6 0 0 0 .6.6h14.8a.6.6 0 0 0 .6-.6V6.5A4.5 4.5 0 0 0 11.5 2h-7Zm2.829 1H11.5A3.5 3.5 0 0 1 15 6.5V13H9V6.5A4.491 4.491 0 0 0 7.329 3ZM8 13V6.5a3.5 3.5 0 1 0-7 0V13h7Zm1.5-7a.5.5 0 0 1 .5-.5h3.5a.5.5 0 0 1 .5.5v2a.5.5 0 0 1-.5.5H12a.5.5 0 0 1-.5-.5V6.5H10a.5.5 0 0 1-.5-.5Zm3 .5v1h.5v-1h-.5Zm-10-1a.5.5 0 0 0 0 1H6a.5.5 0 0 0 0-1H2.5Z";

    protected override string IconGeometry20 { get; }
        = "M5.625 2.5A5.625 5.625 0 0 0 0 8.125v8.625c0 .414.336.75.75.75h18.5a.75.75 0 0 0 .75-.75V8.125A5.625 5.625 0 0 0 14.375 2.5h-8.75Zm3.536 1.25h5.214a4.375 4.375 0 0 1 4.375 4.375v8.125h-7.5V8.125A5.614 5.614 0 0 0 9.16 3.75ZM10 16.25V8.125a4.375 4.375 0 1 0-8.75 0v8.125H10Zm1.875-8.75c0-.345.28-.625.625-.625h4.375c.345 0 .625.28.625.625V10c0 .345-.28.625-.625.625H15a.625.625 0 0 1-.625-.625V8.125H12.5a.625.625 0 0 1-.625-.625Zm3.75.625v1.25h.625v-1.25h-.625Zm-12.5-1.25a.625.625 0 1 0 0 1.25H7.5a.625.625 0 1 0 0-1.25H3.125Z"; 

    protected override string IconGeometry24 { get; }
        = "M6.75 3A6.75 6.75 0 0 0 0 9.75V20.1a.9.9 0 0 0 .9.9h22.2a.9.9 0 0 0 .9-.9V9.75A6.75 6.75 0 0 0 17.25 3H6.75Zm4.243 1.5h6.257c2.9 0 5.25 2.35 5.25 5.25v9.75h-9V9.75c0-2.12-.978-4.013-2.507-5.25ZM12 19.5V9.75a5.25 5.25 0 1 0-10.5 0v9.75H12ZM14.25 9a.75.75 0 0 1 .75-.75h5.25A.75.75 0 0 1 21 9v3a.75.75 0 0 1-.75.75H18a.75.75 0 0 1-.75-.75V9.75H15a.75.75 0 0 1-.75-.75Zm4.5.75v1.5h.75v-1.5h-.75Zm-15-1.5a.75.75 0 0 0 0 1.5H9a.75.75 0 0 0 0-1.5H3.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M9 4a9 9 0 0 0-9 9v13.8A1.2 1.2 0 0 0 1.2 28h29.6a1.2 1.2 0 0 0 1.2-1.2V13a9 9 0 0 0-9-9H9Zm5.657 2H23a7 7 0 0 1 7 7v13H18V13a8.983 8.983 0 0 0-3.343-7ZM16 26V13a7 7 0 1 0-14 0v13h14Zm3-14a1 1 0 0 1 1-1h7a1 1 0 0 1 1 1v4a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1v-3h-3a1 1 0 0 1-1-1Zm6 1v2h1v-2h-1ZM5 11a1 1 0 1 0 0 2h7a1 1 0 1 0 0-2H5Z";
}