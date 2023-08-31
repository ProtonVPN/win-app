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

using System;

namespace ProtonVPN.Core.Servers
{
    public class ServerFeatures
    {
        public const ulong STANDARD = 0;
        public const ulong SECURE_CORE = 1;
        public const ulong TOR = 2;
        public const ulong P2P = 4;
        public const ulong STREAMING = 8;
        public const ulong IPV6 = 16;
        public const ulong B2B = 32;

        private readonly ulong _value;

        public ServerFeatures(ulong value)
        {
            _value = value;
        }

        public static implicit operator ulong(ServerFeatures item) => item._value;

        public bool IsSecureCore() => IsSecureCore(_value);
        public bool SupportsTor() => SupportsTor(_value);
        public bool SupportsP2P() => SupportsP2P(_value);
        public bool IsB2B() => IsB2B(_value);

        public static bool IsSecureCore(ulong value) => (value & SECURE_CORE) != 0;
        public static bool SupportsTor(ulong value) => (value & TOR) != 0;
        public static bool SupportsP2P(ulong value) => (value & P2P) != 0;
        public static bool SupportsStreaming(ulong value) => (value & STREAMING) != 0;
        public static bool SupportsIpV6(ulong value) => (value & IPV6) != 0;
        public static bool IsB2B(ulong value) => (value & B2B) != 0;
    }

    [Flags]
    public enum Features : ulong
    {
        None = 0,
        SecureCore = 1,
        Tor = 2,
        P2P = 4,
        B2B = 32,
    }

    public static class FeaturesExtensions
    {
        public static bool IsSecureCore(this Features value) => (value & Features.SecureCore) != 0;
        public static bool SupportsTor(this Features value) => (value & Features.Tor) != 0;
        public static bool SupportsP2P(this Features value) => (value & Features.P2P) != 0;
        public static bool IsB2B(this Features value) => (value & Features.B2B) != 0;
    }
}