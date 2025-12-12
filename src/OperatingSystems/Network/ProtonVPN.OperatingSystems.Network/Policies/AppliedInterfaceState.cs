/*
 * Copyright (c) 2025 Proton AG
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

using static Vanara.PInvoke.Ws2_32;

namespace ProtonVPN.OperatingSystems.Network.Policies;

public record AppliedInterfaceState
{
    public ADDRESS_FAMILY Family;
    public bool OriginalWeakHostSend;
    public bool WasWeakHostSendModified;
    public bool OriginalWeakHostReceive;
    public bool WasWeakHostReceiveModified;
}