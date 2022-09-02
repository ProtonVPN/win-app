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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Helpers;

namespace ProtonVPN.Common.Tests.Helpers
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
            IEnumerable<int> source1 = Enumerable.Empty<int>();
            IEnumerable<int> source2 = Enumerable.Empty<int>();
            IEnumerable<int> source3 = Enumerable.Empty<int>();
            ConcatenatedSequence<int> subject = new ConcatenatedSequence<int>(source1, source2, source3);
            // Act
            List<int> result = subject.ToList();
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Enumerable_ShouldBe_ConcatenatedSources()
        {
            // Arrange
            string[] source1 = new [] { "A"  };
            string[] source2 = new [] { "B", "C", "D" };
            string[] source3 = new [] { "E", "F" };
            ConcatenatedSequence<string> subject = new ConcatenatedSequence<string>(source1, source2, source3);

            string[] expected = new[] {"A", "B", "C", "D", "E", "F"};
            // Act
            List<string> result = subject.ToList();
            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void NotGeneric_Enumerable_ShouldBe_ConcatenatedSources()
        {
            // Arrange
            string[] source1 = new[] { "A", "B", "C" };
            string[] source2 = new[] { "D", "E" };
            string[] source3 = new[] { "F" };
            ConcatenatedSequence<string> subject = new ConcatenatedSequence<string>(source1, source2, source3);

            object[] expected = new object[] { "A", "B", "C", "D", "E", "F" };
            // Act
            List<object> result = ToList(subject);
            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        #region Helpers

        private List<object> ToList(IEnumerable source)
        {
            List<object> list = new List<object>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (object item in source)
                list.Add(item);

            return list;
        }

        #endregion
    }
}
