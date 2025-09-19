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

using ProtonVPN.OperatingSystems.WebAuthn.Enums;

namespace ProtonVPN.OperatingSystems.WebAuthn.Tests.Enums;

[TestClass]
public class UserVerificationParserTest
{
    [TestMethod]
    [DataRow(UserVerificationRequirement.Preferred, "preferred")]
    [DataRow(UserVerificationRequirement.Preferred, "Preferred")]
    [DataRow(UserVerificationRequirement.Preferred, "PREFERRED")]
    [DataRow(UserVerificationRequirement.Required, "required")]
    [DataRow(UserVerificationRequirement.Required, "Required")]
    [DataRow(UserVerificationRequirement.Required, "REQUIRED")]
    [DataRow(UserVerificationRequirement.Discouraged, "discouraged")]
    [DataRow(UserVerificationRequirement.Discouraged, "Discouraged")]
    [DataRow(UserVerificationRequirement.Discouraged, "DISCOURAGED")]
    // Empty or null
    [DataRow(UserVerificationRequirement.Discouraged, "NULL")]
    [DataRow(UserVerificationRequirement.Discouraged, "")]
    [DataRow(UserVerificationRequirement.Discouraged, " ")]
    [DataRow(UserVerificationRequirement.Discouraged, null)]
    // Typos
    [DataRow(UserVerificationRequirement.Discouraged, "prefered")]
    [DataRow(UserVerificationRequirement.Discouraged, "require")]
    [DataRow(UserVerificationRequirement.Discouraged, "discoraged")]
    // Numeric value
    [DataRow(UserVerificationRequirement.Discouraged, "0")]
    [DataRow(UserVerificationRequirement.Required, "1")]
    [DataRow(UserVerificationRequirement.Preferred, "2")]
    [DataRow(UserVerificationRequirement.Discouraged, "3")]
    // Non-existing value
    [DataRow(UserVerificationRequirement.Discouraged, "4")]
    public void TestValue(UserVerificationRequirement expectedResult, string input)
    {
        UserVerificationRequirement result = UserVerificationParser.Parse(input);

        Assert.AreEqual(expectedResult, result);
    }
}
