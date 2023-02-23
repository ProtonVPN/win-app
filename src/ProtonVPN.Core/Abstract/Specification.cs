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

namespace ProtonVPN.Core.Abstract
{
    public abstract class Specification<T> : ISpecification<T>
    {
        public static Specification<T> operator &(Specification<T> left, Specification<T> right)
        {
            return new PredicateSpecification<T>(t => left.IsSatisfiedBy(t) && right.IsSatisfiedBy(t));
        }

        public static Specification<T> operator |(Specification<T> left, Specification<T> right)
        {
            return new PredicateSpecification<T>(t => left.IsSatisfiedBy(t) || right.IsSatisfiedBy(t));
        }

        public static Specification<T> operator !(Specification<T> specification)
        {
            return new PredicateSpecification<T>(t => !specification.IsSatisfiedBy(t));
        }

        public static bool operator true(Specification<T> specification)
        {
            return false;
        }

        public static bool operator false(Specification<T> specification)
        {
            return false;
        }

        public abstract bool IsSatisfiedBy(T item);
    }
}
