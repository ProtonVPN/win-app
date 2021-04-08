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
using System.Security;
using System.Text;

namespace ProtonVPN.Core.Srp
{
    public static class SrpPInvoke
    {
        private const string BinaryName = "GoSrp.dll";

        static SrpPInvoke()
        {
            Environment.SetEnvironmentVariable("GODEBUG", "cgocheck=0");

            IntPtr handle = LoadLibrary(BinaryPath);
            if (handle == IntPtr.Zero)
            {
                throw new DllNotFoundException($"Failed to load binary: {BinaryPath}.");
            }
        }

        private static string BinaryPath =>
            Environment.Is64BitOperatingSystem ? $"x64\\{BinaryName}" : $"x86\\{BinaryName}";

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(IntPtr dest, int size);

        public struct GoBytes : IDisposable
        {
            // Pointer to the UTF-8 encoded string buffer
            public IntPtr P;

            // Length of the string buffer in bytes
            public IntPtr N;

            // Total size of the buffer in bytes
            public IntPtr C;

            public void Dispose()
            {
                ZeroMemory(P, C.ToInt32());
                Marshal.FreeHGlobal(P);
            }
        }

        public struct GoSlice
        {
            public IntPtr Data;
            public int Len;
            public int Cap;
        }

        public static byte[] ConvertToBytes(this GoSlice goBytes)
        {
            byte[] bytes = new byte[goBytes.Len];
            for (int i = 0; i < goBytes.Len; i++)
            {
                bytes[i] = Marshal.ReadByte(goBytes.Data, i);
            }

            return bytes;
        }

        public static GoBytes ToGoBytes(this string str)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
            return new GoBytes {P = nativeUtf8, N = (IntPtr)buffer.Length, C = (IntPtr)buffer.Length};
        }

        public static unsafe GoBytes ToGoBytes(this SecureString str)
        {
            IntPtr nativeUnicode = Marshal.SecureStringToGlobalAllocUnicode(str);
            int maxLength = Encoding.UTF8.GetMaxByteCount(str.Length);
            IntPtr nativeUtf8 = Marshal.AllocHGlobal(Encoding.UTF8.GetMaxByteCount(str.Length));
            int nativeUtf8Length = Encoding.UTF8.GetBytes(
                (char*)nativeUnicode.ToPointer(),
                str.Length,
                (byte*)nativeUtf8.ToPointer(),
                maxLength);
            Marshal.ZeroFreeGlobalAllocUnicode(nativeUnicode);
            return new GoBytes {P = nativeUtf8, N = (IntPtr)nativeUtf8Length, C = (IntPtr)nativeUtf8Length};
        }

        public class GoProofs
        {
            public byte[] ClientProof;
            public byte[] ClientEphemeral;
            public byte[] ExpectedServerProof;
        }

        public static GoProofs GenerateProofs(int version, string username, SecureString password, string salt,
            string signedModulus, string serverEphemeral, int bitLength = 2048)
        {
            byte[] bytes;

            using (var goUsername = username.ToGoBytes())
            using (var goPassword = password.ToGoBytes())
            using (var goSalt = salt.ToGoBytes())
            using (var goModulus = signedModulus.ToGoBytes())
            using (var goEphemeral = serverEphemeral.ToGoBytes())
            {
                bytes = NativeGenerateProofs(version, goUsername, goPassword, goSalt, goModulus, goEphemeral, bitLength)
                    .ConvertToBytes();
            }

            using (var memStream = new MemoryStream(bytes))
            {
                var reader = new BinaryReader(memStream);
                reader.ReadByte();
                byte type = reader.ReadByte();

                if (type == 0)
                {
                    ushort size = reader.ReadUInt16();
                    byte[] bmsg = reader.ReadBytes(size);
                    throw new Exception("go-srp: " + Encoding.UTF8.GetString(bmsg));
                }

                if (type == 1)
                {
                    ushort size = reader.ReadUInt16();
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
        private static extern GoSlice NativeGenerateProofs(int version, GoBytes username, GoBytes password,
            GoBytes salt, GoBytes signedModulus, GoBytes serverEphemeral, int bits);
    }
}