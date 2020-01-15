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
using NSubstitute;
using ProtonVPN.Common.Logging;
using System;
using System.IO;
using System.Linq;

namespace ProtonVPN.Common.Test.Logging
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
            var cleaner = new LogCleaner(_logger);
            // Act
            Action action = () => cleaner.Clean(logPath, 0);
            //Assert
            action.Should().NotThrow<Exception>();
        }

        [TestMethod]
        public void Clean_ShouldDeleteOldestFiles()
        {
            // Arrange
            const string logPath = nameof(Clean_ShouldDeleteOldestFiles);
            CreateEmptyDirectory(logPath);
            var files = new[] {"file.log", "file1.log", "file2.log", "file3.log", "file4.log"}.Select(f => Path.Combine(logPath, f)).ToList();
            files.ForEach(CreateEmptyFile);
            File.SetCreationTimeUtc(files[0], new DateTime(2019, 03, 15, 10, 50, 0));
            File.SetCreationTimeUtc(files[1], new DateTime(2019, 03, 15, 10, 40, 0));
            File.SetCreationTimeUtc(files[2], new DateTime(2019, 03, 15, 10, 30, 0));
            File.SetCreationTimeUtc(files[3], new DateTime(2019, 03, 15, 10, 20, 0));
            File.SetCreationTimeUtc(files[4], new DateTime(2019, 03, 15, 10, 10, 0));
            var cleaner = new LogCleaner(_logger);

            // Act
            cleaner.Clean(logPath, 2);
            var result = Directory.GetFiles(logPath).Select(Path.GetFileName);
            
            // Assert
            result.Should().HaveCount(2)
                .And.Contain(new[] {"file.log", "file1.log"});
        }

        [TestMethod]
        public void Clean_ShouldSkipLockedFile()
        {
            // Arrange
            const string logPath = nameof(Clean_ShouldSkipLockedFile);
            CreateEmptyDirectory(logPath);
            var files = new[] { "file2.log", "file1.log", "file.log" }.Select(f => Path.Combine(logPath, f)).ToList();
            files.ForEach(CreateEmptyFile);
            File.SetCreationTimeUtc(files[0], new DateTime(2000, 01, 01, 0, 10, 0));
            File.SetCreationTimeUtc(files[1], new DateTime(2000, 01, 01, 0, 20, 0));
            File.SetCreationTimeUtc(files[2], new DateTime(2000, 01, 01, 0, 30, 0));
            var cleaner = new LogCleaner(_logger);

            // Act
            using (File.OpenRead(files[1]))
            {
                cleaner.Clean(logPath, 0);
            }
            var result = Directory.GetFiles(logPath).Select(Path.GetFileName);

            // Assert
            result.Should().HaveCount(1)
                .And.Contain(new[] { "file1.log" });
        }

        #region Helpers

        private static void CreateEmptyDirectory(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(path);
        }

        private static void CreateEmptyFile(string filename)
        {
            File.Create(filename).Dispose();
        }

        #endregion
    }
}
