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

public class TextQuote : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3 2.5a.5.5 0 0 0-1 0v11a.5.5 0 0 0 1 0v-11Zm1.5 3.833c0-1.066.952-1.833 2-1.833s2 .767 2 1.833V7c0 .098-.008.193-.023.285-.131 1.384-.807 2.678-1.902 3.65l-.493.439a.5.5 0 1 1-.664-.748l.492-.438A4.843 4.843 0 0 0 7.017 8.77c-.167.042-.34.063-.517.063-1.048 0-2-.767-2-1.833v-.667Zm3 .458c0 .115-.005.23-.014.343-.074.361-.444.7-.986.7-.61 0-1-.427-1-.834v-.667c0-.406.39-.833 1-.833s1 .427 1 .833v.458Zm2-.458c0-1.066.952-1.833 2-1.833s2 .767 2 1.833V7c0 .098-.008.193-.023.285-.131 1.384-.807 2.678-1.902 3.65l-.493.439a.5.5 0 1 1-.664-.748l.493-.438c.468-.416.84-.897 1.106-1.418-.167.042-.34.063-.517.063-1.048 0-2-.767-2-1.833v-.667Zm3 .458c0 .115-.005.23-.014.343-.075.361-.444.7-.986.7-.61 0-1-.427-1-.834v-.667c0-.406.39-.833 1-.833s1 .427 1 .833v.458Z";
}