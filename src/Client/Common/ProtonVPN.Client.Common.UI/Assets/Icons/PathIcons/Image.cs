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

public class Image : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M13 3H3a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V4a1 1 0 0 0-1-1ZM3 2a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2H3Zm9.446 10c.452 0 .713-.483.448-.828L9.792 7.129a.344.344 0 0 0-.538 0l-2.256 2.94-1.254-1.634a.344.344 0 0 0-.537 0l-2.1 2.737c-.266.345-.005.828.447.828h8.892ZM6.5 7a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z";

    protected override string IconGeometry20 { get; }
        = "M16.25 3.75H3.75c-.69 0-1.25.56-1.25 1.25v10c0 .69.56 1.25 1.25 1.25h12.5c.69 0 1.25-.56 1.25-1.25V5c0-.69-.56-1.25-1.25-1.25ZM3.75 2.5A2.5 2.5 0 0 0 1.25 5v10a2.5 2.5 0 0 0 2.5 2.5h12.5a2.5 2.5 0 0 0 2.5-2.5V5a2.5 2.5 0 0 0-2.5-2.5H3.75ZM15.558 15c.564 0 .89-.604.56-1.036L12.24 8.912a.43.43 0 0 0-.672 0l-2.82 3.674-1.568-2.043a.43.43 0 0 0-.672 0l-2.625 3.421c-.331.432-.005 1.036.56 1.036h11.115ZM8.125 8.75a1.25 1.25 0 1 0 0-2.5 1.25 1.25 0 0 0 0 2.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M19.5 4.5h-15A1.5 1.5 0 0 0 3 6v12a1.5 1.5 0 0 0 1.5 1.5h15A1.5 1.5 0 0 0 21 18V6a1.5 1.5 0 0 0-1.5-1.5ZM4.5 3a3 3 0 0 0-3 3v12a3 3 0 0 0 3 3h15a3 3 0 0 0 3-3V6a3 3 0 0 0-3-3h-15Zm14.17 15c.676 0 1.068-.725.67-1.243l-4.652-6.063a.515.515 0 0 0-.806 0l-3.384 4.41-1.882-2.452a.515.515 0 0 0-.806 0l-3.15 4.105c-.398.518-.006 1.243.67 1.243h13.34Zm-8.92-7.5a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Z"; 

    protected override string IconGeometry32 { get; }
        = "M26 6H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2ZM6 4a4 4 0 0 0-4 4v16a4 4 0 0 0 4 4h20a4 4 0 0 0 4-4V8a4 4 0 0 0-4-4H6Zm18.892 20c.903 0 1.425-.966.896-1.657l-6.205-8.084a.687.687 0 0 0-1.074 0l-4.512 5.879-2.509-3.269a.687.687 0 0 0-1.075 0l-4.2 5.474c-.53.69-.008 1.657.895 1.657h17.784ZM13 14a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z";
}