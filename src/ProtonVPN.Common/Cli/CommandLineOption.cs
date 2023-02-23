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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;

namespace ProtonVPN.Common.Cli
{
    public class CommandLineOption
    {
        private readonly string _name;
        private readonly string[] _args;

        public CommandLineOption(string name, string[] args)
        {
            Ensure.NotNull(args, nameof(args));

            _name = name;
            _args = args;
        }

        public bool Exists()
        {
            return _args
                .SkipWhile(a => !IsOptionWithName(a, _name))
                .Any();
        }

        public IReadOnlyList<string> Params()
        {
            return _args
                .SkipWhile(a => !IsOptionWithName(a, _name))
                .Skip(1)
                .TakeWhile(a => !IsOption(a))
                .ToList();
        }

        private bool IsOptionWithName(string arg, string name)
        {
            return arg.EqualsIgnoringCase($"/{name}") || 
                   arg.EqualsIgnoringCase($"-{name}") ||
                   arg.EqualsIgnoringCase($"--{name}");
        }

        private bool IsOption(string arg)
        {
            return arg.StartsWith("-") || arg.StartsWith("/");
        }
    }
}
