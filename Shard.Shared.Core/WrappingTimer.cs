﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shard.Shared.Core
{
    public class WrappingTimer : MarshalByRefObject, ITimer
    {
        private readonly Timer systemTimer;

        public WrappingTimer(Timer systemTimer)
        {
            this.systemTimer = systemTimer;
        }
        
        public bool Change(int dueTime, int period) => systemTimer.Change(dueTime, period);
        public bool Change(long dueTime, long period) => systemTimer.Change(dueTime, period);
        public bool Change(TimeSpan dueTime, TimeSpan period) => systemTimer.Change(dueTime, period);
        public bool Change(uint dueTime, uint period) => systemTimer.Change(dueTime, period);
        public void Dispose()
        {
            systemTimer.Dispose();
            GC.SuppressFinalize(this);
        }
        public ValueTask DisposeAsync() => systemTimer.DisposeAsync();
    }
}