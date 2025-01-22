/*
 * Copyright (c) 2024 Proton AG
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

namespace ProtonInstaller;

public class CommandLineOption
{
    private readonly string _name;
    private readonly string[] _args;

    public CommandLineOption(string name, string[] args)
    {
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
        arg = arg.ToLower();
        name = name.ToLower();

        return arg.Equals($"/{name}") || 
               arg.Equals($"-{name}") ||
               arg.Equals($"--{name}");
    }

    private bool IsOption(string arg)
    {
        return arg.StartsWith("-") || arg.StartsWith("/");
    }
}