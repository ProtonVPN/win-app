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

using FlaUI.Core.Conditions;
using System;

namespace ProtonVPN.UI.Tests.UiTools;

public class Element
{
    public string SelectorName;
    public Func<ConditionFactory, ConditionBase> Condition;
    public Element ChildElement;

    public Element(Func<ConditionFactory, ConditionBase> selector, string selectorName, Element child = null)
    {
        SelectorName = selectorName;
        Condition = selector;
        ChildElement = child;
    }

    public static Element ByAutomationId(string automationId)
    {
        return new Element(cf => cf.ByAutomationId(automationId), automationId);
    }

    public static Element ByName(string name)
    {
        return new Element(cf => cf.ByName(name), name);
    }

    public static Element ByClassName(string name)
    {
        return new Element(cf => cf.ByClassName(name), name);
    }
}