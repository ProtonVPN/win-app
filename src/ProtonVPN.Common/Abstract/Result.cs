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

using System;

namespace ProtonVPN.Common.Abstract
{
    public class Result
    {
        protected Result(bool success, string error = null, Exception exception = null)
        {
            Success = success;
            Error = error ?? "";
            Exception = exception;
        }

        public bool Success { get; }

        public string Error { get; protected set; }

        public Exception Exception { get; }

        public bool Failure => !Success;

        public static Result Fail(string message = null) => new(false, message);

        public static Result Fail(Exception exception) => new(false, "", exception);

        public static Result<T> Fail<T>(string message = "") => new(default(T), false, message);

        public static Result Ok() => new(true, "");

        public static Result<T> Ok<T>(T value) => new(value, true, "");
    }
}