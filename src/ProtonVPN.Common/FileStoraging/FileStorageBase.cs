﻿/*
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
using System.Collections;
using System.IO;
using System.Linq;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Text.Serialization;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Common.FileStoraging
{
    public abstract class FileStorageBase<T> : IFileStorageBase<T>
    {
        private readonly ITextSerializer<T> _serializer;
        private readonly ILogger _logger;
        private readonly string _fileName;

        protected FileStorageBase(ILogger logger, ITextSerializerFactory serializerFactory, string fileName)
        {
            Ensure.NotEmpty(fileName, nameof(fileName));
            
            _logger = logger;
            _fileName = fileName;
            _serializer = serializerFactory.Serializer<T>();
        }

        public T Get()
        {
            try
            {
                return Logged(ExecuteGet, $"Failed reading {NameOf(typeof(T))} from storage");
            }
            catch
            {
                return default;
            }
        }

        public T Logged(Func<T> function, string message)
        {
            try
            {
                return function();
            }
            catch (Exception ex)
            {
                LogException(ex, message);
                throw;
            }
        }

        private void LogException(Exception exception, string message)
        {
            _logger.Error<AppLog>($"{message}: {exception.CombinedMessage()}");
        }

        private T ExecuteGet()
        {
            using (StreamReader reader = new(_fileName))
            {
                return _serializer.Deserialize(reader);
            }
        }

        private static string NameOf(Type type)
        {
            if (IsEnumerableType(type) && type.GetGenericArguments().Any())
            {
                return $"{type.GetGenericArguments()[0].Name} collection";
            }

            return type.Name;
        }

        private static bool IsEnumerableType(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
        }

        public void Set(T value)
        {
            try
            {
                Logged(() => ExecuteSet(value), $"Failed writing {NameOf(typeof(T))} to storage");
            }
            catch
            {
            }
        }

        public void Logged(Action action, string message)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                LogException(ex, message);
                throw;
            }
        }

        private void ExecuteSet(T value)
        {
            using (StreamWriter writer = new(_fileName))
            {
                _serializer.Serialize(value, writer);
            }
        }
    }
}
