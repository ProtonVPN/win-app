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

using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Contracts.Bases.ViewModels;

public abstract partial class ActivatableViewModelBase : ViewModelBase, IActivationAware
{
    public bool IsActive { get; private set; }

    protected ActivatableViewModelBase(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizer, logger, issueReporter)
    { }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;

            OnActivated();
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;

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