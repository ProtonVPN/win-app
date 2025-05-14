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

using ProtonVPN.Client.Contracts.Services.Edition;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;
using Windows.ApplicationModel.DataTransfer;

namespace ProtonVPN.Client.Services.Edition;

public class ClipboardEditor : IClipboardEditor
{
    private const int MAX_CLIPBOARD_TRIES = 4;
    private const int BASE_CLIPBOARD_RETRY_TIME_IN_MILLISECONDS = 100;

    private readonly ILogger _logger;

    public ClipboardEditor(ILogger logger)
    {
        _logger = logger;
    }

    public async Task SetTextAsync(string text)
    {
        int retryTimeInMilliseconds = BASE_CLIPBOARD_RETRY_TIME_IN_MILLISECONDS;
        for (int i = 1; i <= MAX_CLIPBOARD_TRIES; i++)
        {
            try
            {
                DataPackage dataPackage = new();
                dataPackage.SetText(text);
                Clipboard.SetContent(dataPackage);
                break;
            }
            catch (Exception exception)
            {
                string logMessage = $"Error when copying text to clipboard. Try {i} of {MAX_CLIPBOARD_TRIES}.";
                if (i == MAX_CLIPBOARD_TRIES)
                {
                    _logger.Error<OperatingSystemLog>(logMessage, exception);
                    break;
                }
                _logger.Warn<OperatingSystemLog>($"{logMessage} Waiting {retryTimeInMilliseconds}ms before retry.", exception);
                await Task.Delay(TimeSpan.FromMilliseconds(retryTimeInMilliseconds));
                retryTimeInMilliseconds *= 2;
            }
        }
    }

    public async Task<string> GetTextAsync()
    {
        try
        {
            DataPackageView data = Clipboard.GetContent();

            return data != null && data.Contains(StandardDataFormats.Text)
                ? (await data.GetTextAsync()).Trim()
                : string.Empty;
        }
        catch (Exception e)
        {
            _logger.Error<OperatingSystemLog>("Error when retrieving text from clipboard.", e);
            return string.Empty;
        }
    }
}