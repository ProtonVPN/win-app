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

using ProtonVPN.Client.Core.Bases.Models;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Settings.Contracts.Enums;

namespace ProtonVPN.Client.Models.Settings;

public class NatTypeItem : ModelBase
{
    public NatType NatType { get; }

    public string Header => Localizer.GetNatType(NatType);

    public bool IsStrictNat => NatType == NatType.Strict;

    public bool IsModerateNat => NatType == NatType.Moderate;

    public NatTypeItem(
        ILocalizationProvider localizer,
        NatType natType)
        : base(localizer)
    {
        NatType = natType;
    }
}