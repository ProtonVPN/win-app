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
using ProtonVPN.BugReporting.Attachments.Sources;
using ProtonVPN.Tests.Common;

namespace ProtonVPN.App.Tests.BugReporting.Attachments.Source
{
    [TestClass]
    public abstract class LogFileSourceBaseTest<T>
        where T : LogFileSourceBase
    {
        protected const long MAX_FILE_SIZE = 50 * 1024;

        [TestMethod]
        public void Get_ShouldBe_FileNamesFrom_Directory()
        {
            // Arrange
            string folderPath = TestConfig.GetFolderPath();
            string[] fileNames = { "Log 1.txt", "Log 2.txt", "Log 3.txt" };
            PrepareFiles(folderPath, fileNames);
            T fileSource = Construct(folderPath, 5);

            // Act
            List<string> result = fileSource.Get().Select(Path.GetFileName).ToList();

            // Assert
            result.Should().Contain(fileNames);
        }

        private static void PrepareFiles(string folderName, IEnumerable<string> fileNames)
        {
            if (Directory.Exists(folderName))
            {
                Directory.Delete(folderName, true);
            }

            foreach (string filename in fileNames)
            {
                CopyFile("bug-report-test.txt", folderName, filename);
            }
        }

        private static void CopyFile(string sourcePath, string destPath, string newFilename = null)
        {
            if (!string.IsNullOrEmpty(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            string filename = !string.IsNullOrEmpty(newFilename) ? newFilename : Path.GetFileName(sourcePath);
            string destFullPath = Path.Combine(destPath ?? "", filename ?? "");

            File.Copy(TestConfig.GetFolderPath(sourcePath), destFullPath);
        }

        protected abstract T Construct(string folderPath, int maxNumOfFiles);

        [TestMethod]
        public void Get_ShouldTake_OnlyCountOfFiles()
        {
            // Arrange
            string folderPath = TestConfig.GetFolderPath();
            string[] fileNames = { "Log 1.txt", "Log 2.txt", "Log 3.txt" };
            PrepareFiles(folderPath, fileNames);
            T fileSource = Construct(folderPath, 2);

            // Act
            List<string> result = fileSource.Get().ToList();

            // Assert
            result.Should().HaveCount(2);
        }

        [TestMethod]
        public void Get_ShouldBe_OrderedBy_LastWriteTime()
        {
            // Arrange
            string folderPath = TestConfig.GetFolderPath();
            string[] fileNames = { "Log 1.txt", "Log 2.txt", "Log 3.txt" };
            PrepareFiles(folderPath, fileNames);
            File.SetLastWriteTimeUtc(Path.Combine(folderPath, "Log 1.txt"), new DateTime(2020, 05, 15, 0, 3, 3));
            File.SetLastWriteTimeUtc(Path.Combine(folderPath, "Log 2.txt"), new DateTime(2020, 05, 15, 0, 1, 1));
            File.SetLastWriteTimeUtc(Path.Combine(folderPath, "Log 3.txt"), new DateTime(2020, 05, 15, 0, 2, 2));
            T fileSource = Construct(folderPath, 3);

            // Act
            List<string> result = fileSource.Get().Select(Path.GetFileName).ToList();

            // Assert
            result.Should().ContainInOrder("Log 1.txt", "Log 3.txt", "Log 2.txt");
        }

        [TestMethod]
        public void Get_ShouldTake_OnlyUnderSizeLimit()
        {
            // Arrange
            string folderPath = TestConfig.GetFolderPath();
            string[] fileNames = { "Log 1.txt", "Log 2.txt", "tooBigFile.txt", "Log 3.txt" };
            PrepareFiles(folderPath, fileNames);
            File.WriteAllBytes(Path.Combine(folderPath, "tooBigFile.txt"), new byte[MAX_FILE_SIZE + 1]);
            T fileSource = Construct(folderPath, 4);

            // Act
            List<string> result = fileSource.Get().Select(Path.GetFileName).ToList();

            // Assert
            result.Contains("tooBigFile.txt").Should().BeFalse();
        }
    }
}