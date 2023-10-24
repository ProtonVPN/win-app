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

using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Defaults;

namespace ProtonVPN.Configurations.Tests;

[TestClass]
public class DefaultConfigurationTest
{
    [TestMethod]
    public void HasAllConfigurationProperties()
    {
        PropertyInfo[] properties = typeof(IConfiguration).GetProperties(BindingFlagsConstants.PUBLIC_DECLARED_ONLY);
        PropertyInfo[] defaultProperties = typeof(DefaultConfiguration).GetProperties();
        foreach (PropertyInfo property in properties)
        {
            Assert.IsNotNull(defaultProperties.FirstOrDefault(dp => dp.Name == property.Name),
                $"The class '{nameof(DefaultConfiguration)}' is missing the property '{property.Name}' " +
                $"as it should implement all properties from '{nameof(IConfiguration)}'.");
        }
    }
}