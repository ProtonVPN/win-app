/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System.Windows;

namespace ProtonVPN.Resource
{
    public class BasePopupWindow : WindowBase
    {
        private bool _isStartupPositionSet;

        public BasePopupWindow()
        {
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetStartupPosition();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            SetStartupPosition();
        }

        private void SetStartupPosition()
        {
            object dataContext = DataContext;
            if (!_isStartupPositionSet && dataContext != null && ActualWidth > 0 && ActualHeight > 0)
            {
                Window owner = ((dynamic)dataContext).Owner;
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = owner.Left + ((owner.ActualWidth - ActualWidth) / 2);
                Top = owner.Top + ((owner.ActualHeight - ActualHeight) / 2);
                _isStartupPositionSet = true;
            }
        }
    }
}
