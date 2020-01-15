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
                if (ModalOpened<T>())
                    return false;

                return _windowManager.ShowDialog(Screen<T>(options), null, Settings());
            });
        }

        public void Close<T>(bool? dialogResult = null) where T : IModal
        {
            _container.Resolve<T>().TryClose(dialogResult);
        }

        public void CloseAll()
        {
            var dialogs = _container.Resolve<IEnumerable<IModal>>().Where(m => !m.StayOnTop);
            foreach (var dialog in dialogs)
            {
                dialog.TryClose(false);
            }
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
            var screen = _container.Resolve<T>();
            screen.BeforeOpenModal(options);
            return screen;
        }

        private bool ModalOpened<T>()
        {
            return _modalWindows.List().Any(x => x.DataContext.GetType() == typeof(T));
        }
    }
}
