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

namespace ProtonVPN.OperatingSystems.Registries.Contracts
{
    public interface IRegistryEditor
    {
        public object? ReadObject(RegistryUri uri);
        public int? ReadInt(RegistryUri uri);
        public string? ReadString(RegistryUri uri);
        Dictionary<string, string> ReadAll(RegistryUri uri);

        /// <summary>Returns 'true' if successfully written or 'false' if an error occurred.</summary>
        public bool WriteInt(RegistryUri uri, int value);

        /// <summary>Returns 'true' if successfully written or 'false' if an error occurred.</summary>
        public bool WriteString(RegistryUri uri, string value);

        /// <summary>Returns 'true' if successfully deleted, 'null' if the key does not exist,
        /// or 'false' if the key exists but an error occurred when deleting.</summary>
        public bool? Delete(RegistryUri uri);
    }
}