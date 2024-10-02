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

namespace ProtonVPN.Client.Legacy.Contracts.ViewModels;

public abstract partial class ActivatableViewModelBase : ViewModelBase
{
    private bool _isActive;

    /// <summary>
    /// Gets or sets a value indicating whether the current view model is currently active.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (SetProperty(ref _isActive, value))
            {
                if (value)
                {
                    OnActivated();
                }
                else
                {
                    OnDeactivated();
                }
            }
        }
    }

    public ActivatableViewModelBase(ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizationProvider, logger, issueReporter)
    { }

    /// <summary>
    /// Invoked whenever the <see cref="IsActive"/> property is set to <see langword="true"/>.
    /// </summary>
    protected virtual void OnActivated()
    { }

    /// <summary>
    /// Invoked whenever the <see cref="IsActive"/> property is set to <see langword="false"/>.
    /// </summary>
    protected virtual void OnDeactivated()
    { }
}