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
using OtpNet;

namespace ProtonVPN.UI.Tests.TestsHelper
{
    public class TestUserData
    {
        public string Username { get; set; }
        public string Password { get; set; }

        private TestUserData(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public static TestUserData GetFreeUser()
        {
            (string username, string password) = GetUsernameAndPassword("FREE_USER");
            return new TestUserData(username, password);
        }

        public static TestUserData GetUserWithSpecialChars()
        {
            (string username, string password) = GetUsernameAndPassword("SPECIAL_CHARS_USER");
            return new TestUserData(username, password);
        }

        public static TestUserData GetPlusUser()
        {
            (string username, string password) = GetUsernameAndPassword("PLUS_USER");
            return new TestUserData(username, password);
        }

        public static TestUserData GetVisionaryUser()
        {
            (string username, string password) = GetUsernameAndPassword("VISIONARY_USER");
            return new TestUserData(username, password);
        }

        public static TestUserData GetTwoPassUser()
        {
            (string username, string password) = GetUsernameAndPassword("TWO_PASS_USER");
            return new TestUserData(username, password);
        }
        public static TestUserData GetZeroAssignedConnectionUser()
        {
            (string username, string password) = GetUsernameAndPassword("ZERO_CONNECTIONS_USER");
            return new TestUserData(username, password);
        }

        public static TestUserData GetIncorrectCredentialsUser()
        {
            return new TestUserData("IncorrectUsername", "IncorrectPass");
        }

        public static TestUserData GetTwoFactorUser()
        {
            (string username, string password) = GetUsernameAndPassword("TWO_FACTOR_AUTH_USER");
            return new TestUserData(username, password);
        }

        public static string GetTwoFactorCode()
        {
            string key = Environment.GetEnvironmentVariable("TWO_FA_KEY");
            Totp totp = new Totp(Base32Encoding.ToBytes(key));
            return totp.ComputeTotp();
        }

        private static (string, string) GetUsernameAndPassword(string userType)
        {
            string str = Environment.GetEnvironmentVariable(userType);
            if (string.IsNullOrEmpty(str))
            {
                throw new Exception($"Missing environment variable: {userType}");
            }

            string[] split = str.Split(':');
            return (split[0], split[1]);
        }
    }
}
