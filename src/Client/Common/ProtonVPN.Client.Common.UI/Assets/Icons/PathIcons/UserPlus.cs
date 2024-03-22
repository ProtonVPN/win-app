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

public class UserPlus : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M11 5a2 2 0 1 1-4 0 2 2 0 0 1 4 0Zm1 0a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm-7.145 5.98A2.092 2.092 0 0 1 6.641 10h4.718c.74 0 1.416.38 1.786.98l.782 1.272c.182.296-.01.748-.467.748H4.54c-.457 0-.649-.452-.467-.748l.782-1.272Zm-.851-.524A3.092 3.092 0 0 1 6.64 9h4.718a3.09 3.09 0 0 1 2.637 1.457l.782 1.271c.615 1-.123 2.272-1.318 2.272H4.54c-1.195 0-1.933-1.272-1.319-2.272l.783-1.271ZM2 6V4.5a.5.5 0 0 1 1 0V6h1.5a.5.5 0 0 1 0 1H3v1.5a.5.5 0 0 1-1 0V7H.5a.5.5 0 0 1 0-1H2Z";

    protected override string IconGeometry20 { get; }
        = "M13.75 6.25a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0Zm1.25 0a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0Zm-8.93 7.476A2.615 2.615 0 0 1 8.3 12.5h5.898c.926 0 1.77.474 2.232 1.226l.977 1.59c.228.37-.011.934-.583.934H5.675c-.572 0-.811-.565-.583-.935l.977-1.59Zm-1.065-.655A3.865 3.865 0 0 1 8.3 11.25h5.898c1.35 0 2.602.691 3.296 1.82l.978 1.59c.769 1.25-.154 2.84-1.648 2.84H5.675c-1.494 0-2.417-1.59-1.648-2.84l.978-1.59ZM2.5 7.5V5.625a.625.625 0 1 1 1.25 0V7.5h1.875a.625.625 0 1 1 0 1.25H3.75v1.875a.625.625 0 1 1-1.25 0V8.75H.625a.625.625 0 1 1 0-1.25H2.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M16.5 7.5a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm1.5 0a4.5 4.5 0 1 1-9 0 4.5 4.5 0 0 1 9 0ZM7.283 16.47A3.138 3.138 0 0 1 9.961 15h7.078c1.11 0 2.123.569 2.678 1.47l1.173 1.908c.273.444-.014 1.122-.7 1.122H6.81c-.686 0-.973-.678-.7-1.122l1.173-1.907Zm-1.277-.785C6.839 14.329 8.34 13.5 9.96 13.5h7.078c1.62 0 3.122.83 3.955 2.185l1.174 1.907c.922 1.5-.185 3.408-1.978 3.408H6.81c-1.793 0-2.9-1.908-1.978-3.408l1.174-1.907ZM3 9V6.75a.75.75 0 0 1 1.5 0V9h2.25a.75.75 0 0 1 0 1.5H4.5v2.25a.75.75 0 0 1-1.5 0V10.5H.75a.75.75 0 0 1 0-1.5H3Z"; 

    protected override string IconGeometry32 { get; }
        = "M22 10a4 4 0 1 1-8 0 4 4 0 0 1 8 0Zm2 0a6 6 0 1 1-12 0 6 6 0 0 1 12 0ZM9.711 21.96c.74-1.202 2.09-1.96 3.57-1.96h9.437c1.482 0 2.831.758 3.571 1.96l1.564 2.544c.364.592-.018 1.496-.933 1.496H9.08c-.915 0-1.298-.904-.933-1.496l1.564-2.543Zm-1.704-1.047C9.12 19.106 11.121 18 13.282 18h9.436c2.16 0 4.163 1.106 5.275 2.913l1.564 2.543c1.23 2-.247 4.544-2.637 4.544H9.08c-2.39 0-3.867-2.544-2.637-4.544l1.564-2.543ZM4 12V9a1 1 0 0 1 2 0v3h3a1 1 0 1 1 0 2H6v3a1 1 0 1 1-2 0v-3H1a1 1 0 1 1 0-2h3Z";
}