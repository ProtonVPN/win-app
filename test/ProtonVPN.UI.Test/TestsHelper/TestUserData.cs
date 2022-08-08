/*
 * Copyright (c) 2022 Proton Technologies AG
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

namespace ProtonVPN.UI.Test.TestsHelper
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
            var (username, password) = GetUsernameAndPassword("FREE_USER");
            return new TestUserData(username, password);
        }

        public static TestUserData GetUserWithSpecialChars()
        {
            var (username, password) = GetUsernameAndPassword("SPECIAL_CHARS_USER");
            return new TestUserData(username, password);
        }

        public static TestUserData GetPlusUser()
        {
            var (username, password) = GetUsernameAndPassword("PLUS_USER");
            return new TestUserData(username, password);
        }

        public static TestUserData GetVisionaryUser()
        {
            var (username, password) = GetUsernameAndPassword("VISIONARY_USER");
            return new TestUserData(username, password);
        }

        public static TestUserData GetTestrailUser()
        {
            var (username, password) = GetUsernameAndPassword("TESTRAIL_USER");
            return new TestUserData(username, password);
        }

        private static (string, string) GetUsernameAndPassword(string userType)
        {
            var str = Environment.GetEnvironmentVariable(userType);
            if (string.IsNullOrEmpty(str))
            {
                throw new Exception($"Missing environment variable: {userType}");
            }

            var split = str.Split(':');
            return (split[0], split[1]);
        }
    }
}
