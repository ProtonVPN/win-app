/*
 * Copyright (c) 2020 Proton Technologies AG
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
using System.ComponentModel;
using ProtonVPN.Common.Abstract;

namespace ProtonVPN.Common.OS.Services
{
    public class SafeService : IService
    {
        private readonly IService _origin;

        public SafeService(IService origin)
        {
            _origin = origin;
        }

        public string Name => _origin.Name;

        public event EventHandler<string> ServiceStartedHandler;

        public bool IsRunning()
        {
            try
            {
                return _origin.IsRunning();
            }
            catch (Win32Exception)
            {
                return false;
            }
        }

        public Result Start()
        {
            return Safe(() => _origin.Start());
        }

        public Result Stop()
        {
            return Safe(() => _origin.Stop());
        }

        private Result Safe(Func<Result> action)
        {
            try
            {
                return action();
            }
            catch (InvalidOperationException ex) when (ex.IsServiceAlreadyRunning() || ex.IsServiceNotRunning())
            {
                return Result.Ok();
            }
            catch (Exception ex) when (IsExpectedException(ex))
            {
                return Result.Fail(ex);
            }
        }

        private bool IsExpectedException(Exception ex) => ex is InvalidOperationException ||
                                                          ex is System.ServiceProcess.TimeoutException ||
                                                          ex is TimeoutException;
    }
}
