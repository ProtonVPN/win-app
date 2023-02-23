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
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.BugReporting.Actions;
using ProtonVPN.BugReporting.Attachments;
using ProtonVPN.BugReporting.Diagnostic;
using ProtonVPN.Common.Abstract;

namespace ProtonVPN.BugReporting
{
    public class BugReport : IBugReport
    {
        private readonly IApiClient _apiClient;
        private readonly IReportFieldProvider _reportFieldProvider;
        private readonly IAttachmentsLoader _attachmentsLoader;
        private readonly NetworkLogWriter _networkLogWriter;

        public BugReport(IApiClient apiClient, IReportFieldProvider reportFieldProvider,
            IAttachmentsLoader attachmentsLoader, NetworkLogWriter networkLogWriter)
        {
            _apiClient = apiClient;
            _reportFieldProvider = reportFieldProvider;
            _attachmentsLoader = attachmentsLoader;
            _networkLogWriter = networkLogWriter;
        }

        public async Task<Result> SendAsync(SendReportAction message)
        {
            KeyValuePair<string, string>[] fields = _reportFieldProvider.GetFields(message);
            return message.SendLogs
                ? await SendWithLogsAsync(fields)
                : await SendAsync(fields);
        }

        private async Task<Result> SendWithLogsAsync(KeyValuePair<string, string>[] fields)
        {
            await _networkLogWriter.WriteAsync();
            return await SendInternalAsync(fields, new AttachmentsToApiFiles(_attachmentsLoader.Get()));
        }

        private async Task<Result> SendInternalAsync(KeyValuePair<string, string>[] fields, IEnumerable<File> files = null)
        {
            files ??= new List<File>();
            try
            {
                return await _apiClient.ReportBugAsync(fields, files);
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        }

        private async Task<Result> SendAsync(KeyValuePair<string, string>[] fields)
        {
            return await SendInternalAsync(fields);
        }
    }
}