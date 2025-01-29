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

using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace ProtonVPN.Client.UI.Main.Settings.Bases;

public class ChangedSettingArgs
{
    private readonly Func<dynamic?> _getNewValue;
    private readonly Func<dynamic?> _getter;
    private readonly Action<dynamic?> _setter;
    private readonly Func<dynamic?, dynamic?, bool> _comparer;

    public string Name { get; }

    public dynamic? NewValue => _getNewValue();

    private ChangedSettingArgs(
        string name,
        Func<dynamic?> getNewValue,
        Func<dynamic?> getter,
        Action<dynamic?> setter,
        Func<dynamic?, dynamic?, bool> comparer)
    {
        Name = name;

        _getNewValue = getNewValue;
        _getter = getter;
        _setter = setter;
        _comparer = comparer;
    }

    public bool HasChanged()
    {
        try
        {
            return !_comparer(_getter(), NewValue);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void ApplyChanges()
    {
        if (HasChanged())
        {
            _setter.Invoke(NewValue);
        }
    }

    public static ChangedSettingArgs Create<T>(
        Expression<Func<T>> propertyExpression,
        Func<T> newValueProvider)
    {
        if (propertyExpression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Expression must represent a property access.", nameof(propertyExpression));
        }

        if (memberExpression.Member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException("Only property access expressions are supported.", nameof(propertyExpression));
        }

        // Extract property name
        string propertyName = propertyInfo.Name;

        // Compile the getter and wrap it in a function returning dynamic
        Func<T> strongTypedGetter = propertyExpression.Compile();
        Func<dynamic?> getter = () => strongTypedGetter();

        // Convert the new value provider to a dynamic function
        Func<dynamic?> getNewValue = () => newValueProvider();

        // Compile the setter to accept a dynamic value
        Func<object> instanceExpression = Expression.Lambda<Func<object>>(memberExpression.Expression!).Compile();
        object instance = instanceExpression();
        Action<dynamic?> setter = value => propertyInfo.SetValue(instance, value);

        // Build comparer
        Func<dynamic?, dynamic?, bool> comparer;
        if (typeof(IEnumerable).IsAssignableFrom(typeof(T)) && typeof(T) != typeof(string))
        {
            comparer = (a, b) => a is IEnumerable aSeq && b is IEnumerable bSeq
                ? aSeq.Cast<object>().SequenceEqual(bSeq.Cast<object>())
                : Equals(a, b);
        }
        else
        {
            comparer = (a, b) => EqualityComparer<T>.Default.Equals((T)a!, (T)b!);
        }

        return new ChangedSettingArgs(propertyName, getNewValue, getter, setter, comparer);
    }
}