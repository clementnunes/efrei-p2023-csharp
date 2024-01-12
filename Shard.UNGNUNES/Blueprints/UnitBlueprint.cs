using System.Collections.Generic;

namespace Shard.UNGNUNES.Blueprints
{
    public class UnitBlueprint
    {
        public string Id { get; set; }
        public string System { get; set; }
        public string Planet { get; set; }
        public string Type { get; set; }
        public string BuilderId { get; set; }
        public Dictionary<string,int> ResourcesQuantity { get; set; }
        public string DestinationSystem { get; set; }
        public string DestinationPlanet { get; set; }
        public int ?Health { get; set; }
    }
}