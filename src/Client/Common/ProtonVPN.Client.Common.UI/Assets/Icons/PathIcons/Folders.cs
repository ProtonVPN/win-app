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

public class Folders : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3.11 2.5A1.5 1.5 0 0 1 4.61 1h3.536a1.5 1.5 0 0 1 .67.158l1.642.821a.2.2 0 0 0 .09.021H14.5A1.5 1.5 0 0 1 16 3.5v8a1.5 1.5 0 0 1-1.5 1.5H4.61a1.5 1.5 0 0 1-1.5-1.5V4H3a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h9a1 1 0 0 0 1-1h1a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h.11v-.5ZM15 3.5v8a.5.5 0 0 1-.5.5H4.61a.5.5 0 0 1-.5-.5v-9a.5.5 0 0 1 .5-.5h3.536a.5.5 0 0 1 .223.053l1.642.82a1.2 1.2 0 0 0 .536.127H14.5a.5.5 0 0 1 .5.5Z";

    protected override string IconGeometry20 { get; }
        = "M18.759 5.188v9.187a.623.623 0 0 1-.62.625H5.729a.623.623 0 0 1-.62-.625V3.125c0-.345.277-.625.62-.625h4.916c.144 0 .283.05.395.143l1.897 1.577c.267.221.602.342.947.342h4.255c.342 0 .62.28.62.626ZM3.868 3.125c0-1.036.833-1.875 1.861-1.875h4.916c.432 0 .85.152 1.184.429l1.897 1.576c.045.037.1.057.158.057h4.255c1.028 0 1.861.84 1.861 1.876v9.187a1.868 1.868 0 0 1-1.861 1.875H5.729a1.868 1.868 0 0 1-1.861-1.875v-8.75h-.136c-.685 0-1.241.56-1.241 1.25v9.375c0 .69.556 1.25 1.24 1.25h11.926c.686 0 1.241-.56 1.241-1.25h1.241c0 1.38-1.111 2.5-2.482 2.5H3.732a2.491 2.491 0 0 1-2.482-2.5V6.875c0-1.38 1.111-2.5 2.482-2.5h.136v-1.25Z"; 

    protected override string IconGeometry24 { get; }
        = "M22.51 6.225V17.25c0 .414-.333.75-.744.75H6.875a.747.747 0 0 1-.745-.75V3.75c0-.414.334-.75.745-.75h5.899c.172 0 .34.06.473.171l2.277 1.892c.32.266.722.412 1.137.412h5.105c.412 0 .745.336.745.75ZM4.642 3.75c0-1.243 1-2.25 2.234-2.25h5.899c.518 0 1.02.182 1.421.514l2.277 1.892c.053.045.12.069.19.069h5.104A2.242 2.242 0 0 1 24 6.225V17.25c0 1.243-1 2.25-2.234 2.25H6.875a2.242 2.242 0 0 1-2.234-2.25V6.75h-.163c-.822 0-1.489.672-1.489 1.5V19.5c0 .828.667 1.5 1.49 1.5h14.31c.822 0 1.489-.672 1.489-1.5h1.488c0 1.657-1.333 3-2.977 3H4.479a2.989 2.989 0 0 1-2.979-3V8.25c0-1.657 1.333-3 2.978-3h.163v-1.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M30.015 8.3V23c0 .552-.445 1-.993 1H9.166a.996.996 0 0 1-.992-1V5c0-.552.444-1 .992-1h7.865c.23 0 .454.08.632.229L20.7 6.75c.427.355.963.549 1.516.549h6.807c.548 0 .993.448.993 1ZM6.188 5c0-1.657 1.334-3 2.978-3h7.865a2.97 2.97 0 0 1 1.896.686l3.035 2.523c.072.059.16.091.253.091h6.807C30.667 5.3 32 6.643 32 8.3V23c0 1.657-1.333 3-2.978 3H9.166c-1.644 0-2.978-1.343-2.978-3V9h-.217a1.993 1.993 0 0 0-1.985 2v15c0 1.104.888 2 1.985 2h19.08a1.993 1.993 0 0 0 1.986-2h1.985c0 2.21-1.777 4-3.97 4H5.97C3.778 30 2 28.209 2 26V11c0-2.21 1.778-4 3.971-4h.217V5Z";
}