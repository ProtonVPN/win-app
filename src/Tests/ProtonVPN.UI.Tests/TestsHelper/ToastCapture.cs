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

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static partial class ToastCapture
{
    private const int TOAST_POLL_INTERVAL_MS = 50;
    private const int TOAST_DEFAULT_TIMEOUT_MS = 8000;

    [GeneratedRegex(@"\b(\d{2,5})\b", RegexOptions.CultureInvariant)]
    private static partial Regex PortRegex();
    private static readonly Regex _portRegex = PortRegex();

    private static readonly Element _title2 = Element.ByAutomationId("Title2");
    private static readonly Element _messageText2 = Element.ByAutomationId("MessageText2");
    private static readonly Element _verbButton = Element.ByAutomationId("VerbButton");    

    public static bool WaitForToastVisible(UIA3Automation automation, TimeSpan timeout)
    {
        AutomationElement desktop = automation.GetDesktop();
        DateTime end = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < end)
        {
            AutomationElement? scope = FindToastRoot(desktop);
            if (scope != null)
            {
                AutomationElement? title = scope.FindFirstDescendant(_title2.Condition);
                AutomationElement? message = scope.FindFirstDescendant(_messageText2.Condition);

                bool titleVisible = title != null && !title.Properties.IsOffscreen.ValueOrDefault;
                bool messageVisible = message != null && !message.Properties.IsOffscreen.ValueOrDefault;

                if (titleVisible || messageVisible)
                {
                    return true;
                }
            }

            Thread.Sleep(TOAST_POLL_INTERVAL_MS);
        }

        return false;
    }

    public static int GetPortFromVisibleToast(UIA3Automation automation, TimeSpan? timeout = null)
    {
        AutomationElement desktop = automation.GetDesktop();
        TimeSpan effectiveTimeout = timeout ?? TimeSpan.FromMilliseconds(TOAST_DEFAULT_TIMEOUT_MS);
        DateTime end = DateTime.UtcNow + effectiveTimeout;

        while (DateTime.UtcNow < end)
        {
            AutomationElement? scope = FindToastRoot(desktop);
            if (scope != null)
            {
                int? fromMessage = TryResolveFromMessage(scope);
                if (fromMessage.HasValue)
                {
                    return fromMessage.Value;
                }

                int? fromTitle = TryResolveFromTitle(scope);
                if (fromTitle.HasValue)
                {
                    return fromTitle.Value;
                }
            }

            Thread.Sleep(TOAST_POLL_INTERVAL_MS);
        }

        throw new TimeoutException("Toast text not found in time.");
    }

    public static int ClickToastCopyAndGetPort(UIA3Automation automation, TimeSpan? timeout = null)
    {
        AutomationElement desktop = automation.GetDesktop();
        TimeSpan effectiveTimeout = timeout ?? TimeSpan.FromMilliseconds(TOAST_DEFAULT_TIMEOUT_MS);
        DateTime end = DateTime.UtcNow + effectiveTimeout;

        while (DateTime.UtcNow < end)
        {
            AutomationElement? scope = FindToastRoot(desktop);
            if (scope != null)
            {
                int? value = TryResolveByClickThenClipboard(scope);
                if (value.HasValue)
                {
                    return value.Value;
                }
            }

            Thread.Sleep(TOAST_POLL_INTERVAL_MS);
        }

        throw new TimeoutException("Toast copy button/clipboard not available.");
    }

    private static AutomationElement? FindToastRoot(AutomationElement root)
    {
        AutomationElement? toast = root.FindFirstDescendant(cf => cf.ByAutomationId("NormalToastView"));
        if (toast != null)
        {
            return toast;
        }

        return root.FindFirstDescendant(cf => cf.ByAutomationId("ToastCenterScrollViewer"));
    }

    private static int? TryResolveFromMessage(AutomationElement scope)
    {
        AutomationElement? element = scope.FindFirstDescendant(_messageText2.Condition);
        return ParsePortFromElement(element);
    }

    private static int? TryResolveFromTitle(AutomationElement scope)
    {
        AutomationElement? element = scope.FindFirstDescendant(_title2.Condition);
        return ParsePortFromElement(element);
    }
 
    private static int? TryResolveByClickThenClipboard(AutomationElement scope)
    {
        AutomationElement? verb = scope.FindFirstDescendant(_verbButton.Condition);
        if (verb != null && !verb.Properties.IsOffscreen.ValueOrDefault && verb.IsEnabled)
        {
            try
            {
                FlaUI.Core.Patterns.IInvokePattern? invokable = verb.Patterns.Invoke.PatternOrDefault;
                if (invokable != null)
                {
                    invokable.Invoke();
                }
                else
                {
                    verb.Click();
                }
            }
            catch
            {
                try
                {
                    verb.Click();
                }
                catch
                {
                }
            }

            int? fromClipboard = ParsePortFromText(ReadClipboard());
            if (fromClipboard.HasValue)
            {
                return fromClipboard.Value;
            }
        }

        return TryResolveFromMessage(scope);
    }

    private static int? ParsePortFromElement(AutomationElement? element)
    {
        if (element == null)
        {
            return null;
        }

        bool hasValue = element.Properties.Name.TryGetValue(out string? text);
        if (!hasValue || string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return ParsePortFromText(text);
    }

    private static int? ParsePortFromText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        Match loose = _portRegex.Match(text);
        if (loose.Success && int.TryParse(loose.Groups[1].Value, out int p2))
        {
            return p2;
        }

        return null;
    }

    private static string ReadClipboard()
    {
        string? value = null;

        Thread thread = new(() =>
        {
            try
            {
                string? text = Clipboard.GetText();
                value = text?.Trim();
            }
            catch
            {
                value = null;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        return value is null
            ? string.Empty
            : value;
    }
}