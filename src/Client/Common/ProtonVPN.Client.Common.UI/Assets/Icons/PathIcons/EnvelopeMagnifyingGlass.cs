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

public class EnvelopeMagnifyingGlass : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M2.88 2c-.403 0-.735 0-1.006.022-.281.023-.54.072-.782.196a2 2 0 0 0-.874.874c-.124.243-.173.501-.196.782C0 4.144 0 4.477 0 4.88v5.242c0 .402 0 .734.022 1.005.023.281.072.54.196.782a2 2 0 0 0 .874.874c.243.124.501.173.782.196.27.022.603.022 1.005.022H8v-1H2.9c-.428 0-.72 0-.944-.019-.22-.018-.332-.05-.41-.09a1 1 0 0 1-.437-.437c-.04-.078-.072-.19-.09-.41A12.925 12.925 0 0 1 1 10.1V5.372L6.219 8.43a1.5 1.5 0 0 0 1.562 0L13 5.372V8h1V4.88c0-.403 0-.735-.022-1.006-.023-.281-.072-.54-.196-.782a2 2 0 0 0-.874-.874c-.243-.124-.501-.173-.782-.196C11.856 2 11.523 2 11.12 2H2.879Zm10.115 2.217a4.978 4.978 0 0 0-.014-.261c-.018-.22-.05-.332-.09-.41a1 1 0 0 0-.437-.437c-.078-.04-.19-.072-.41-.09A12.925 12.925 0 0 0 11.1 3H2.9c-.428 0-.72 0-.944.019-.22.018-.332.05-.41.09a1 1 0 0 0-.437.437c-.04.078-.072.19-.09.41a4.99 4.99 0 0 0-.014.26l5.733 3.358a.5.5 0 0 0 .524 0l.01-.006 5.723-3.351Z M14.445 13.738a3 3 0 1 0-.707.707l1.409 1.408a.5.5 0 1 0 .707-.707l-1.409-1.408ZM12 14a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z";
}