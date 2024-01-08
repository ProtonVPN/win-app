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

using ProtonVPN.Common.Legacy.NetShield;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;

namespace ProtonVPN.ProcessCommunication.EntityMapping.NetShield;

public class NetShieldStatisticMapper : IMapper<NetShieldStatistic, NetShieldStatisticIpcEntity>
{
    public NetShieldStatisticIpcEntity Map(NetShieldStatistic leftEntity)
    {
        return leftEntity is null
            ? null
            : new NetShieldStatisticIpcEntity()
            {
                NumOfMaliciousUrlsBlocked = leftEntity.NumOfMaliciousUrlsBlocked,
                NumOfAdvertisementUrlsBlocked = leftEntity.NumOfAdvertisementUrlsBlocked,
                NumOfTrackingUrlsBlocked = leftEntity.NumOfTrackingUrlsBlocked,
                TimestampUtc = leftEntity.TimestampUtc,
            };
    }

    public NetShieldStatistic Map(NetShieldStatisticIpcEntity rightEntity)
    {
        return rightEntity is null
            ? null
            : new NetShieldStatistic()
            {
                NumOfMaliciousUrlsBlocked = rightEntity.NumOfMaliciousUrlsBlocked,
                NumOfAdvertisementUrlsBlocked = rightEntity.NumOfAdvertisementUrlsBlocked,
                NumOfTrackingUrlsBlocked = rightEntity.NumOfTrackingUrlsBlocked,
                TimestampUtc = rightEntity.TimestampUtc,
            };
    }
}