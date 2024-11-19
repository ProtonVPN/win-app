/*
 * Copyright (c) 2024 Proton AG
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

using System.Text.Json.Serialization;
using ProtonInstaller.Contracts.ProtonDrive;
using ProtonInstaller.Contracts.ProtonMail;
using ProtonInstaller.Contracts.ProtonPass;

namespace ProtonInstaller;

[JsonSerializable(typeof(ProtonDriveReleasesResponse))]
[JsonSerializable(typeof(ProtonDriveReleaseResponse))]
[JsonSerializable(typeof(ProtonDriveFileResponse))]
[JsonSerializable(typeof(ProtonMailReleasesResponse))]
[JsonSerializable(typeof(ProtonMailReleaseResponse))]
[JsonSerializable(typeof(ProtonMailFileResponse))]
[JsonSerializable(typeof(ProtonPassReleasesResponse))]
[JsonSerializable(typeof(ProtonPassReleaseResponse))]
[JsonSerializable(typeof(ProtonPassFileResponse))]
public partial class JsonContext : JsonSerializerContext;