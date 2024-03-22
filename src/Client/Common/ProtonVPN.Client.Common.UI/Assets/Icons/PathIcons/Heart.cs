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

public class Heart : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.195 3.903a3.025 3.025 0 0 0-4.314 0A3.112 3.112 0 0 0 2.817 8.2L8 13.772 13.184 8.2a3.112 3.112 0 0 0-.065-4.297 3.025 3.025 0 0 0-4.314 0l-.456.461a.49.49 0 0 1-.698 0l-.456-.461Zm-5.012-.707a4.005 4.005 0 0 1 5.71 0L8 3.305l.107-.109a4.005 4.005 0 0 1 5.71 0 4.12 4.12 0 0 1 .085 5.689L8.36 14.843a.491.491 0 0 1-.72 0L2.097 8.885a4.12 4.12 0 0 1 .086-5.689Z";

    protected override string IconGeometry20 { get; }
        = "M8.993 4.879a3.782 3.782 0 0 0-5.392 0 3.89 3.89 0 0 0-.08 5.37L10 17.216l6.48-6.965a3.89 3.89 0 0 0-.081-5.371 3.782 3.782 0 0 0-5.392 0l-.57.577a.612.612 0 0 1-.873 0l-.57-.577Zm-6.265-.883a5.006 5.006 0 0 1 7.138 0L10 4.13l.134-.135a5.006 5.006 0 0 1 7.138 0 5.149 5.149 0 0 1 .106 7.11l-6.929 7.448a.614.614 0 0 1-.898 0l-6.93-7.448a5.149 5.149 0 0 1 .107-7.11Z"; 

    protected override string IconGeometry24 { get; }
        = "M10.792 5.854a4.538 4.538 0 0 0-6.47 0 4.668 4.668 0 0 0-.097 6.446L12 20.658l7.775-8.358a4.668 4.668 0 0 0-.096-6.446 4.538 4.538 0 0 0-6.47 0l-.685.693a.734.734 0 0 1-1.048 0l-.684-.693Zm-7.518-1.06a6.007 6.007 0 0 1 8.565 0l.161.163.16-.162a6.007 6.007 0 0 1 8.566 0c2.315 2.341 2.371 6.12.128 8.532l-8.315 8.937a.737.737 0 0 1-1.078 0l-8.315-8.937C.903 10.915.96 7.137 3.274 4.795Z"; 

    protected override string IconGeometry32 { get; }
        = "M14.39 7.806a6.05 6.05 0 0 0-8.628 0C3.43 10.164 3.373 13.97 5.633 16.4L16 27.543 26.367 16.4c2.26-2.43 2.203-6.236-.129-8.594a6.05 6.05 0 0 0-8.627 0l-.913.923a.98.98 0 0 1-1.396 0l-.913-.923ZM4.364 6.393a8.01 8.01 0 0 1 11.42 0L16 6.61l.215-.217a8.01 8.01 0 0 1 11.42 0c3.086 3.122 3.162 8.16.17 11.376L16.72 29.686a.982.982 0 0 1-1.438 0L4.195 17.769c-2.991-3.215-2.916-8.254.17-11.376Z";
}