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
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Configurations.Repositories;

namespace ProtonVPN.Configurations.Tests;

[TestClass]
public class ConfigurationTest
{
    [TestMethod]
    public void TestDefaultConfigurationsDoNotThrow()
    {
        IConfigurationRepository configRepository = Substitute.For<IConfigurationRepository>();
        configRepository
            .GetByType(Arg.Any<Type>(), Arg.Any<string>())
            .Returns(null);
        Configuration config = new(configRepository);

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
}