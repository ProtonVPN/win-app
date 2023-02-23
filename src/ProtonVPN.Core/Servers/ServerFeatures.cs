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
        public const int STANDARD = 0;
        public const int SECURE_CORE = 1;
        public const int TOR = 2;
        public const int P2P = 4;
        public const int STREAMING = 8;
        public const int IPV6 = 16;
        public const int PARTNER = 64;

        private readonly int _value;

        public ServerFeatures(int value)
        {
            _value = value;
        }

        public static implicit operator int(ServerFeatures item) => item._value;

        public bool IsSecureCore() => IsSecureCore(_value);
        public bool SupportsTor() => SupportsTor(_value);
        public bool SupportsP2P() => SupportsP2P(_value);

        public static bool IsSecureCore(int value) => (value & SECURE_CORE) != 0;
        public static bool SupportsTor(int value) => (value & TOR) != 0;
        public static bool SupportsP2P(int value) => (value & P2P) != 0;
        public static bool SupportsStreaming(int value) => (value & STREAMING) != 0;
        public static bool SupportsIpV6(int value) => (value & IPV6) != 0;
        public static bool IsPartner(int value) => (value & PARTNER) != 0;
    }

    [Flags]
    public enum Features
    {
        None = 0,
        SecureCore = 1,
        Tor = 2,
        P2P = 4,
    }

    public static class FeaturesExtensions
    {
        public static bool IsSecureCore(this Features value) => (value & Features.SecureCore) != 0;
        public static bool SupportsTor(this Features value) => (value & Features.Tor) != 0;
        public static bool SupportsP2P(this Features value) => (value & Features.P2P) != 0;
    }
}