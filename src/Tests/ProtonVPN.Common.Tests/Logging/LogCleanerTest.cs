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
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Tests.Common;

namespace ProtonVPN.Common.Tests.Logging
{
    [TestClass]
    public class LogCleanerTest
    {
        private ILogger _logger;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
        }

        [TestMethod]
        public void Clean_ShouldNotThrow_WhenFolderNotExists()
        {
            // Arrange
            const string logPath = "Folder\\Does\\Not\\Exist";
            LogCleaner cleaner = new(_logger);
            // Act
            Action action = () => cleaner.Clean(logPath, 0);
            //Assert
            action.Should().NotThrow<Exception>();
        }

        [TestMethod]
        public void Clean_ShouldDeleteOldFiles()
        {
            DateTime lastWriteDate = DateTime.UtcNow.Subtract(LogCleaner.MAXIMUM_FILE_AGE).AddDays(-2);

            IEnumerable<string> result = ArrangeAndActByLastWriteDate(lastWriteDate);
            
            // Assert
            result.Should().HaveCount(0);
        }

        private IEnumerable<string> ArrangeAndActByLastWriteDate(DateTime lastWriteDate)
        {
            // Arrange
            string logPath = TestConfig.GetFolderPath();
            CreateEmptyDirectory(logPath);
            IList<string> files = new[] { "file.log", "file1.log", "file2.log", "file3.log", "file4.log" }.Select(f => Path.Combine(logPath, f)).ToList();
            files.ForEach(CreateEmptyFile);
            File.SetLastWriteTimeUtc(files[0], lastWriteDate.AddMinutes(50));
            File.SetLastWriteTimeUtc(files[1], lastWriteDate.AddMinutes(40));
            File.SetLastWriteTimeUtc(files[2], lastWriteDate.AddMinutes(30));
            File.SetLastWriteTimeUtc(files[3], lastWriteDate.AddMinutes(20));
            File.SetLastWriteTimeUtc(files[4], lastWriteDate.AddMinutes(10));
            LogCleaner cleaner = new(_logger);

            // Act
            cleaner.Clean(logPath, 2);
            return Directory.GetFiles(logPath).Select(Path.GetFileName);
        }

        [TestMethod]
        public void Clean_ShouldDeleteOldestFiles()
        {
            DateTime lastWriteDate = DateTime.UtcNow.Subtract(LogCleaner.MAXIMUM_FILE_AGE).AddDays(2);

            IEnumerable<string> result = ArrangeAndActByLastWriteDate(lastWriteDate);

            // Assert
            result.Should().HaveCount(2)
                .And.Contain(new[] { "file.log", "file1.log" });
        }

        [TestMethod]
        public void Clean_ShouldSkipLockedFile()
        {
            // Arrange
            string logPath = TestConfig.GetFolderPath();
            CreateEmptyDirectory(logPath);
            IList<string> files = new[] { "file2.log", "file1.log", "file.log" }.Select(f => Path.Combine(logPath, f)).ToList();
            files.ForEach(CreateEmptyFile);
            File.SetCreationTimeUtc(files[0], new(2000, 01, 01, 0, 10, 0));
            File.SetCreationTimeUtc(files[1], new(2000, 01, 01, 0, 20, 0));
            File.SetCreationTimeUtc(files[2], new(2000, 01, 01, 0, 30, 0));
            LogCleaner cleaner = new(_logger);

            // Act
            using (File.OpenRead(files[1]))
            {
                cleaner.Clean(logPath, 0);
            }
            IEnumerable<string> result = Directory.GetFiles(logPath).Select(Path.GetFileName);

            // Assert
            result.Should().HaveCount(1)
                .And.Contain(new[] { "file1.log" });
        }

        #region Helpers

        private static void CreateEmptyDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }

        private static void CreateEmptyFile(string filename)
        {
            File.Create(filename).Dispose();
        }

        #endregion
    }
}