/*
 * Copyright (c) 2020 Proton Technologies AG
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ProtonVPN.Core.Srp
{
    public static class SrpPInvoke
    {
        private const string binaryName = "GoSrp.dll";

        static SrpPInvoke()
        {
            Environment.SetEnvironmentVariable("GODEBUG", "cgocheck=0");

            var handle = LoadLibrary(BinaryPath);
            if (handle == IntPtr.Zero)
            {
                throw new DllNotFoundException($"Failed to load binary: {BinaryPath}.");
            }
        }

        static string BinaryPath
        {
            get
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    return $"x64\\{binaryName}";
                }

                return $"x86\\{binaryName}";
            }
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        public struct GoString
        {
            public IntPtr p;
            public IntPtr n;

            public void Free()
            {
                Marshal.FreeHGlobal(p);
            }
        }

        public struct GoSlice
        {
            public IntPtr data;
            public int len;
            public int cap;
        }

        public static byte[] ConvertToBytes(this GoSlice goBytes)
        {
            var bytes = new byte[goBytes.len];
            for (var i = 0; i < goBytes.len; i++)
            {
                bytes[i] = Marshal.ReadByte(goBytes.data, i);
            }

            return bytes;
        }

        public static GoString ToGoString(this string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
            return new GoString
            {
                p = nativeUtf8,
                n = (IntPtr) buffer.Length
            };
        }

        public class GoProofs
        {
            public byte[] ClientProof;
            public byte[] ClientEphemeral;
            public byte[] ExpectedServerProof;
        }

        public static GoProofs GenerateProofs(int version, string username, string password, string salt, string signedModulus, string serverEphemeral, int bitLength = 2048)
        {
            var goUsername = username.ToGoString();
            var goPassword = password.ToGoString();
            var goSalt = salt.ToGoString();
            var goModulus = signedModulus.ToGoString();
            var goEphemeral = serverEphemeral.ToGoString();
            var outBytes = NativeGenerateProofs(version, goUsername, goPassword, goSalt, goModulus, goEphemeral, bitLength);
            var bytes = outBytes.ConvertToBytes();

            using (var memStream = new MemoryStream(bytes))
            {
                var reader = new BinaryReader(memStream);
                reader.ReadByte();
                var type = reader.ReadByte();

                if (type == 0)
                {
                    var size = reader.ReadUInt16();
                    var bmsg = reader.ReadBytes(size);
                    throw new Exception("go-srp: " + Encoding.UTF8.GetString(bmsg));
                }

                if (type == 1)
                {
                    UInt16 size = reader.ReadUInt16();
                    byte[] clientProof = reader.ReadBytes(size);
                    size = reader.ReadUInt16();
                    byte[] clientEphemeral = reader.ReadBytes(size);
                    size = reader.ReadUInt16();
                    byte[] expectedServerProof = reader.ReadBytes(size);

                    return new GoProofs
                    {
                        ClientProof = clientProof,
                        ClientEphemeral = clientEphemeral,
                        ExpectedServerProof = expectedServerProof
                    };
                }
            }

            return null;
        }

        [DllImport("GoSrp", EntryPoint = "GenerateProofs", CallingConvention = CallingConvention.Cdecl)]
        private static extern GoSlice NativeGenerateProofs(int version, GoString username, GoString password, GoString salt, GoString signedModulus, GoString serverEphemeral, int bits);
    }
}
