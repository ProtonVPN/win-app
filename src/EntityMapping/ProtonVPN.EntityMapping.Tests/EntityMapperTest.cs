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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Logging;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.EntityMapping.Tests.Mocks;

namespace ProtonVPN.EntityMapping.Tests
{
    [TestClass]
    public class EntityMapperTest
    {
        private ILogger _logger;
        private Lazy<IEnumerable<IMapper>> _mappers;
        private EntityMapper _entityMapper;

        [TestInitialize]
        public void Initialize()
        {
            _logger = Substitute.For<ILogger>();
            _mappers = new Lazy<IEnumerable<IMapper>>(CreateMappers);
            _entityMapper = new EntityMapper(_logger, _mappers);
        }

        private IEnumerable<IMapper> CreateMappers()
        {
            yield return new MockOfEntityMapper();
            yield return new MockOfEnumMapper();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _logger = null;
            _mappers = null;
            _entityMapper = null;
        }

        [TestMethod]
        public void TestMap_Entity()
        {
            string value = DateTimeOffset.UtcNow.ToString();
            MockOfEntityA mockOfEntityA = new() { Value = value.ToString() };

            MockOfEntityB result = _entityMapper.Map<MockOfEntityA, MockOfEntityB>(mockOfEntityA);

            Assert.IsNotNull(result);
            Assert.AreEqual(value, result.Value);
        }

        [TestMethod]
        public void TestMap_Entity_Inverse()
        {
            string value = DateTimeOffset.UtcNow.ToString();
            MockOfEntityB mockOfEntityB = new() { Value = value.ToString() };

            MockOfEntityA result = _entityMapper.Map<MockOfEntityB, MockOfEntityA>(mockOfEntityB);

            Assert.IsNotNull(result);
            Assert.AreEqual(value, result.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestMap_WhenMapperDoesNotExist()
        {
            string value = DateTimeOffset.UtcNow.ToString();
            MockOfEntityA mockOfEntityA = new() { Value = value.ToString() };

            _entityMapper.Map<MockOfEntityA, MockOfEnumA>(mockOfEntityA);
        }

        [TestMethod]
        public void TestMap_Enum()
        {
            MockOfEnumA mockOfEnumA = MockOfEnumA.One;

            MockOfEnumB result = _entityMapper.Map<MockOfEnumA, MockOfEnumB>(mockOfEnumA);

            Assert.IsNotNull(result);
            Assert.AreEqual(MockOfEnumB.One, result);
        }

        [TestMethod]
        public void TestMap_Enum_Inverse()
        {
            MockOfEnumB mockOfEnumB = MockOfEnumB.Two;

            MockOfEnumA result = _entityMapper.Map<MockOfEnumB, MockOfEnumA>(mockOfEnumB);

            Assert.IsNotNull(result);
            Assert.AreEqual(MockOfEnumA.Two, result);
        }

        [TestMethod]
        [DataRow(MockOfEnumB.Two, MockOfEnumA.Two)]
        [DataRow(null, null)]
        public void TestMapNullableStruct(MockOfEnumB? expectedResult, MockOfEnumA? mockOfEnumA)
        {
            MockOfEnumB? result = _entityMapper.MapNullableStruct<MockOfEnumA, MockOfEnumB>(mockOfEnumA);

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        [DataRow(MockOfEnumA.One, MockOfEnumB.One)]
        [DataRow(null, null)]
        public void TestMapNullableStruct_Inverse(MockOfEnumA? expectedResult, MockOfEnumB? mockOfEnumB)
        {
            MockOfEnumA? result = _entityMapper.MapNullableStruct<MockOfEnumB, MockOfEnumA>(mockOfEnumB);

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestMapNullableStruct_WhenMapperDoesNotExist()
        {
            MockOfEnumA mockOfEnumA = MockOfEnumA.One;

            _entityMapper.Map<MockOfEnumA, DateTimeKind>(mockOfEnumA);
        }

        [TestMethod]
        public void TestMap_ListOfEntities()
        {
            List<MockOfEntityA> mockOfAEntities = GenerateEntities<MockOfEntityA>(
                (int i) => new() { Value = DateTimeOffset.UtcNow.AddDays(i).ToString() });

            List<MockOfEntityB> result = _entityMapper.Map<MockOfEntityA, MockOfEntityB>(mockOfAEntities.ToList());

            AssertListsAreEqual(mockOfAEntities, result,
                (MockOfEntityA mockOfEntityA, MockOfEntityB mockOfEntityB) => mockOfEntityA.Value == mockOfEntityB.Value);
        }

        private List<T> GenerateEntities<T>(Func<int, T> generator)
        {
            List<T> entities = new();
            for (int i = 0; i < 9; i++)
            {
                entities.Add(generator(i));
            }
            return entities;
        }

        private void AssertListsAreEqual<TA, TB>(List<TA> expectedResult, List<TB> result, Func<TA, TB, bool> comparer)
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult.Count, result.Count);
            for (int i = 0; i < expectedResult.Count; i++)
            {
                Assert.IsTrue(comparer(expectedResult[i], result[i]));
            }
        }

        [TestMethod]
        public void TestMap_ListOfEntities_Inverse()
        {
            List<MockOfEntityB> mockOfBEntities = GenerateEntities<MockOfEntityB>(
                (int i) => new() { Value = DateTimeOffset.UtcNow.AddDays(i).ToString() });

            List<MockOfEntityA> result = _entityMapper.Map<MockOfEntityB, MockOfEntityA>(mockOfBEntities.ToList());

            AssertListsAreEqual(mockOfBEntities, result,
                (MockOfEntityB mockOfEntityB, MockOfEntityA mockOfEntityA) => mockOfEntityB.Value == mockOfEntityA.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestMap_List_WhenMapperDoesNotExist()
        {
            List<MockOfEntityA> mockOfAEntities = GenerateEntities<MockOfEntityA>(
                (int i) => new() { Value = DateTimeOffset.UtcNow.AddDays(i).ToString() });

            _entityMapper.Map<MockOfEntityA, MockOfEnumA>(mockOfAEntities.ToList());
        }

        [TestMethod]
        public void TestMap_ListOfEnums()
        {
            List<MockOfEnumA> mockOfAEnums = GenerateEnums<MockOfEnumA>((int i) => (MockOfEnumA)i);

            List<MockOfEnumB> result = _entityMapper.Map<MockOfEnumA, MockOfEnumB>(mockOfAEnums.ToList());

            AssertListsAreEqual(mockOfAEnums, result,
                (MockOfEnumA mockOfEnumA, MockOfEnumB mockOfEnumB) => (int)mockOfEnumA == (int)mockOfEnumB);
        }

        private List<T> GenerateEnums<T>(Func<int, T> generator)
        {
            List<T> entities = new();
            for (int i = 0; i < 3; i++)
            {
                entities.Add(generator(i));
            }
            return entities;
        }

        [TestMethod]
        public void TestMap_ListOfEnums_Inverse()
        {
            List<MockOfEnumB> mockOfBEnums = GenerateEnums<MockOfEnumB>((int i) => (MockOfEnumB)i);

            List<MockOfEnumA> result = _entityMapper.Map<MockOfEnumB, MockOfEnumA>(mockOfBEnums.ToList());

            AssertListsAreEqual(mockOfBEnums, result,
                (MockOfEnumB mockOfEnumB, MockOfEnumA mockOfEnumA) => (int)mockOfEnumB == (int)mockOfEnumA);
        }
    }
}