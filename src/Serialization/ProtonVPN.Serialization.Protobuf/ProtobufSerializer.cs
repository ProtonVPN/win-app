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

using System.Reflection;
using ProtoBuf;
using ProtoBuf.Meta;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Serialization.Protobuf;

public class ProtobufSerializer : IProtobufSerializer
{
    public Serializers Type => Serializers.Protobuf;

    public ProtobufSerializer(IProtobufSerializableEntities protobufSerializableEntities)
    {
        RuntimeTypeModel.Default.InferTagFromNameDefault = true;
        foreach (Type type in protobufSerializableEntities.Types)
        {
            if (!RuntimeTypeModel.Default.IsDefined(type))
            {
                MetaType metaType = RuntimeTypeModel.Default.Add(type, applyDefaultBehaviour: false);
                if (type.IsClass || type.IsInterface || type.IsStruct())
                {
                    metaType.Add(type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToArray());
                }
            }
        }
    }

    public T? Deserialize<T>(MemoryStream memoryStream)
    {
        memoryStream.Position = 0;
        return Serializer.Deserialize<T>(memoryStream);
    }

    public MemoryStream Serialize<T>(T? instance)
    {
        MemoryStream memoryStream = new();
        Serializer.Serialize(memoryStream, instance);
        memoryStream.Position = 0;
        return memoryStream;
    }
}