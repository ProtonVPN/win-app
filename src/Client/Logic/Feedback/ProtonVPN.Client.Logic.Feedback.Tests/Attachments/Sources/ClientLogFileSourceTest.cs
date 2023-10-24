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

using NSubstitute;
using ProtonVPN.Client.Logic.Feedback.Attachments.Sources;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Logic.Feedback.Tests.Attachments.Sources;

[TestClass]
public class ClientLogFileSourceTest : LogFileSourceBaseTest<ClientLogFileSource>
{
    protected override ClientLogFileSource Construct(string folderPath, int maxNumOfFiles)
    {
        ILogger logger = Substitute.For<ILogger>();
        IConfiguration config = Substitute.For<IConfiguration>();
        config.BugReportingMaxFileSize.Returns(MAX_FILE_SIZE);
        config.ClientLogsFolder.Returns(folderPath);
        config.MaxClientLogsAttached.Returns(maxNumOfFiles);

        return new ClientLogFileSource(logger, config);
    }
}