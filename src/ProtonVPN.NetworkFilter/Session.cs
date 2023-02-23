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

namespace ProtonVPN.NetworkFilter
{
    public class Session
    {
        public IntPtr Handle { get; }
        public SessionType Type { get; }

        protected Session(IntPtr handle, SessionType type)
        {
            Handle = handle;
            Type = type;
        }

        public static Session Dynamic()
        {
            return new Session(IpFilterNative.CreateDynamicSession(), SessionType.Dynamic);
        }

        public static Session Permanent()
        {
            return new Session(IpFilterNative.CreateSession(), SessionType.Permanent);
        }

        public void Close()
        {
            IpFilterNative.DestroySession(Handle);
        }

        public void StartTransaction()
        {
            IpFilterNative.StartTransaction(Handle);
        }

        public void AbortTransaction()
        {
            IpFilterNative.AbortTransaction(Handle);
        }

        public void CommitTransaction()
        {
            IpFilterNative.CommitTransaction(Handle);
        }
    }

}
