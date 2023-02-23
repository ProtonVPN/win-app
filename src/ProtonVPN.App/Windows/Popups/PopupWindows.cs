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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Windows.Popups;
using ProtonVPN.Resource;

namespace ProtonVPN.Windows.Popups
{
    public class PopupWindows : IPopupWindows
    {
        private readonly IComponentContext _container;
        private readonly IWindowManager _windowManager;
        private readonly IScheduler _scheduler;

        public PopupWindows(
            IScheduler scheduler,
            IComponentContext container,
            IWindowManager windowManager)
        {
            _scheduler = scheduler;
            _container = container;
            _windowManager = windowManager;
        }

        public void Show<T>(dynamic options = null) where T : IPopupWindow
        {
            T screen = _container.Resolve<T>();
            _scheduler.Schedule(() =>
            {
                if (!IsOpen<T>())
                {
                    _windowManager.ShowWindow(screen);
                }
            });
        }

        public bool IsOpen<T>() where T : IPopupWindow
        {
            return IsOpen(typeof(T));
        }

        private bool IsOpen(Type type)
        {
            return GetAll().Any(x => x.DataContext.GetType() == type);
        }

        private IEnumerable<BasePopupWindow> GetAll()
        {
            return Application.Current.Windows.OfType<BasePopupWindow>();
        }

        public void Close<T>() where T : IPopupWindow
        {
            _container.Resolve<T>().TryClose();
        }

        public void CloseAll()
        {
            IEnumerable<IPopupWindow> windows = _container.Resolve<IEnumerable<IPopupWindow>>();
            foreach (IPopupWindow window in windows)
            {
                window.TryClose();
            }
        }

        public void Show(Type type, dynamic options = null)
        {
            IPopupWindow screen = (IPopupWindow)_container.Resolve(type);
            _scheduler.Schedule(() =>
            {
                if (!IsOpen(type))
                {
                    _windowManager.ShowWindow(screen);
                }
            });
        }
    }
}
