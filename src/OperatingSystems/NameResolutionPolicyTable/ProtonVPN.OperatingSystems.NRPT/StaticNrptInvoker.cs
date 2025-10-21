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

using Microsoft.Win32;

namespace ProtonVPN.OperatingSystems.NRPT;

public static class StaticNrptInvoker
{
    private const string NRPT_COMMENT_KEY_NAME = "Comment";
    private const string NRPT_DISPLAY_NAME_KEY_NAME = "DisplayName";

    private const string NRPT_COMMENT_VALUE = "Force all DNS requests via Proton VPN";
    private const string NRPT_DISPLAY_NAME_VALUE = "Proton VPN";

    private const string NRPT_RULES_PATH = @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters\DnsPolicyConfig";

    private static readonly string[] _allDomains = ["."];
    private static readonly object _lock = new();

    /// <returns>If the NRPT rule was added successfully</returns>
    public static bool CreateRule(string nameServers, Action<string, Exception> onException, Action<string> onError, Action<string> onSuccess)
    {
        try
        {
            lock (_lock)
            {
                string ruleGuid = Guid.NewGuid().ToString().ToUpper();
                string rulePath = $"{NRPT_RULES_PATH}\\{{{ruleGuid}}}";
                using (RegistryKey ruleKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).CreateSubKey(rulePath, writable: true))
                {
                    if (ruleKey == null)
                    {
                        onError($"Failed to open or create NRPT registry path {NRPT_RULES_PATH}.");
                        return false;
                    }

                    ruleKey.SetValue(NRPT_COMMENT_KEY_NAME, NRPT_COMMENT_VALUE, RegistryValueKind.String);
                    ruleKey.SetValue("ConfigOptions", 8, RegistryValueKind.DWord);
                    ruleKey.SetValue(NRPT_DISPLAY_NAME_KEY_NAME, NRPT_DISPLAY_NAME_VALUE, RegistryValueKind.String);
                    ruleKey.SetValue("GenericDNSServers", nameServers, RegistryValueKind.String);
                    ruleKey.SetValue("IPSECCARestriction", string.Empty, RegistryValueKind.String);
                    ruleKey.SetValue("Name", _allDomains, RegistryValueKind.MultiString);
                    ruleKey.SetValue("Version", 2, RegistryValueKind.DWord);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            if (onException is not null)
            {
                onException("Exception thrown when adding the NRPT rule", ex);
            }
            return false;
        }
    }

    /// <returns>If the NRPT rule was removed successfully</returns>
    public static bool DeleteRule(Action<string, Exception> onException, Action<string> onSuccess)
    {
        try
        {
            lock (_lock)
            {
                bool result = false;

                using (RegistryKey pathKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(NRPT_RULES_PATH, writable: true))
                {
                    if (pathKey == null)
                    {
                        return false; // NRPT path doesn't exist, nothing to do here
                    }

                    string[] nrptRulesKeyNames = pathKey.GetSubKeyNames();

                    foreach (string nrptRuleKeyName in nrptRulesKeyNames)
                    {
                        result = CheckAndDeleteRule(nrptRuleKeyName, pathKey, onException, onSuccess: onSuccess) || result;
                    }
                }

                return result;
            }
        }
        catch (Exception ex)
        {
            if (onException is not null)
            {
                onException("Exception thrown when removing the NRPT rule", ex);
            }
            return false;
        }
    }

    /// <returns>Was the NRPT rule removed successfully (Failure doesn't mean anything wrong, might not be our rule)</returns>
    private static bool CheckAndDeleteRule(string nrptRuleKeyName, RegistryKey pathKey,
        Action<string, Exception> onException, Action<string> onSuccess)
    {
        try
        {
            using (RegistryKey nrptRuleKey = pathKey.OpenSubKey(nrptRuleKeyName))
            {
                if (nrptRuleKey == null)
                {
                    return false; // NRPT rule key name doesn't exist
                }

                object displayNameObj = nrptRuleKey.GetValue(NRPT_DISPLAY_NAME_KEY_NAME);
                string displayName = displayNameObj?.ToString();
                if (displayName is not null && displayName.Equals(NRPT_DISPLAY_NAME_VALUE, StringComparison.InvariantCultureIgnoreCase))
                {
                    DeleteKey(pathKey, nrptRuleKeyName, onSuccess);
                    return true;
                }

                object commentObj = nrptRuleKey.GetValue(NRPT_COMMENT_KEY_NAME);
                string comment = commentObj?.ToString();
                if (comment is not null && comment.Equals(NRPT_COMMENT_VALUE, StringComparison.InvariantCultureIgnoreCase))
                {
                    DeleteKey(pathKey, nrptRuleKeyName, onSuccess);
                    return true;
                }
            }

            return false; // This NRPT rule exists but is not ours, leave it as is
        }
        catch (Exception ex)
        {
            if (onException is not null)
            {
                onException("Exception thrown when removing the NRPT rule", ex);
            }
            return false;
        }
    }

    private static void DeleteKey(RegistryKey pathKey, string nrptRuleKeyName, Action<string> onSuccess)
    {
        pathKey.DeleteSubKey(nrptRuleKeyName);
        onSuccess($"Successfully deleted the NRPT rule '{nrptRuleKeyName}'.");
    }
}
