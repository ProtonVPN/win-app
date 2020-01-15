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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ProtonVPN.Common.Test.Helpers
{
    [TestClass]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public class ConcatenatedSequenceTest
    {
        [TestMethod]
        public void ConcatenatedSequence_ShouldThrow_WhenSources_IsNull()
        {
            // Act
            Action action = () => new ConcatenatedSequence<object>(null);
            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ConcatenatedSequence_ShouldThrow_WhenSources_IsEmpty()
        {
            // Act
            Action action = () => new ConcatenatedSequence<object>();
            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Enumerable_ShouldBeEmpty_WhenAllSourcesAreEmpty()
        {
            // Arrange
            var source1 = Enumerable.Empty<int>();
            var source2 = Enumerable.Empty<int>();
            var source3 = Enumerable.Empty<int>();
            var subject = new ConcatenatedSequence<int>(source1, source2, source3);
            // Act
            var result = subject.ToList();
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Enumerable_ShouldBe_ConcatenatedSources()
        {
            // Arrange
            var source1 = new [] { "A"  };
            var source2 = new [] { "B", "C", "D" };
            var source3 = new [] { "E", "F" };
            var subject = new ConcatenatedSequence<string>(source1, source2, source3);

            var expected = new[] {"A", "B", "C", "D", "E", "F"};
            // Act
            var result = subject.ToList();
            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void NotGeneric_Enumerable_ShouldBe_ConcatenatedSources()
        {
            // Arrange
            var source1 = new[] { "A", "B", "C" };
            var source2 = new[] { "D", "E" };
            var source3 = new[] { "F" };
            var subject = new ConcatenatedSequence<string>(source1, source2, source3);

            var expected = new object[] { "A", "B", "C", "D", "E", "F" };
            // Act
            var result = ToList(subject);
            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        #region Helpers

        private List<object> ToList(IEnumerable source)
        {
            var list = new List<object>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var item in source)
                list.Add(item);

            return list;
        }

        #endregion
    }
}
