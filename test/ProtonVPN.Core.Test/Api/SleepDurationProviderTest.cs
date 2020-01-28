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
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using ProtonVPN.Core.Api;

namespace ProtonVPN.Core.Test.Api
{
    [TestClass]
    public class SleepDurationProviderTest
    {
        [TestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(15)]
        [DataRow(100)]
        public void Value_ShouldBeNotHigherThan10(int delta)
        {
            GetProvider(delta).Value().Seconds.Should().BeLessOrEqualTo(10);
        }

        private SleepDurationProvider GetProvider(int seconds)
        {
            var message = new HttpResponseMessage();
            message.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(seconds));
            var response = new DelegateResult<HttpResponseMessage>(message);
            return new SleepDurationProvider(response);
        }
    }
}
