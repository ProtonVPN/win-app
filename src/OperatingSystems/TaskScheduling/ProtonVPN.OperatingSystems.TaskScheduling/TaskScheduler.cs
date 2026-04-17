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

using Microsoft.Win32.TaskScheduler;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;
using ProtonVPN.OperatingSystems.TaskScheduling.Contracts;

namespace ProtonVPN.OperatingSystems.TaskScheduling;

public class TaskScheduler : ITaskScheduler
{
    private readonly ILogger _logger;

    public TaskScheduler(ILogger logger)
    {
        _logger = logger;
    }

    public bool Create(string taskName, string exePath, string args)
    {
        try
        {
            CreateScheduledTask(taskName, exePath, args);
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemLog>("Exception thrown when creating a scheduled task.", ex);
            return false;
        }

        return true;
    }

    private static void CreateScheduledTask(string taskName, string exePath, string args)
    {
        using TaskService taskService = new();

        TaskDefinition taskDef = taskService.NewTask();

        // Trigger: On boot
        taskDef.Triggers.Add(new BootTrigger());

        // Action: Run the executable
        taskDef.Actions.Add(new ExecAction(exePath, args));

        // Permissions: Run as SYSTEM
        taskDef.Principal.UserId = "S-1-5-18";
        taskDef.Principal.RunLevel = TaskRunLevel.Highest;
        taskDef.Principal.LogonType = TaskLogonType.ServiceAccount;

        // Settings: Disable the power settings
        taskDef.Settings.DisallowStartIfOnBatteries = false;
        taskDef.Settings.StopIfGoingOnBatteries = false;
        taskDef.Settings.IdleSettings.StopOnIdleEnd = false;
        taskDef.Settings.ExecutionTimeLimit = TimeSpan.Zero; // No time limit

        // Create or update scheduled task (Ex.: Updates path on version updates)
        taskService.RootFolder.RegisterTaskDefinition(taskName, taskDef);
    }
}