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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Update.Releases;

namespace ProtonVPN.Update.Updates
{
    /// <summary>
    /// Represents app update state and an interface to update related operations.
    /// Operations are performed by <see cref="AppUpdates"/>.
    /// </summary>
    public class AppUpdate : IAppUpdate
    {
        private readonly InternalState _state;

        public AppUpdate(AppUpdates appUpdates): this(
            new InternalState
            {
                AppUpdates = appUpdates,
                Releases = new List<Release>(),
                NewRelease = Release.EmptyRelease()
            })
        {
        }

        private AppUpdate(InternalState state)
        {
            _state = state;
        }

        public bool Available => !_state.NewRelease.Empty();

        public bool Ready => _state.Ready;

        public string FilePath
        {
            get
            {
                if (_state.NewRelease.New)
                {
                    return _state.AppUpdates.FilePath(_state.NewRelease);
                }

                return string.Empty;
            }
        }

        public string FileArguments
        {
            get
            {
                if (_state.NewRelease.New)
                {
                    return _state.NewRelease.File.Arguments;
                }

                return string.Empty;
            }
        }

        public IReadOnlyList<IRelease> ReleaseHistory()
        {
            return _state.EarlyAccess ? GetAllReleases() : GetStableReleases();
        }

        public async Task<IAppUpdate> Latest(bool earlyAccess)
        {
            IReadOnlyList<Release> releases = await _state.AppUpdates.ReleaseHistory(earlyAccess);
            return WithReleases(releases, earlyAccess);
        }

        public IAppUpdate CachedLatest(bool earlyAccess)
        {
            return WithReleases(_state.Releases, earlyAccess);
        }

        public async Task<IAppUpdate> Downloaded()
        {
            if (Available && !Ready)
                await _state.AppUpdates.Download(_state.NewRelease);

            return this;
        }

        public async Task<IAppUpdate> Validated()
        {
            bool valid = await _state.AppUpdates.Valid(_state.NewRelease);
            return WithReady(valid);
        }

        public async Task<IAppUpdate> Started(bool auto)
        {
            if (auto && _state.NewRelease.DisableAutoUpdate)
            {
                throw new AppUpdateException("Automatic update to this release is disabled");
            }

            await _state.AppUpdates.StartUpdate(_state.NewRelease);

            return this;
        }

        private AppUpdate WithReleases(IReadOnlyList<Release> releases, bool earlyAccess)
        {
            Release newRelease = NewRelease(releases, earlyAccess);
            bool releaseChanged = !Equals(newRelease, _state.NewRelease);

            InternalState state = _state.Clone();
            state.EarlyAccess = earlyAccess;
            state.Releases = releases;
            state.NewRelease = newRelease;
            if (releaseChanged)
                state.Ready = false;

            return new AppUpdate(state);
        }

        private AppUpdate WithReady(bool ready)
        {
            InternalState state = _state.Clone();
            state.Ready = ready;
            return new AppUpdate(state);
        }

        private IReadOnlyList<IRelease> GetAllReleases()
        {
            return _state.Releases.ToList();
        }

        private IReadOnlyList<IRelease> GetStableReleases()
        {
            return _state.Releases
                .SkipWhile(r => r.EarlyAccess && r.New)
                .ToList();
        }

        private static Release NewRelease(IEnumerable<Release> releases, bool earlyAccess)
        {
            return releases
                   .FirstOrDefault(r => r.New && (!r.EarlyAccess || earlyAccess))
                   ?? Release.EmptyRelease();
        }

        private static bool Equals(Release one, Release other)
        {
            return one.Version.Equals(other.Version) &&
                   (one.File == null && other.File == null ||
                    one.File != null && other.File != null && one.File.Equals(other.File));
        }
    }
}
