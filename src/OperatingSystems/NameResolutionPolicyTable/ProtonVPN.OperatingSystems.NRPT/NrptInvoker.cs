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

using System.Management.Automation;
using System.Text;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;
using ProtonVPN.OperatingSystems.NRPT.Contracts;

namespace ProtonVPN.OperatingSystems.NRPT;

public class NrptInvoker : INrptInvoker
{
    private readonly ILogger _logger;
    private readonly IIssueReporter _issueReporter;

    public NrptInvoker(ILogger logger, IIssueReporter issueReporter)
    {
        _logger = logger;
        _issueReporter = issueReporter;
    }

    public void CreateRule(string nameServers)
    {
        StaticNrptInvoker.CreateRule(nameServers, OnNrptException, OnError, OnSuccess);
    }

    private void OnNrptException(string errorMessage, Exception ex)
    {
        _logger.Error<OperatingSystemNrptLog>(errorMessage, ex);
        _issueReporter.CaptureError(new Exception(errorMessage, ex));
    }

    private void OnError(string message, List<ErrorRecord> errors)
    {
        StringBuilder errorMessageBuilder = new();
        foreach (ErrorRecord error in errors)
        {
            _logger.Error<OperatingSystemNrptLog>($"{message}: {error}");
            errorMessageBuilder.AppendLine(error.ToString());
        }
        _issueReporter.CaptureError(message, errorMessageBuilder.ToString());
    }

    private void OnSuccess(string message)
    {
        _logger.Info<OperatingSystemNrptLog>(message);
    }

    public void DeleteRule()
    {
        StaticNrptInvoker.DeleteRule(OnNrptException, OnError, OnSuccess);
    }
}
