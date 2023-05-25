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

public class FilePdf : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M13 13V6h-2.5A1.5 1.5 0 0 1 9 4.5V2H4a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1ZM10 2.414 12.586 5H10.5a.5.5 0 0 1-.5-.5V2.414ZM4 1a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V5.828a2 2 0 0 0-.586-1.414l-2.828-2.828A2 2 0 0 0 9.172 1H4Zm3.941 4.5a1.22 1.22 0 0 0-1.057 1.83l.488.846-1.124 2.2H5.165a1.273 1.273 0 0 0-1.272 1.272c0 1.28 1.677 1.758 2.352.672l.587-.944h2.23l.586.943c.675 1.086 2.351.609 2.352-.67a1.27 1.27 0 0 0-1.27-1.273H9.646L8.51 8.175l.487-.845A1.22 1.22 0 0 0 7.94 5.5Zm.58 4.876-.578-1.12-.572 1.12h1.15Zm-3.356 1h.49l-.259.416c-.144.232-.503.13-.503-.144a.272.272 0 0 1 .272-.272Zm5.333.416-.258-.416h.49a.27.27 0 0 1 .27.271v.001c0 .273-.358.376-.502.144ZM8.132 6.83l-.19.33-.192-.33a.22.22 0 1 1 .382 0Z";
}