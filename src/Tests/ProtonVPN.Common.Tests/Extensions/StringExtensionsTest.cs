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
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Common.Tests.Extensions
{
    [TestClass]
    public class StringExtensionsTest
    {
        [DataTestMethod]
        [DataRow(false, "false")]
        [DataRow(false, "FALSE")]
        [DataRow(true, "true")]
        [DataRow(true, "TRUE")]
        [DataRow(null, null)]
        [DataRow(null, "")]
        [DataRow(null, " ")]
        [DataRow(null, "0")]
        [DataRow(null, "1")]
        [DataRow(null, "falso")]
        public void TestToBoolOrNull(bool? expectedResult, string input)
        {
            bool? result = StringExtensions.ToBoolOrNull(input);
            Assert.AreEqual(expectedResult, result);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void TestSplitToEnumerable_ReturnsNull(string input)
        {
            IEnumerable<string> result = StringExtensions.SplitToEnumerable(input, '.');
            Assert.IsNull(result);
        }

        [DataTestMethod]
        [DataRow(',')]
        [DataRow(';')]
        [DataRow('.')]
        [DataRow(':')]
        [DataRow('-')]
        public void TestSplitToEnumerable_ForOne(char separator)
        {
            string input = "protonvpn";
            List<string> expectedResult = new() { input };

            IEnumerable<string> result = StringExtensions.SplitToEnumerable(input, separator);

            AssertEnumerable(expectedResult, result);
        }

        private void AssertEnumerable(IEnumerable<string> expectedResult, IEnumerable<string> result)
        {
            if (expectedResult is null)
            {
                Assert.IsNull(result);
            }
            else
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(expectedResult.Count(), result.Count());
                IEnumerator<string> expectedEnumerator = expectedResult.GetEnumerator();
                IEnumerator<string> enumerator = result.GetEnumerator();
                while (expectedEnumerator.MoveNext() && enumerator.MoveNext())
                {
                    Assert.AreEqual(expectedEnumerator.Current, enumerator.Current);
                }
            }
        }

        [DataTestMethod]
        [DataRow(',')]
        [DataRow(';')]
        [DataRow('.')]
        [DataRow(':')]
        [DataRow('-')]
        public void TestSplitToEnumerable_ForMultiple(char separator)
        {
            string input = $" inbox {separator} VPN {separator} Calendar {separator} Drive {separator} 9455";
            List<string> expectedResult = new() { "inbox", "VPN", "Calendar", "Drive", "9455" };

            IEnumerable<string> result = StringExtensions.SplitToEnumerable(input, separator);

            AssertEnumerable(expectedResult, result);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void TestSplitToHashSet_ReturnsNull(string input)
        {
            HashSet<string> result = StringExtensions.SplitToHashSet(input, '.');
            Assert.IsNull(result);
        }

        [DataTestMethod]
        [DataRow(',')]
        [DataRow(';')]
        [DataRow('.')]
        [DataRow(':')]
        [DataRow('-')]
        public void TestSplitToHashSet_ForOne(char separator)
        {
            string input = "protonvpn";
            HashSet<string> expectedResult = new() { input };

            HashSet<string> result = StringExtensions.SplitToHashSet(input, separator);

            AssertEnumerable(expectedResult, result);
        }

        [DataTestMethod]
        [DataRow(',')]
        [DataRow(';')]
        [DataRow('.')]
        [DataRow(':')]
        [DataRow('-')]
        public void TestSplitToHashSet_ForMultiple(char separator)
        {
            string input = $" inbox {separator} VPN {separator} Calendar {separator} Drive {separator} 9455";
            HashSet<string> expectedResult = new() { "inbox", "VPN", "Calendar", "Drive", "9455" };

            HashSet<string> result = StringExtensions.SplitToHashSet(input, separator);

            AssertEnumerable(expectedResult, result);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void TestSplitToList_ReturnsNull(string input)
        {
            List<string> result = StringExtensions.SplitToList(input, '.');
            Assert.IsNull(result);
        }

        [DataTestMethod]
        [DataRow(',')]
        [DataRow(';')]
        [DataRow('.')]
        [DataRow(':')]
        [DataRow('-')]
        public void TestSplitToList_ForOne(char separator)
        {
            string input = "protonvpn";
            List<string> expectedResult = new() { input };

            List<string> result = StringExtensions.SplitToList(input, separator);

            AssertEnumerable(expectedResult, result);
        }

        [DataTestMethod]
        [DataRow(',')]
        [DataRow(';')]
        [DataRow('.')]
        [DataRow(':')]
        [DataRow('-')]
        public void TestSplitToList_ForMultiple(char separator)
        {
            string input = $" inbox {separator} VPN {separator} Calendar {separator} Drive {separator} 9455";
            List<string> expectedResult = new() { "inbox", "VPN", "Calendar", "Drive", "9455" };

            List<string> result = StringExtensions.SplitToList(input, separator);

            AssertEnumerable(expectedResult, result);
        }

        [DataTestMethod]
        [DataRow("https://192.168.1.1/", "192.168.1.1")]
        [DataRow("https://protonvpn.com/", "protonvpn.com")]
        [DataRow("http://protonvpn.com/", "http://protonvpn.com")]
        [DataRow("https://account.proton.me/", "account.proton.me")]
        [DataRow("https://account.proton.me/switch", "https://account.proton.me/switch")]
        public void TestIsHttpUri(string expectedUriString, string input)
        {
            bool isUri = StringExtensions.IsHttpUri(input, out Uri uri);

            Assert.IsTrue(isUri);
            Assert.IsNotNull(uri);
            Assert.AreEqual(expectedUriString, uri.AbsoluteUri);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(".")]
        public void TestIsHttpUri_ReturnsFalse(string input)
        {
            bool isUri = StringExtensions.IsHttpUri(input, out Uri uri);

            Assert.IsFalse(isUri);
        }
    }
}