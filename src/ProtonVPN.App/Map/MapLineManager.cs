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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Map.ViewModels.MapLine;
using ProtonVPN.Map.ViewModels.Pins;

namespace ProtonVPN.Map
{
    internal class MapLineManager : IVpnStateAware
    {
        private readonly ServerManager _serverManager;
        private List<AbstractPinViewModel> _secureCorePins;
        private Dictionary<string, AbstractPinViewModel> _pins;
        private List<MapLine> _lines = new List<MapLine>();
        private List<MapLine> _secureCoreLines = new List<MapLine>();
        private VpnStatus _vpnStatus;

        public MapLineManager(ServerManager serverManager)
        {
            _serverManager = serverManager;
        }

        private static readonly IReadOnlyList<string> _secureCoreCountries = SecureCoreCountry.CountryCodes;

        public void BuildLines()
        {
            BuildHomeLines();
            BuildSecureCoreLines();
        }

        public void SetSecureCorePins(List<AbstractPinViewModel> pins)
        {
            _secureCorePins = pins;
        }

        public void SetPins(Dictionary<string, AbstractPinViewModel> pins)
        {
            _pins = pins;
        }

        public List<MapLine> GetLines()
        {
            return _lines;
        }

        public List<MapLine> GetSecureCoreLines()
        {
            return _secureCoreLines;
        }

        public void SetSecureCoreLinesVisibility(bool show)
        {
            if (_vpnStatus.Equals(VpnStatus.Connected) && show)
            {
                return;
            }

            var lines = GetSecureCoreLines().OfType<SecureCoreLine>();
            foreach (var line in lines)
            {
                line.Visible = show;
            }
        }

        public void SetExitLinesVisibility(string entryCode, bool show)
        {
            var lines = GetSecureCoreLines().OfType<ExitNodeLine>().Where(l => l.EntryNodeCountry.Equals(entryCode) && !l.Connected);
            foreach (var line in lines)
            {
                line.Visible = show;
            }
        }

        public void DisconnectExitLines()
        {
            var lines = GetSecureCoreLines().OfType<ExitNodeLine>();
            foreach (var line in lines)
            {
                line.Connected = false;
            }
        }

        public void SetSecureCoreHomeLineVisibility(string entryCode, bool show)
        {
            var lines = GetSecureCoreLines().OfType<HomeLine>().Where(l => l.EntryNodeCountry.Equals(entryCode));
            foreach (var line in lines)
            {
                line.Visible = show;
            }
        }

        public void SetEntryLinesVisibility(string exitCode, bool show)
        {
            var lines = GetSecureCoreLines().OfType<ExitNodeLine>().Where(l => l.ExitNodeCountry.Equals(exitCode) && !l.Connected);
            foreach (var line in lines)
            {
                line.Visible = show;
            }
        }

        public void SetHomeLineVisibilityByExitNode(string exitCode, bool show)
        {
            var lines = GetSecureCoreLines().OfType<ExitNodeLine>().Where(l => l.ExitNodeCountry.Equals(exitCode) && !l.Connected);
            foreach (var line in lines)
            {
                var homeLines = GetSecureCoreLines().OfType<HomeLine>().Where(l => l.EntryNodeCountry.Equals(line.EntryNodeCountry) && !l.Connected);
                foreach (var homeLine in homeLines)
                {
                    homeLine.Visible = show;
                }
            }
        }

        public void HideExitLines()
        {
            var lines = GetSecureCoreLines().OfType<ExitNodeLine>().Where(c => !c.Connected);
            foreach (var line in lines)
            {
                line.Visible = false;
            }
        }

        public void HideHomeLines()
        {
            var lines = GetSecureCoreLines().OfType<HomeLine>().Where(c => !c.Connected);
            foreach (var line in lines)
            {
                line.Visible = false;
            }
        }

        public void SetHomeLineVisibility(string entryCode, bool show)
        {
            var lines = GetLines().OfType<HomeLine>().Where(l => l.EntryNodeCountry.Equals(entryCode));
            foreach (var line in lines)
            {
                line.Visible = show;
            }
        }

        public void DisconnectSecureCoreHomeLine()
        {
            var lines = GetSecureCoreLines().OfType<HomeLine>();
            foreach (var line in lines)
            {
                line.Connected = false;
            }
        }

        public void ResetLineStates()
        {
            var lines = GetLines();
            foreach (var line in lines)
            {
                line.Visible = false;
                line.Connected = false;
            }
        }

        public void ResetSecureCoreLineStates()
        {
            var lines = GetSecureCoreLines().OfType<SecureCoreLine>();
            foreach (var line in lines)
            {
                line.Connected = false;
                line.Visible = true;
            }
        }

        public void SetConnectedLines(Server server, bool visible)
        {
            if (server.IsSecureCore())
            {
                SetSecureCoreLinesVisibility(false);
                var lines = GetSecureCoreLines();
                var exitLine = lines.OfType<ExitNodeLine>()
                    .FirstOrDefault(l => l.ExitNodeCountry.Equals(server.ExitCountry) && l.EntryNodeCountry.Equals(server.EntryCountry));

                if (exitLine != null)
                {
                    exitLine.Connected = true;
                }

                var homeLine = lines.OfType<HomeLine>()
                    .FirstOrDefault(l => l.EntryNodeCountry.Equals(server.EntryCountry));
                if (homeLine != null)
                {
                    homeLine.Connected = true;
                    homeLine.Visible = visible;
                }
            }
            else
            {
                var lines = GetLines();
                var homeLine = lines.OfType<HomeLine>()
                    .FirstOrDefault(l => l.EntryNodeCountry.Equals(server.EntryCountry));
                if (homeLine != null)
                {
                    homeLine.Connected = true;
                    homeLine.Visible = visible;
                }
            }
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            return Task.CompletedTask;
        }

        private void BuildHomeLines()
        {
            _lines = _pins
                .Select(pin => new HomeLine(pin.Value))
                .Cast<MapLine>()
                .ToList();
        }

        private void BuildSecureCoreLines()
        {
            _secureCoreLines = BuildExitNodeLines()
                .Union(BuildSecureTriangleLines())
                .ToList();
        }

        private IEnumerable<MapLine> BuildExitNodeLines()
        {
            var lines = new List<MapLine>();
            var servers = _serverManager.GetServers(new SecureCoreServer());

            foreach (var server in servers)
            {
                var pin1 = GetSecureCorePinByCountry(server.EntryCountry);
                var pin2 = GetSecureCorePinByCountry(server.ExitCountry);

                if (pin1 != null && pin2 != null)
                {
                    lines.Add(new ExitNodeLine(pin1, pin2));
                }
            }

            return lines;
        }

        private IEnumerable<MapLine> BuildSecureTriangleLines()
        {
            var lines = new List<MapLine>();

            for (var i = 0; i < _secureCoreCountries.Count; i++)
            {
                var pin1 = GetSecureCorePinByCountry(_secureCoreCountries[i]);
                var pin2 = GetSecureCorePinByCountry(i < _secureCoreCountries.Count - 1
                    ? _secureCoreCountries[i + 1] : _secureCoreCountries[0]);
                if (pin1 == null || pin2 == null) continue;

                lines.Add(
                    new SecureCoreLine(
                        pin1 as SecureCorePinViewModel,
                        pin2 as SecureCorePinViewModel)
                    {
                        Visible = true
                    });

                lines.Add(new HomeLine(pin1));
            }

            return lines;
        }

        private AbstractPinViewModel GetSecureCorePinByCountry(string countryCode)
        {
            return _secureCorePins.FirstOrDefault(c => c.CountryCode.EqualsIgnoringCase(countryCode));
        }
    }
}
