using System;

namespace Shard.UNGNUNES.Entities.Buildings
{
    public class Mine : Building
    {
        public Mine(string id, string type, string resourceCategory, string builderId, string system, string planet, DateTime now) : base(id, type, resourceCategory, builderId, system, planet, now)
        {
        }
    }
}