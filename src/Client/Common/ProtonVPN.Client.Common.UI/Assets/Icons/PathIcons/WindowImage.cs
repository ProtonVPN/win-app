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

public class WindowImage : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3 3h10a1 1 0 0 1 1 1H2a1 1 0 0 1 1-1ZM1 5V4a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V5Zm1 0h12v7a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V5Zm9.557 7c.361 0 .57-.387.358-.663L9.433 8.104a.275.275 0 0 0-.43 0L7.2 10.455 6.195 9.148a.275.275 0 0 0-.43 0l-1.68 2.19c-.212.275-.003.662.358.662h7.114ZM6.8 8a.8.8 0 1 0 0-1.6.8.8 0 0 0 0 1.6Z";

    protected override string IconGeometry20 { get; }
        = "M3.75 3.75h12.5c.69 0 1.25.56 1.25 1.25v.625h-15V5c0-.69.56-1.25 1.25-1.25Zm-2.5 3.125V5a2.5 2.5 0 0 1 2.5-2.5h12.5a2.5 2.5 0 0 1 2.5 2.5v10a2.5 2.5 0 0 1-2.5 2.5H3.75a2.5 2.5 0 0 1-2.5-2.5V6.875Zm1.25 0h15V15c0 .69-.56 1.25-1.25 1.25H3.75c-.69 0-1.25-.56-1.25-1.25V6.875Zm11.946 8.501c.452 0 .713-.483.448-.828l-3.102-4.043a.343.343 0 0 0-.538 0l-2.256 2.94-1.254-1.634a.344.344 0 0 0-.537 0l-2.1 2.737c-.266.345-.005.828.447.828h8.892Zm-5.946-5a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.5 4.5h15A1.5 1.5 0 0 1 21 6v.75H3V6a1.5 1.5 0 0 1 1.5-1.5Zm-3 3.75V6a3 3 0 0 1 3-3h15a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3h-15a3 3 0 0 1-3-3V8.25Zm1.5 0h18V18a1.5 1.5 0 0 1-1.5 1.5h-15A1.5 1.5 0 0 1 3 18V8.25Zm14.335 10.201c.542 0 .855-.58.538-.994l-3.723-4.85a.412.412 0 0 0-.645 0l-2.707 3.527-1.505-1.961a.412.412 0 0 0-.645 0l-2.52 3.284c-.318.414-.005.994.537.994h10.67Zm-7.135-6a1.2 1.2 0 1 0 0-2.4 1.2 1.2 0 0 0 0 2.4Z"; 

    protected override string IconGeometry32 { get; }
        = "M6 6h20a2 2 0 0 1 2 2v1H4V8a2 2 0 0 1 2-2Zm-4 5V8a4 4 0 0 1 4-4h20a4 4 0 0 1 4 4v16a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V11Zm2 0h24v13a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V11Zm19.114 13.602c.722 0 1.14-.774.716-1.326l-4.963-6.467a.55.55 0 0 0-.86 0l-3.61 4.703-2.007-2.615a.55.55 0 0 0-.86 0l-3.36 4.38c-.424.551-.006 1.325.716 1.325h14.228ZM13.6 16.6a1.6 1.6 0 1 0 0-3.2 1.6 1.6 0 0 0 0 3.2Z";
}