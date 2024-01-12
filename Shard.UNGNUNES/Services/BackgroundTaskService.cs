using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shard.Shared.Core;
using Shard.UNGNUNES.Services;

namespace Shard.UNGNUNES
{
    public class BackgroundTaskService : IHostedService
    {
        private readonly IClock _clock;
        private readonly ShipFightingService _shipFightingService;
        private ITimer _timer;
            
        public BackgroundTaskService(IClock clock, ShipFightingService shipFightingService)
        {
            _clock = clock;
            _shipFightingService = shipFightingService;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = _clock.CreateTimer(_ => _shipFightingService.ShipFight(_clock), null,
                TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}