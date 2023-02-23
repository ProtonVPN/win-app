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

using System;
using System.IO;

namespace ProtonVPN.MarkupValidator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
            {
                Console.WriteLine("Error: Resource file not provided");
                Environment.Exit(1);
                return;
            }

            var file = args[0];
            if (!File.Exists(file))
            {
                Console.WriteLine("Error: file not found");
                Environment.Exit(1);
                return;
            }

            var resourceFile = new ResourceFile(file);
            var valid = resourceFile.Valid();
            if (!valid)
            {
                Console.WriteLine(resourceFile.Error);
            }

            Environment.Exit(valid ? 0 : 1);
        }
    }
}
