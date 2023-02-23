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

using ProtonVPN.Common.Helpers;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Settings.Contracts;
using System;
using System.IO;

namespace ProtonVPN.Settings.SplitTunneling
{
    public class AppViewModel : ViewModel, IEquatable<AppViewModel>
    {
        private readonly SplitTunnelingApp _dataSource;

        public AppViewModel(SplitTunnelingApp dataSource)
        {
            Ensure.NotNull(dataSource, nameof(dataSource));

            _dataSource = dataSource;
        }

        public string Name => _dataSource.Name; 
        public string Path => _dataSource.Path;

        public bool FileExists => File.Exists(_dataSource.Path);

        private bool? _enabled;
        public bool Enabled
        {
            get => _enabled ?? (_enabled = _dataSource.Enabled).Value;
            set => Set(ref _enabled, value);
        }

        public bool Suggested { get; set; }

        public SplitTunnelingApp DataSource() => new SplitTunnelingApp
        {
            Name = _dataSource.Name,
            Path = _dataSource.Path,
            AdditionalPaths = _dataSource.AdditionalPaths,
            Enabled = Enabled
        };

        #region IEquatable

        public bool Equals(AppViewModel other)
        {
            if (other == null)
                return false;

            return _dataSource.Equals(other.DataSource());
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AppViewModel);
        }

        public override int GetHashCode()
        {
            return _dataSource.GetHashCode();
        }

        #endregion
    }
}
