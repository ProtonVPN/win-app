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
using System.Dynamic;
using System.Linq;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Modals;

namespace ProtonVPN.Modals
{
    public class Modals : IModals
    {
        private readonly IComponentContext _container;
        private readonly IWindowManager _windowManager;
        private readonly IModalWindows _modalWindows;
        private readonly IScheduler _scheduler;

        public Modals(
            IScheduler scheduler,
            IComponentContext container,
            IWindowManager windowManager,
            IModalWindows modalWindows)
        {
            _scheduler = scheduler;
            _modalWindows = modalWindows;
            _container = container;
            _windowManager = windowManager;
        }

        public bool? Show<T>(dynamic options = null) where T : IModal
        {
            return _scheduler.Schedule<bool?>(() =>
            {
                if (IsOpen<T>())
                {
                    return false;
                }

                return _windowManager.ShowDialog(Screen<T>(options), null, Settings());
            });
        }

        public bool IsOpen<T>() where T : IModal
        {
            return IsOpen(typeof(T));
        }

        private bool IsOpen(Type type)
        {
            return _modalWindows.List().Any(x => x.DataContext.GetType() == type);
        }

        private dynamic Settings()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            return settings;
        }

        private T Screen<T>(dynamic options = null) where T : IModal
        {
            T screen = _container.Resolve<T>();
            screen.BeforeOpenModal(options);
            return screen;
        }

        public void Close<T>(bool? dialogResult = null) where T : IModal
        {
            _container.Resolve<T>().TryClose(dialogResult);
        }

        public void CloseAll()
        {
            IEnumerable<IModal> dialogs = _container.Resolve<IEnumerable<IModal>>()
                .Where(m => !m.StayOnTop);
            foreach (IModal dialog in dialogs)
            {
                dialog.TryClose(false);
            }
        }

        public bool? Show(Type type, dynamic options = null)
        {
            return _scheduler.Schedule<bool?>(() =>
            {
                if (IsOpen(type))
                {
                    return false;
                }

                return _windowManager.ShowDialog(Screen(type, options), null, Settings());
            });
        }

        private IModal Screen(Type type, dynamic options = null)
        {
            IModal screen = (IModal)_container.Resolve(type);
            screen.BeforeOpenModal(options);
            return screen;
        }
    }
}