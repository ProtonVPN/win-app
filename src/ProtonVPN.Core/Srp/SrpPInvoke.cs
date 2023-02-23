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
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using ProtonVPN.Common.Go;

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

            using (GoString goUsername = username.ToGoString())
            using (DisposableGoBytes goPassword = password.ToDisposableGoBytes())
            using (GoString goSalt = salt.ToGoString())
            using (GoString goModulus = signedModulus.ToGoString())
            using (GoString goEphemeral = serverEphemeral.ToGoString())
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
        private static extern GoBytes NativeGenerateProofs(int version, GoString username, DisposableGoBytes password,
            GoString salt, GoString signedModulus, GoString serverEphemeral, int bits);

        [DllImport("GoSrp", EntryPoint = "SetTest", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetUnitTest();
    }
}