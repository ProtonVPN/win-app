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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Connection.EntityMapping;

public class FeatureIntentMapper : IMapper<IFeatureIntent, SerializableFeatureIntent>
{
    public SerializableFeatureIntent Map(IFeatureIntent leftEntity)
    {
        return leftEntity is null
            ? null
            : new SerializableFeatureIntent()
            {
                TypeName = leftEntity.GetType().Name,
                EntryCountryCode = leftEntity is SecureCoreFeatureIntent secureCoreFeatureIntent ? secureCoreFeatureIntent.EntryCountryCode : null
            };
    }

    public IFeatureIntent Map(SerializableFeatureIntent rightEntity)
    {
        return rightEntity is null
            ? null
            : rightEntity.TypeName switch
            {
                nameof(B2BFeatureIntent) => new B2BFeatureIntent(),
                nameof(P2PFeatureIntent) => new P2PFeatureIntent(),
                nameof(SecureCoreFeatureIntent) => new SecureCoreFeatureIntent(rightEntity.EntryCountryCode),
                nameof(TorFeatureIntent) => new TorFeatureIntent(),
                _ => throw new NotImplementedException($"No mapping is implemented for {rightEntity.TypeName}"),
            };
    }
}