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

using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProtonVPN.Crypto.Tests
{
    [TestClass]
    public class HashGeneratorTest
    {
        [TestMethod]
        [DataRow(2008619323U, "SW3WCX7MDVPP73AGH5350FZ1CKWCQFK8N458HSYYXFQMH3P22PZ03.1.0")]
        [DataRow(2502136392U, "SW3WCX7MDVPP73AGH5350FZ1CKWCQFK8N458HSYYXFQMH3P22PZ03.0.5")]
        [DataRow(3775902997U, "SW3WCX7MDVPP73AGH5350FZ1CKWCQFK8N458HSYYXFQMH3P22PZ03.0.7")]
        [DataRow(4122662162U, "SW3WCX7MDVPP73AGH5350FZ1CKWCQFK8N458HSYYXFQMH3P22PZ03.1.1")]
        [DataRow(3292430320U, "SW3WCX7MDVPP73AGH5350FZ1CKWCQFK8N458HSYYXFQMH3P22PZ04.0.0")]
        [DataRow(1790419583U, "GZ69SVHZ9J0R233PD876QVWDFGWEFGQPX14V9EH8E9JDCXQ8MN403.1.0")]
        [DataRow(  49919467U, "GZ69SVHZ9J0R233PD876QVWDFGWEFGQPX14V9EH8E9JDCXQ8MN403.0.5")]
        [DataRow(4285208048U, "GZ69SVHZ9J0R233PD876QVWDFGWEFGQPX14V9EH8E9JDCXQ8MN403.0.7")]
        [DataRow(1231401386U, "GZ69SVHZ9J0R233PD876QVWDFGWEFGQPX14V9EH8E9JDCXQ8MN403.1.1")]
        [DataRow(2387756961U, "GZ69SVHZ9J0R233PD876QVWDFGWEFGQPX14V9EH8E9JDCXQ8MN404.0.0")]        
        public void TestHashToUint(uint expectedResult, string text)
        {
            uint result = HashGenerator.HashToUint(text);

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        [DataRow("0.4676681299385773320539336028", "SW3WCX7MDVPP73AGH5350FZ1CKWCQFK8N458HSYYXFQMH3P22PZ03.1.0")]
        [DataRow("0.5825740267947721357445167694", "SW3WCX7MDVPP73AGH5350FZ1CKWCQFK8N458HSYYXFQMH3P22PZ03.0.5")]
        [DataRow("0.8791459253707774741041421597", "SW3WCX7MDVPP73AGH5350FZ1CKWCQFK8N458HSYYXFQMH3P22PZ03.0.7")]
        [DataRow("0.9598820849694037076480229636", "SW3WCX7MDVPP73AGH5350FZ1CKWCQFK8N458HSYYXFQMH3P22PZ03.1.1")]
        [DataRow("0.7665786707695989568646994785", "SW3WCX7MDVPP73AGH5350FZ1CKWCQFK8N458HSYYXFQMH3P22PZ04.0.0")]
        [DataRow("0.4168645440174416974227506894", "GZ69SVHZ9J0R233PD876QVWDFGWEFGQPX14V9EH8E9JDCXQ8MN403.1.0")]
        [DataRow("0.0116227816351742440916537876", "GZ69SVHZ9J0R233PD876QVWDFGWEFGQPX14V9EH8E9JDCXQ8MN403.0.5")]
        [DataRow("0.9977277482388838539456212553", "GZ69SVHZ9J0R233PD876QVWDFGWEFGQPX14V9EH8E9JDCXQ8MN403.0.7")]
        [DataRow("0.2867079773654015682091474459", "GZ69SVHZ9J0R233PD876QVWDFGWEFGQPX14V9EH8E9JDCXQ8MN403.1.1")]
        [DataRow("0.555942990248078245261702278",  "GZ69SVHZ9J0R233PD876QVWDFGWEFGQPX14V9EH8E9JDCXQ8MN404.0.0")]
        public void TestHashToPercentage(string expectedResult, string text)
        {
            decimal result = HashGenerator.HashToPercentage(text);

            Assert.AreEqual(decimal.Parse(expectedResult, CultureInfo.InvariantCulture), result);
        }
    }
}