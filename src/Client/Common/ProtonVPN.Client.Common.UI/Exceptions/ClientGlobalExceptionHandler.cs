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
using Microsoft.UI.Xaml;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Events;
using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;

namespace ProtonVPN.Client.Common.UI.Exceptions;

public sealed class ClientGlobalExceptionHandler : GlobalExceptionHandlerBase
{
    public void Initialize(Application app)
    {
        Initialize();

        app.UnhandledException += OnUiUnhandledException;
    }

    private void OnUiUnhandledException(object? sender, UnhandledExceptionEventArgs ex)
    {
        TryLogException("UI unhandled exception", ex.Exception, isFatal: false);
        ex.Handled = true;
    }

    protected override void LogFatal(string handler, Exception exception)
    {
        Logger?.Fatal<AppCrashLog>(handler, exception);
    }

    protected override void LogError(string handler, Exception exception)
    {
        Logger?.Error<AppLog>(handler, exception);
    }
}
