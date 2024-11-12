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
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Annotations;

public class Sli : Attribute, ITestAction
{
    private string _sliName;
    //Execute once when this Attribute is Initalized
    public Sli(string name)
    {
        _sliName = name;
    }

    public ActionTargets Targets => ActionTargets.Test;

    public void AfterTest(ITest test)
    {
        // Do Nothing
    }

    public void BeforeTest(ITest test)
    {
        SliHelper.SliName = _sliName;
        SliHelper.RunId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
