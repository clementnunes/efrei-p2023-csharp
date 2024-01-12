using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Shard.UNGNUNES.Entities.Buildings
{
    public class StarPort : Building
    {
        public List<Unit> Queue { get; set; }
        
        public StarPort(string id, string type, string resourceCategory, string builderId, string system, string planet, DateTime now) : base(id, type, resourceCategory, builderId, system, planet, now)
        {
            Queue = new List<Unit>();
        }
    }
}