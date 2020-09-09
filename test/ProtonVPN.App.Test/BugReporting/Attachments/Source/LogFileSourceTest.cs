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
using ProtonVPN.BugReporting.Attachments.Source;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProtonVPN.App.Test.BugReporting.Attachments.Source
{
    [TestClass]
    [DeploymentItem("BugReporting\\Attachments\\TestData", "TestData")]
    public class LogFileSourceTest
    {
        private const long MaxFileSize = 50 * 1024;

        [TestMethod]
        public void Enumerable_ShouldBe_FileNamesFrom_Directory()
        {
            // Arrange
            const string folderPath = nameof(Enumerable_ShouldBe_FileNamesFrom_Directory);
            var fileNames = new[] { "Log 1.txt", "Log 2.txt", "Log 3.txt" };
            PrepareFiles(folderPath, fileNames);
            var fileSource = new LogFileSource(MaxFileSize, folderPath, 5);
            // Act
            var result = fileSource.Select(Path.GetFileName).ToList();
            // Assert
            result.Should().Contain(fileNames);
        }

        [TestMethod]
        public void Enumerable_ShouldTake_OnlyCountOfFiles()
        {
            // Arrange
            const string folderPath = nameof(Enumerable_ShouldTake_OnlyCountOfFiles);
            var fileNames = new[] { "Log 1.txt", "Log 2.txt", "Log 3.txt" };
            PrepareFiles(folderPath, fileNames);
            var fileSource = new LogFileSource(MaxFileSize, folderPath, 2);
            // Act
            var result = fileSource.ToList();
            // Assert
            result.Should().HaveCount(2);
        }

        [TestMethod]
        public void Enumerable_ShouldBe_OrderedBy_LastWriteTime()
        {
            // Arrange
            const string folderPath = nameof(Enumerable_ShouldBe_OrderedBy_LastWriteTime);
            var fileNames = new[] { "Log 1.txt", "Log 2.txt", "Log 3.txt" };
            PrepareFiles(folderPath, fileNames);
            File.SetLastWriteTimeUtc(Path.Combine(folderPath, "Log 1.txt"), new DateTime(2020, 05, 15, 0, 3, 3));
            File.SetLastWriteTimeUtc(Path.Combine(folderPath, "Log 2.txt"), new DateTime(2020, 05, 15, 0, 1, 1));
            File.SetLastWriteTimeUtc(Path.Combine(folderPath, "Log 3.txt"), new DateTime(2020, 05, 15, 0, 2, 2));
            var fileSource = new LogFileSource(MaxFileSize, folderPath, 3);
            // Act
            var result = fileSource.Select(Path.GetFileName).ToList();
            // Assert
            result.Should().ContainInOrder("Log 1.txt", "Log 3.txt", "Log 2.txt");
        }

        [TestMethod]
        public void Enumerable_ShouldTake_OnlyUnderSizeLimit()
        {
            // Arrange
            const string folderPath = nameof(Enumerable_ShouldTake_OnlyUnderSizeLimit);
            var fileNames = new[] { "Log 1.txt", "Log 2.txt", "tooBigFile.txt", "Log 3.txt" };
            PrepareFiles(folderPath, fileNames);
            File.WriteAllBytes(Path.Combine(folderPath, "tooBigFile.txt"), new byte[MaxFileSize + 1]);
            var fileSource = new LogFileSource(MaxFileSize, folderPath, 4);

            // Act
            var result = fileSource.Select(Path.GetFileName).ToList();

            // Assert
            result.Contains("tooBigFile.txt").Should().BeFalse();
        }

        #region Helpers

        private static void PrepareFiles(string folderName, IEnumerable<string> fileNames)
        {
            if (Directory.Exists(folderName))
                Directory.Delete(folderName, true);

            foreach (var filename in fileNames)
            {
                CopyFile("test.txt", folderName, filename);
            }
        }

        private static void CopyFile(string sourcePath, string destPath, string newFilename = null)
        {
            if (!string.IsNullOrEmpty(destPath))
                Directory.CreateDirectory(destPath);

            var filename = !string.IsNullOrEmpty(newFilename) ? newFilename : Path.GetFileName(sourcePath);
            var destFullPath = Path.Combine(destPath ?? "", filename ?? "");

            File.Copy(Path.Combine("TestData", sourcePath), destFullPath);
        }

        #endregion
    }
}
