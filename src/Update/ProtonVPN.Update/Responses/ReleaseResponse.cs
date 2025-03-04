﻿/*
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

using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProtonVPN.Update.Responses;

public class ReleaseResponse
{
    public string CategoryName { get; set; }
    public string Version { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public FileResponse File { get; set; }
    public IReadOnlyList<ReleaseNote> ReleaseNotes { get; set; }
    public SystemVersion SystemVersion { get; set; }
    public decimal? RolloutProportion { get; set; }
}