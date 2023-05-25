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

public class Globe : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3.012 8H1.02a6.504 6.504 0 0 1 3.805-5.425 7.143 7.143 0 0 0-.591.917C3.539 4.764 3.09 6.39 3.013 8Zm0 1H1.02a6.504 6.504 0 0 0 3.805 5.425 7.14 7.14 0 0 1-.591-.917C3.539 12.236 3.09 10.61 3.013 9ZM7 14.917c-.664-.219-1.331-.864-1.89-1.888C4.5 11.908 4.092 10.451 4.014 9H7v5.917Zm3.176-.492c.217-.284.415-.593.591-.917.694-1.272 1.143-2.897 1.22-4.508h1.994a6.504 6.504 0 0 1-3.805 5.425ZM10.986 9c-.077 1.451-.485 2.908-1.097 4.03-.558 1.023-1.225 1.668-1.889 1.887V9h2.986Zm1.002-1h1.993a6.504 6.504 0 0 0-3.805-5.425c.217.284.415.593.591.917.694 1.272 1.143 2.897 1.22 4.508ZM8 2.083c.664.219 1.331.865 1.89 1.888.61 1.121 1.019 2.578 1.096 4.029H8V2.083Zm-1 0V8H4.013c.078-1.451.486-2.908 1.098-4.03C5.669 2.949 6.336 2.303 7 2.084ZM7.5 1a7.5 7.5 0 1 0 0 15 7.5 7.5 0 0 0 0-15Z";
}