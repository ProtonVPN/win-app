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

namespace ProtonVPN.Client.Core.Bases.ViewModels;

public abstract partial class ActivatableViewModelBase : ViewModelBase, IActivationAware
{
    private int _activationCount = 0;

    public bool IsActive => _activationCount > 0;

    protected ActivatableViewModelBase(IViewModelHelper viewModelHelper) : base(viewModelHelper)
    {
    }

    public void Activate()
    {
        bool wasInactive = !IsActive;

        _activationCount++;

        if (wasInactive && IsActive)
        {
            OnActivated();
        }
    }

    public void Deactivate()
    {
        bool wasActive = IsActive;

        _activationCount--;

        if (wasActive && !IsActive)
        {
            OnDeactivated();
        }
    }

    protected virtual void OnActivated()
    {
        InvalidateAllProperties();
    }

    protected virtual void OnDeactivated()
    { }
}