/*
 * Copyright (c) 2026 Proton AG
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
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FlaUI.UIA3;
using FlaUI.Core.Patterns;
using FlaUI.Core.AutomationElements;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static partial class ToastCapture
{
    private const int TOAST_POLL_INTERVAL_MS = 50;
    private const int TOAST_DEFAULT_TIMEOUT_MS = 8000;
    private const string APP_PROCESS_NAME = "ProtonVPN.Client";

    [GeneratedRegex(@"\b(\d{2,5})\b", RegexOptions.CultureInvariant)]
    private static partial Regex PortRegex();

    private static readonly string[] _toastRootIds = ["NormalToastView", "ToastCenterScrollViewer"];
    private static readonly string[] _titleIds = ["Title", "Title2"];
    private static readonly string[] _messageIds = ["MessageText", "MessageText2"];
    private static readonly Element _verbButton = Element.ByAutomationId("VerbButton");
    private static readonly Element _dismissButton = Element.ByAutomationId("DismissButton");

    public static bool WaitForToastVisible(UIA3Automation automation, TimeSpan timeout)
    {
        return PollToast(automation, timeout, scope => IsVisible(FindByAnyId(scope, _titleIds)) || IsVisible(FindByAnyId(scope, _messageIds)));
    }

    public static void DismissToast(UIA3Automation automation, TimeSpan? timeout = null)
    {
        bool dismissed = PollToast(automation, timeout, scope =>
        {
            AutomationElement? dismissButton = scope.FindFirstDescendant(_dismissButton.Condition);
            if (dismissButton == null || dismissButton.Properties.IsOffscreen.ValueOrDefault || !dismissButton.IsEnabled)
            {
                return false;
            }

            InvokeOrClick(dismissButton);
            return true;
        });

        if (!dismissed)
        {
            throw new TimeoutException("Dismiss button not found or not clickable.");
        }
    }

    public static string GetConnectionStateFromVisibleToast(UIA3Automation automation, TimeSpan? timeout = null)
    {
        string? text = null;
        PollToast(automation, timeout, scope => (text = ReadText(scope, _messageIds) ?? ReadText(scope, _titleIds)) != null);

        return text ?? throw new TimeoutException("Toast text not found in time.");
    }

    public static int GetPortFromVisibleToast(UIA3Automation automation, TimeSpan? timeout = null)
    {
        int? port = null;
        PollToast(automation, timeout, scope => (port = ReadPort(scope)).HasValue);

        return port ?? throw new TimeoutException("Toast text not found in time.");
    }

    public static int ClickToastCopyAndGetPort(UIA3Automation automation, TimeSpan? timeout = null)
    {
        int? port = null;
        PollToast(automation, timeout, scope => (port = ClickCopyAndReadPort(scope)).HasValue);

        return port ?? throw new TimeoutException("Toast copy button/clipboard not available.");
    }

    // Polls until the probe succeeds for the toast, or the timeout elapses.
    private static bool PollToast(UIA3Automation automation, TimeSpan? timeout, Func<AutomationElement, bool> probe)
    {
        AutomationElement desktop = automation.GetDesktop();
        DateTime end = DateTime.UtcNow + (timeout ?? TimeSpan.FromMilliseconds(TOAST_DEFAULT_TIMEOUT_MS));

        while (DateTime.UtcNow < end)
        {
            AutomationElement? scope = FindToastRoot(desktop);
            if (scope != null && probe(scope))
            {
                return true;
            }

            Thread.Sleep(TOAST_POLL_INTERVAL_MS);
        }

        return false;
    }

    // Toasts are drawn by the Windows shell, so the app's own windows are skipped: a UI Automation call
    // into a frozen app blocks until the app exits, and it would freeze the whole test run.
    private static AutomationElement? FindToastRoot(AutomationElement desktop)
    {
        HashSet<int> appProcessIds = GetAppProcessIds();
        AutomationElement[] shellWindows = desktop.FindAllChildren()
            .Where(window => !appProcessIds.Contains(window.Properties.ProcessId.ValueOrDefault))
            .ToArray();

        foreach (string toastRootId in _toastRootIds)
        {
            foreach (AutomationElement window in shellWindows)
            {
                // The top-level window can itself be the toast root, as a desktop-wide descendant search would also match it.
                AutomationElement? toast = window.Properties.AutomationId.ValueOrDefault == toastRootId
                    ? window
                    : window.FindFirstDescendant(cf => cf.ByAutomationId(toastRootId));
                if (toast != null)
                {
                    return toast;
                }
            }
        }

        return null;
    }

    private static HashSet<int> GetAppProcessIds()
    {
        Process[] processes = Process.GetProcessesByName(APP_PROCESS_NAME);
        try
        {
            return processes.Select(process => process.Id).ToHashSet();
        }
        finally
        {
            foreach (Process process in processes)
            {
                process.Dispose();
            }
        }
    }

    private static AutomationElement? FindByAnyId(AutomationElement scope, string[] automationIds)
    {
        return automationIds
            .Select(automationId => scope.FindFirstDescendant(cf => cf.ByAutomationId(automationId)))
            .FirstOrDefault(element => element != null);
    }

    private static bool IsVisible(AutomationElement? element)
    {
        return element != null && !element.Properties.IsOffscreen.ValueOrDefault;
    }

    private static string? ReadText(AutomationElement scope, string[] automationIds)
    {
        AutomationElement? element = FindByAnyId(scope, automationIds);
        if (element == null || !element.Properties.Name.TryGetValue(out string? text) || string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return text.Trim();
    }

    private static int? ReadPort(AutomationElement scope)
    {
        return ParsePortFromText(ReadText(scope, _messageIds)) ?? ParsePortFromText(ReadText(scope, _titleIds));
    }

    private static int? ClickCopyAndReadPort(AutomationElement scope)
    {
        AutomationElement? verb = scope.FindFirstDescendant(_verbButton.Condition);
        if (verb != null && !verb.Properties.IsOffscreen.ValueOrDefault && verb.IsEnabled)
        {
            InvokeOrClick(verb);

            int? fromClipboard = ParsePortFromText(DesktopRobot.ReadClipboardText());
            if (fromClipboard.HasValue)
            {
                return fromClipboard;
            }
        }

        return ReadPort(scope);
    }

    private static void InvokeOrClick(AutomationElement element)
    {
        try
        {
            IInvokePattern? invokable = element.Patterns.Invoke.PatternOrDefault;
            if (invokable != null)
            {
                invokable.Invoke();
            }
            else
            {
                element.Click();
            }
        }
        catch
        {
            try
            {
                element.Click();
            }
            catch { }
        }
    }

    private static int? ParsePortFromText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        Match match = PortRegex().Match(text);
        return match.Success && int.TryParse(match.Groups[1].Value, out int port) ? port : null;
    }
}
