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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ProtonVPN.Tests.Common.Breakpoints
{
    public class Breakpoint : IDisposable
    {
        private readonly SemaphoreSlim _breakSemaphore = new SemaphoreSlim(0);
        private readonly ConcurrentQueue<BreakpointHit> _queue = new ConcurrentQueue<BreakpointHit>();

        public BreakpointHit Hit()
        {
            var breakpoint = new BreakpointHit();
            _queue.Enqueue(breakpoint);
            _breakSemaphore.Release(1);
            return breakpoint;
        }

        public async Task<BreakpointHit> WaitForHit()
        {
            await _breakSemaphore.WaitAsync();
            _queue.TryDequeue(out BreakpointHit result);
            return result;
        }

        public async Task WaitForHitAndContinue()
        {
            BreakpointHit hit = await WaitForHit();
            hit.Continue();
        }

        public void Dispose()
        {
            _breakSemaphore.Dispose();
            foreach (BreakpointHit hit in _queue)
            {
                hit.Dispose();
            }
        }
    }
}
