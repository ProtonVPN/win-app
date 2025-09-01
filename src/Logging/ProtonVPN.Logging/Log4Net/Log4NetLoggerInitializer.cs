/*
 * Copyright (c) 2025 Proton AG
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

using System.Text;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Logging.Log4Net
{
    public abstract class Log4NetLoggerInitializer
    {
        private readonly ILoggerConfiguration _loggerConfiguration;

        protected ILog InternalLogger { get; private set; }

        public Log4NetLoggerInitializer(ILoggerConfiguration loggerConfiguration)
        {
            _loggerConfiguration = loggerConfiguration;
            Configure();
            InternalLogger = LogManager.GetLogger("ProtonVpnLogger");
        }

        private void Configure()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            hierarchy.Root.RemoveAllAppenders();
            RollingFileAppender roller = CreateRollingFileAppender();

            SetLoggerLevel(hierarchy);

            hierarchy.Configured = true;
            BasicConfigurator.Configure(hierarchy, roller);
        }

        private RollingFileAppender CreateRollingFileAppender()
        {
            PatternLayout patternLayout = new("%utcdate{yyyy-MM-ddTHH:mm:ss.fffZ} | %-5level | %message%newline%exception");
            HangingIndentLayout indentingLayout = new(patternLayout);

            RollingFileAppender roller = new();
            roller.File = _loggerConfiguration.FullFilePath;
            roller.AppendToFile = true;
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.MaxSizeRollBackups = 1;
            roller.MaximumFileSize = "400KB";
            roller.StaticLogFileName = true;
            roller.Encoding = Encoding.UTF8;
            roller.PreserveLogFileNameExtension = true;
            roller.Layout = indentingLayout;
            roller.ActivateOptions();
            return roller;
        }

        private void SetLoggerLevel(Hierarchy hierarchy)
        {
#if DEBUG
            hierarchy.Root.Level = Level.All;
#else
            hierarchy.Root.Level = Level.Info;
#endif
        }
    }
}