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

using System.Text;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Feedback.Attachments;
using ProtonVPN.Client.Logic.Feedback.Contracts;
using ProtonVPN.Client.Logic.Feedback.Diagnostics;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Common.Legacy.OS.DeviceIds;
using ProtonVPN.Common.Legacy.OS.Systems;
using ProtonVPN.OperatingSystems.WebAuthn.Contracts;
using File = ProtonVPN.Api.Contracts.File;

namespace ProtonVPN.Client.Logic.Feedback;

public class ReportIssueSender : IReportIssueSender
{
    private const string NOT_PROVIDED_FIELD_VALUE = "Not provided";
    private const string FREE_VPN_PLAN_FIELD_VALUE = "Proton VPN Free";

    private readonly ISettings _settings;
    private readonly IApiClient _apiClient;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly ISystemState _systemState;
    private readonly IDeviceIdCache _deviceIdCache;
    private readonly IDiagnosticLogWriter _diagnosticLogWriter;
    private readonly IAttachmentsLoader _attachmentsLoader;
    private readonly IWebAuthnAuthenticator _webAuthnAuthenticator;

    public ReportIssueSender(
        ISettings settings, 
        IApiClient apiClient, 
        IUserAuthenticator userAuthenticator, 
        ISystemState systemState, 
        IDeviceIdCache deviceIdCache, 
        IDiagnosticLogWriter diagnosticLogWriter, 
        IAttachmentsLoader attachmentsLoader,
        IWebAuthnAuthenticator webAuthnAuthenticator)
    {
        _settings = settings;
        _apiClient = apiClient;
        _userAuthenticator = userAuthenticator;
        _systemState = systemState;
        _deviceIdCache = deviceIdCache;
        _diagnosticLogWriter = diagnosticLogWriter;
        _attachmentsLoader = attachmentsLoader;
        _webAuthnAuthenticator = webAuthnAuthenticator;
    }

    public async Task<Result> SendAsync(string category, string email, IDictionary<string, string> inputFields, bool includeLogs)
    {
        KeyValuePair<string, string>[] reportFields = GetReportFields(category, email, inputFields);

        return includeLogs
             ? await SendWithLogsAsync(reportFields)
             : await SendInternalAsync(reportFields);
    }

    private async Task<Result> SendWithLogsAsync(KeyValuePair<string, string>[] fields)
    {
        await _diagnosticLogWriter.WriteAsync();
        return await SendInternalAsync(fields, new AttachmentsToApiFiles(_attachmentsLoader.Get()));
    }

    private async Task<Result> SendInternalAsync(KeyValuePair<string, string>[] fields, IEnumerable<File>? files = null)
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

    private KeyValuePair<string, string>[] GetReportFields(string category, string email, IDictionary<string, string> inputFields)
    {
        DeviceLocation? currentLocation = _settings.DeviceLocation;

        return new[]
        {
            new KeyValuePair<string, string>("OS", "Windows"),
            new KeyValuePair<string, string>("OSVersion", OSVersion.GetString()),
            new KeyValuePair<string, string>("Platform", OSArchitecture.StringValue),
            new KeyValuePair<string, string>("Client", "Windows app"),
            new KeyValuePair<string, string>("ClientVersion", AssemblyVersion.Get()),
            new KeyValuePair<string, string>("Title", "Windows app form"),
            new KeyValuePair<string, string>("Description", GetDescription(category, inputFields)),
            new KeyValuePair<string, string>("Username", GetUsername()),
            new KeyValuePair<string, string>("Plan", GetVpnPlanName()),
            new KeyValuePair<string, string>("Email", email),
            new KeyValuePair<string, string>("Country", currentLocation?.CountryCode ?? NOT_PROVIDED_FIELD_VALUE),
            new KeyValuePair<string, string>("ISP", currentLocation?.Isp ?? NOT_PROVIDED_FIELD_VALUE),
            new KeyValuePair<string, string>("ClientType", "2"),
            new KeyValuePair<string, string>("IsWebAuthnSupported", IsWebAuthnSupported()),

        };
    }

    private string IsWebAuthnSupported()
    {
        return _webAuthnAuthenticator.IsSupported.ToBooleanString();
    }

    private string GetUsername()
    {
        return _userAuthenticator.IsLoggedIn && !string.IsNullOrEmpty(_settings.Username)
            ? _settings.Username
            : NOT_PROVIDED_FIELD_VALUE;
    }

    private string GetVpnPlanName()
    {
        if (_userAuthenticator.IsLoggedIn)
        {
            string? vpnPlanTitle = _settings.VpnPlan.Title;
            return !string.IsNullOrEmpty(vpnPlanTitle)
                ? vpnPlanTitle
                : FREE_VPN_PLAN_FIELD_VALUE;
        }

        return NOT_PROVIDED_FIELD_VALUE;
    }

    private string GetDescription(string category, IDictionary<string, string> inputFields)
    {
        StringBuilder stringBuilder = new();

        // Append category
        stringBuilder
            .AppendLine($"Category: {category}")
            .AppendLine();

        // Append input fields
        foreach (KeyValuePair<string, string> field in inputFields)
        {
            if (string.IsNullOrWhiteSpace(field.Value))
            {
                continue;
            }

            stringBuilder
                .AppendLine(field.Key)
                .AppendLine(field.Value)
                .AppendLine();
        }

        // Append additional info
        stringBuilder
            .AppendLine("Additional info")
            .AppendLine($"Pending reboot: {(_systemState.PendingReboot().ToYesNoString())}")
            .AppendLine($"DeviceID: {_deviceIdCache.GetDeviceId()}");

        return stringBuilder.ToString();
    }
}