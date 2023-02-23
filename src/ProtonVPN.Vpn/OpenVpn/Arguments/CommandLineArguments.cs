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

using System.Collections.Generic;
using System.Linq;

namespace ProtonVPN.Vpn.OpenVpn.Arguments
{
    /// <summary>
    /// Command line arguments for a process.
    /// </summary>
    public class CommandLineArguments
    {
        private readonly List<IEnumerable<string>> _arguments = new List<IEnumerable<string>>();

        public CommandLineArguments Add(IEnumerable<string> arguments)
        {
            _arguments.Add(arguments);

            return this;
        }

        public static implicit operator string(CommandLineArguments item) => item.ToString();

        public override string ToString()
        {
            return string.Join(" ", Arguments());
        }

        private IEnumerable<string> Arguments()
        {
            return _arguments.SelectMany(a => a);
        }
    }
}
