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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Storage;

namespace ProtonVPN.Common.Tests.Storage
{
    [TestClass]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public class CollectionStorageTest
    {
        private IStorage<IEnumerable<int>> _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _origin = Substitute.For<IStorage<IEnumerable<int>>>();
        }

        [TestMethod]
        public void CollectionStorage_ShouldThrow_WhenOrigin_IsNull()
        {
            // Act
            Action action = () => new CollectionStorage<int>(null);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetAll_ShouldBeEquivalentTo_OriginGet()
        {
            // Arrange
            int[] expected = new[] {77, 256};
            _origin.Get().Returns(expected);
            CollectionStorage<int> storage = new CollectionStorage<int>(_origin);

            // Act
            IReadOnlyCollection<int> result = storage.GetAll();

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetAll_ShouldBeEmpty_WhenOrigin_Get_IsNull()
        {
            // Arrange
            _origin.Get().Returns((IEnumerable<int>)null);
            CollectionStorage<int> storage = new CollectionStorage<int>(_origin);

            // Act
            IReadOnlyCollection<int> result = storage.GetAll();

            // Assert
            result.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public void SetAll_ShouldCall_Origin_Set()
        {
            // Arrange
            int[] value = new[] { 324, 87, 132 };
            CollectionStorage<int> storage = new CollectionStorage<int>(_origin);

            // Act
            storage.SetAll(value);

            // Assert
            _origin.Received().Set(value);
        }
    }
}
