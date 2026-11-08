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
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Configurations.Defaults;
using ProtonVPN.Configurations.Repositories;

namespace ProtonVPN.Configurations.Tests;

[TestClass]
public class ConfigurationTest
{
    [TestMethod]
    public void TestDefaultConfigurationsDoNotThrow()
    {

        Configuration config = CreateConfigurationWithDefaults();

        PropertyInfo[] properties = typeof(Configuration).GetProperties();
        foreach (PropertyInfo property in properties)
        {
            _ = property.GetValue(config);
        }
    }

    [TestMethod]
    public void TestRepositoryValuesDoNotThrow()
    {
        IConfigurationRepository configRepository = Substitute.For<IConfigurationRepository>();
        PropertyInfo[] properties = typeof(Configuration).GetProperties();
        foreach (PropertyInfo property in properties)
        {
            Type type = property.PropertyType;
            object? returnValue = null;
            if (type.IsValueType)
            {
                returnValue = Activator.CreateInstance(type);
            }

            configRepository
                .GetByType(property.PropertyType, property.Name)
                .Returns(returnValue);
        }
        Configuration config = new(configRepository);

        foreach (PropertyInfo property in properties)
        {
            _ = property.GetValue(config);
        }
    }

    [TestMethod]
    public void TestServerLoadUpdateIntervalDoesNotChangeBetweenReads()
    {
        Configuration config = CreateConfigurationWithDefaults();

        TimeSpan firstRead = config.ServerLoadUpdateInterval;

        for (int i = 0; i < 10; i++)
        {
            Assert.AreEqual(firstRead, config.ServerLoadUpdateInterval,
                "The interval is compared against a timestamp, so a new deviation on each read would skip or repeat a cycle.");
        }
    }

    [TestMethod]
    public void TestServerLoadUpdateIntervalKeepsItsDeviationBetweenInstances()
    {
        HashSet<TimeSpan> intervals = [];
        for (int i = 0; i < 20; i++)
        {
            intervals.Add(CreateConfigurationWithDefaults().ServerLoadUpdateInterval);
        }

        Assert.IsTrue(intervals.Count > 1, "The deviation has to stay randomized from one app instance to the next.");
        Assert.IsTrue(intervals.All(i => i >= DefaultConfiguration.ServerLoadUpdateInterval
                && i < DefaultConfiguration.ServerLoadUpdateInterval * 1.2),
            "The deviation has to stay within the configured bounds.");
    }

    private static Configuration CreateConfigurationWithDefaults()
    {
        IConfigurationRepository configRepository = Substitute.For<IConfigurationRepository>();
        configRepository
            .GetByType(Arg.Any<Type>(), Arg.Any<string>())
            .Returns(null);

        return new Configuration(configRepository);
    }
}