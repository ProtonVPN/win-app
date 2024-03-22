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

public class BrandWindows : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.636 1H1v6.636h6.636V1ZM15 1H8.364v6.636H15V1ZM7.636 8.364H1V15h6.636V8.364Zm7.364 0H8.364V15H15V8.364Z";

    protected override string IconGeometry20 { get; }
        = "M9.545 1.25H1.25v8.295h8.295V1.25Zm9.205 0h-8.295v8.295h8.295V1.25Zm-9.205 9.206H1.25v8.294h8.295v-8.294Zm9.205 0h-8.295v8.294h8.295v-8.294Z"; 

    protected override string IconGeometry24 { get; }
        = "M11.454 1.5H1.5v9.954h9.954V1.5Zm11.046 0h-9.954v9.954H22.5V1.5ZM11.454 12.546H1.5V22.5h9.954v-9.954Zm11.046 0h-9.954V22.5H22.5v-9.954Z"; 

    protected override string IconGeometry32 { get; }
        = "M15.272 2H2v13.271h13.272V2ZM30 2H16.728v13.271H30V2ZM15.272 16.729H2V30h13.272V16.729Zm14.728 0H16.728V30H30V16.729Z";
}