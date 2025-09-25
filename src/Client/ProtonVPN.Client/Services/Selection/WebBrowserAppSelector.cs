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

using ProtonVPN.Client.Core.Models;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.OperatingSystems.Registries.Contracts;

namespace ProtonVPN.Client.Services.Selection;

public class WebBrowserAppSelector : IWebBrowserAppSelector
{
    private const string DEFAULT_BROWSER_REGISTRY_PATH = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
    private const string DEFAULT_BROWSER_REGISTRY_KEY = "ProgId";
    private const string DEFAULT_BROWSER_OPEN_COMMAND = @"\shell\open\command";
    private readonly ILogger _logger;
    private readonly IRegistryEditor _registryEditor;

    private readonly RegistryUri _defaultBrowserUri = RegistryUri.CreateCurrentUserUri(
        DEFAULT_BROWSER_REGISTRY_PATH,
        DEFAULT_BROWSER_REGISTRY_KEY);

    public WebBrowserAppSelector(
        ILogger logger,
        IRegistryEditor registryEditor)
    {
        _logger = logger;
        _registryEditor = registryEditor;
    }

    public WebBrowserApp? GetDefaultWebBrowserApp()
    {
        try
        {
            string? progId = _registryEditor.ReadString(_defaultBrowserUri);
            if (!string.IsNullOrEmpty(progId))
            {
                RegistryUri openCommandUri = RegistryUri.CreateClassesRootUri(progId + DEFAULT_BROWSER_OPEN_COMMAND, string.Empty);

                string? openCommand = _registryEditor.ReadString(openCommandUri);
                if (!string.IsNullOrEmpty(openCommand))
                {
                    string? exePath = ExtractExePath(openCommand);
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        return WebBrowserApp.TryCreate(exePath, GetPrivateBrowsingModeArgument(exePath));
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Unable to retrieve the default web browser app on the device.", e);
        }

        return null;
    }

    private static string? GetPrivateBrowsingModeArgument(string browserAppPath)
    {
        if (!browserAppPath.IsValidPath())
        {
            return null;
        }

        string fileName = Path.GetFileName(browserAppPath).ToLowerInvariant();
        return fileName switch
        {
            // Chromium-based browsers
            "chrome.exe" or
            "chromium.exe" or
            "brave.exe" or
            "vivaldi.exe" or
            "comet.exe" or
            "arc.exe" or
            "fellou.exe" or
            "maxthon.exe" or
            "colibri.exe" or
            "torch.exe" or
            "baidu.exe" => "--incognito",

            // Opera family
            "opera.exe" or
            "operaone.exe" => "--private",

            // Microsoft Edge
            "msedge.exe" => "--inprivate",

            // Mozilla Firefox
            "firefox.exe" => "-private-window",

            // Internet Explorer
            "iexplore.exe" => "-private",

            _ => null
        };
    }

    private static string? ExtractExePath(string command)
    {
        if (string.IsNullOrEmpty(command))
        {
            return null;
        }

        // Example: "\"C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe\" -- \"%1\""
        if (command.StartsWith("\""))
        {
            int endQuote = command.IndexOf('"', 1);
            if (endQuote > 1)
            {
                return command.Substring(1, endQuote - 1);
            }
        }

        // Otherwise assume first token is path
        int spaceIndex = command.IndexOf(' ');
        return spaceIndex > 0 ? command.Substring(0, spaceIndex) : command;
    }
}