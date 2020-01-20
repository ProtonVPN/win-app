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
using System.IO;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Text.Serialization;

namespace ProtonVPN.Common.Storage
{
    public class FileStorage<T> : IStorage<T>, IThrowsExpectedExceptions
    {
        private readonly ITextSerializer<T> _serializer;
        private readonly string _fileName;

        public FileStorage(ITextSerializerFactory serializerFactory, string fileName)
        {
            Ensure.NotNull(serializerFactory, nameof(serializerFactory));
            Ensure.NotEmpty(fileName, nameof(fileName));

            _serializer = serializerFactory.Serializer<T>();

            Ensure.IsTrue(_serializer is IThrowsExpectedExceptions,
                $"{nameof(serializerFactory)}.{nameof(ITextSerializerFactory.Serializer)} must implement {nameof(IThrowsExpectedExceptions)} interface");

            _fileName = fileName;
        }

        public T Get()
        {
            using var reader = new StreamReader(_fileName);
            return _serializer.Deserialize(reader);
        }

        public void Set(T value)
        {
            using var writer = new StreamWriter(_fileName);
            _serializer.Serialize(value, writer);
        }

        public bool IsExpectedException(Exception ex)
        {
            return ex.IsFileAccessException() ||
                   ex.IsExpectedExceptionOf(_serializer);
        }
    }
}
