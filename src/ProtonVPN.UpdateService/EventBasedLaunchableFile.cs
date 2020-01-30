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

using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Event;
using ProtonVPN.Update.Files.Launchable;
using Sentry;
using Sentry.Protocol;
using System;
using System.IO;

namespace ProtonVPN.UpdateService
{
    public class EventBasedLaunchableFile : ILaunchableFile
    {
        private readonly SystemEventLog _systemEventLog;
        private readonly Config _config;
        private const int LaunchFileEventId = 1;

        public EventBasedLaunchableFile(SystemEventLog systemEventLog, Config config)
        {
            _config = config;
            _systemEventLog = systemEventLog;
        }

        public void Launch(string filename, string arguments)
        {
            try
            {
                File.WriteAllText(_config.UpdateFilePath, $"{filename}\n{arguments}");

                _systemEventLog.Log("Update app", LaunchFileEventId);
            }
            catch (Exception e)
            {
                SentrySdk.WithScope(scope =>
                {
                    scope.Level = SentryLevel.Error;
                    scope.SetTag("captured_in", "UpdateService_EventBasedLaunchableFile_Launch");
                    SentrySdk.CaptureException(e);
                });
            }
        }
    }
}
