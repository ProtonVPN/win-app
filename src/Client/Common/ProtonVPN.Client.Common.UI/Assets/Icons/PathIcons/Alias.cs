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

public class Alias : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3.588 3.087a2 2 0 0 1 2.728-1.353l1.093.469a1.5 1.5 0 0 0 1.182 0l1.093-.469a2 2 0 0 1 2.728 1.353L13.39 7h1.11a.5.5 0 0 1 0 1h-13a.5.5 0 0 1 0-1h1.11l.978-3.913Zm2.334-.434a1 1 0 0 0-1.364.677L3.64 7h8.72l-.918-3.67a1 1 0 0 0-1.364-.677l-1.093.469a2.5 2.5 0 0 1-1.97 0l-1.093-.469Zm.65 7.447A2.5 2.5 0 1 0 7 11.5a1 1 0 1 1 2 0 2.5 2.5 0 1 0 .428-1.4C9.065 9.73 8.56 9.5 8 9.5c-.56 0-1.065.23-1.428.6ZM6 11.5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0Zm4 0a1.5 1.5 0 1 0 3 0 1.5 1.5 0 0 0-3 0Z";

    protected override string IconGeometry20 { get; }
        = "M12.098 2.706a2.5 2.5 0 0 1 3.416 1.693l1.237 4.98h1.374a.625.625 0 1 1 0 1.25H1.875a.625.625 0 1 1 0-1.25h1.374l1.237-4.98a2.5 2.5 0 0 1 3.416-1.693l1.355.584a1.875 1.875 0 0 0 1.486 0l1.355-.584ZM14.3 4.7a1.25 1.25 0 0 0-1.708-.847l-1.355.585c-.79.34-1.686.34-2.476 0l-1.355-.585A1.25 1.25 0 0 0 5.7 4.7L4.55 9.329h10.902l-1.15-4.63Zm-6.085 7.93a3.125 3.125 0 1 0 .535 1.75 1.25 1.25 0 0 1 2.5-.001 3.125 3.125 0 1 0 .536-1.75A2.493 2.493 0 0 0 10 11.88c-.7 0-1.332.287-1.785.75ZM7.5 14.378a1.875 1.875 0 1 1-3.75 0 1.875 1.875 0 0 1 3.75 0Zm5 0a1.875 1.875 0 1 0 3.75 0 1.875 1.875 0 0 0-3.75 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M14.517 3.247a3 3 0 0 1 4.1 2.031l1.485 5.977h1.648a.75.75 0 1 1 0 1.5H2.25a.75.75 0 0 1 0-1.5h1.649l1.484-5.977a3 3 0 0 1 4.1-2.031l1.626.701a2.25 2.25 0 0 0 1.782 0l1.626-.701Zm2.645 2.393a1.5 1.5 0 0 0-2.05-1.016l-1.627.702a3.75 3.75 0 0 1-2.97 0l-1.626-.702a1.5 1.5 0 0 0-2.05 1.016l-1.38 5.555h13.082l-1.38-5.555Zm-7.305 9.515a3.75 3.75 0 1 0 .643 2.1 1.5 1.5 0 1 1 3 0 3.75 3.75 0 1 0 .643-2.1 2.991 2.991 0 0 0-2.143-.9 2.99 2.99 0 0 0-2.143.9ZM9 17.255a2.25 2.25 0 1 1-4.5 0 2.25 2.25 0 0 1 4.5 0Zm6 0a2.25 2.25 0 1 0 4.5 0 2.25 2.25 0 0 0-4.5 0Z"; 

    protected override string IconGeometry32 { get; }
        = "M19.356 4.33a4 4 0 0 1 5.467 2.708l1.979 7.969H29a1 1 0 1 1 0 2H3a1 1 0 1 1 0-2h2.198l1.979-7.97a4 4 0 0 1 5.467-2.708l2.168.936a3 3 0 0 0 2.376 0l2.168-.936Zm3.526 3.19a2 2 0 0 0-2.733-1.354L17.98 7.1a5 5 0 0 1-3.962 0l-2.168-.935A2 2 0 0 0 9.118 7.52l-1.84 7.406h17.443l-1.84-7.406Zm-9.739 12.687a5 5 0 1 0 .857 2.8 2 2 0 1 1 4 0 5 5 0 1 0 .857-2.8 3.988 3.988 0 0 0-2.857-1.2c-1.12 0-2.13.459-2.857 1.2ZM12 23.007a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm8 0a3 3 0 1 0 6 0 3 3 0 0 0-6 0Z";
}