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
using System.Linq;
using ProtonVPN.BugReporting.Attachments.Sources;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;

namespace ProtonVPN.BugReporting.Attachments
{
    public class AttachmentsLoader : IAttachmentsLoader
    {
        private readonly ILogger _logger;
        private readonly IDiagnosticsLogFileSource _diagnosticsLogFileSource;
        private readonly IAppLogFileSource _appLogFileSource;
        private readonly IServiceLogFileSource _serviceLogFileSource;

        public AttachmentsLoader(ILogger logger, IDiagnosticsLogFileSource diagnosticsLogFileSource, 
            IAppLogFileSource appLogFileSource, IServiceLogFileSource serviceLogFileSource)
        {
            _logger = logger;
            _diagnosticsLogFileSource = diagnosticsLogFileSource;
            _appLogFileSource = appLogFileSource;
            _serviceLogFileSource = serviceLogFileSource;
        }

        public IList<Attachment> Get()
        {
            try
            {
                return GetFromFileSources()
                       .SelectMany(i => i)
                       .Select(filename => new Attachment(filename))
                       .ToList();
            }
            catch (Exception e)
            {
                _logger.Error<AppLog>("An unexpected error occurred when fetching and processing the attachments.", e);
                return new List<Attachment>();
            }
        }

        private IList<IEnumerable<string>> GetFromFileSources()
        {
            return new List<IEnumerable<string>>
            {
                _diagnosticsLogFileSource.Get(),
                _appLogFileSource.Get(),
                _serviceLogFileSource.Get()
            };
        }
    }
}