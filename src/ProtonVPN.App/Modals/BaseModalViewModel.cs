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

using ProtonVPN.Core.Modals;
using ProtonVPN.Windows;

namespace ProtonVPN.Modals
{
    public abstract class BaseModalViewModel : BaseWindowViewModel, IModal
    {
        private bool _loading;
        protected bool Loaded;

        public bool StayOnTop { get; protected set; }

        public bool Loading
        {
            get => _loading;
            set => Set(ref _loading, value);
        }

        public override void CloseAction()
        {
            TryCloseWithFailure();
        }

        public void TryCloseWithSuccess()
        {
            TryClose(true);
        }

        public void TryCloseWithFailure()
        {
            TryClose(false);
        }

        public virtual void BeforeOpenModal(dynamic options)
        {
        }
    }
}