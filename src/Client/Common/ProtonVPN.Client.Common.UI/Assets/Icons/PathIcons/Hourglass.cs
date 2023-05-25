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

public class Hourglass : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3.5 2a.5.5 0 0 0 0 1H4v.322c0 .801 0 1.202.097 1.575a3 3 0 0 0 .42.927c.217.319.519.582 1.122 1.11l.254.222c.443.388.754.66 1.022.844-.268.184-.58.456-1.022.844l-.254.222c-.603.528-.905.791-1.121 1.11a2.999 2.999 0 0 0-.42.927C4 11.476 4 11.877 4 12.678V13h-.5a.5.5 0 0 0 0 1h9a.5.5 0 1 0 0-1H12v-.322c0-.801 0-1.202-.097-1.575a3.001 3.001 0 0 0-.42-.927c-.217-.319-.519-.582-1.122-1.11l-.254-.222c-.443-.388-.754-.66-1.022-.844.268-.184.58-.456 1.022-.844l.254-.222c.603-.528.905-.791 1.121-1.11.192-.283.334-.596.42-.927C12 4.524 12 4.123 12 3.322V3h.5a.5.5 0 0 0 0-1h-9ZM11 13v-.322c0-.854-.008-1.103-.065-1.322a2 2 0 0 0-.28-.618c-.128-.188-.31-.357-.953-.92l-.253-.222a15.928 15.928 0 0 0-.833-.702c-.191-.143-.281-.18-.332-.195a1 1 0 0 0-.568 0c-.05.015-.14.052-.332.195-.201.15-.448.366-.833.702l-.253.222c-.643.563-.825.732-.953.92a2 2 0 0 0-.28.618c-.057.22-.065.468-.065 1.322V13h6ZM5 3.322V3h6v.322c0 .854-.008 1.103-.065 1.322a2 2 0 0 1-.28.618c-.128.188-.31.357-.953.92l-.253.222c-.385.336-.632.551-.833.702-.191.143-.281.18-.332.195a1 1 0 0 1-.568 0c-.05-.015-.14-.052-.332-.195-.201-.15-.448-.366-.833-.702l-.253-.222c-.643-.563-.825-.732-.953-.92a2 2 0 0 1-.28-.618C5.008 4.424 5 4.176 5 3.322Z";
}