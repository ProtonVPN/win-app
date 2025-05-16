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

namespace ProtonVPN.UI.Tests.TestsHelper;

public class TestUserData
{
    public string Username { get; set; }
    public string Password { get; set; }

    private TestUserData(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public static TestUserData FreeUser => GetUser("FREE_USER");
    public static TestUserData PlusUser => GetUser("PLUS_USER");
    public static TestUserData PlusUserBti => new TestUserData("vpnplus", "12341234");
    public static TestUserData VisionaryUser => GetUser("VISIONARY_USER");
    public static TestUserData SpecialCharsUser => GetUser("SPECIAL_CHARS_USER");
    public static TestUserData TwoPassUser => GetUser("TWO_PASS_USER");
    public static TestUserData ZeroAssignedConnectionsUser => GetUser("ZERO_CONNECTIONS_USER");
    public static TestUserData TwoFactorUser => GetUser("TWO_FACTOR_AUTH_USER");
    public static TestUserData SsoUser => GetUser("SSO_USER");
    public static TestUserData IncorrectUser => new TestUserData("IncorrectUsername", "IncorrectPass");
    public static TestUserData IncorrectUserWithWhitespace => new TestUserData(" Incorrect Username", " Incorrect Pass");

    public static string GetTwoFactorCode()
    {
        string key = Environment.GetEnvironmentVariable("TWO_FA_KEY");
        Totp totp = new Totp(Base32Encoding.ToBytes(key));
        return totp.ComputeTotp();
    }

    private static TestUserData GetUser(string envVarUser)
    {
        (string username, string password) = GetUsernameAndPassword(envVarUser);
        return new TestUserData(username, password);
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
