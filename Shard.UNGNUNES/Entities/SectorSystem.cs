using System.Collections.Generic;

namespace Shard.UNGNUNES.Entities
{
    public class SectorSystem
    {
        public SectorSystem()
        {
            StarSystems = new List<StarSystem>();
        }

        public List<StarSystem> StarSystems { get; }
    }
}