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

using CommunityToolkit.Mvvm.Messaging;

namespace ProtonVPN.Client.Common.Contracts;

public class ServiceRecipient
{    /// <summary>
     /// Initializes a new instance of the <see cref="ServiceRecipient"/> class.
     /// </summary>
     /// <remarks>
     /// This constructor will produce an instance that will use the <see cref="WeakReferenceMessenger.Default"/> instance
     /// to perform requested operations. It will also be available locally through the <see cref="Messenger"/> property.
     /// </remarks>
    protected ServiceRecipient()
        : this(WeakReferenceMessenger.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceRecipient"/> class.
    /// </summary>
    /// <param name="messenger">The <see cref="IMessenger"/> instance to use to send messages.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="messenger"/> is <see langword="null"/>.</exception>
    protected ServiceRecipient(IMessenger messenger)
    {
        ArgumentNullException.ThrowIfNull(messenger);

        Messenger = messenger;
        Messenger.RegisterAll(this);
    }

    /// <summary>
    /// Gets the <see cref="IMessenger"/> instance in use.
    /// </summary>
    protected IMessenger Messenger { get; }
}