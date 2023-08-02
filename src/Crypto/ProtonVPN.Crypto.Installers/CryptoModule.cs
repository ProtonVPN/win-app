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


using Autofac;
using ProtonVPN.Crypto.Contracts;

namespace ProtonVPN.Crypto.Installers;

public class CryptoModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Ed25519SignatureValidator>().As<IEd25519SignatureValidator>().SingleInstance();
        builder.RegisterType<Ed25519Asn1KeyGenerator>().As<IEd25519Asn1KeyGenerator>().SingleInstance();
        builder.RegisterType<X25519KeyGenerator>().As<IX25519KeyGenerator>().SingleInstance();
        builder.RegisterType<RandomStringGenerator>().As<IRandomStringGenerator>().SingleInstance();
        builder.RegisterType<Sha1Calculator>().As<ISha1Calculator>().SingleInstance();
    }
}