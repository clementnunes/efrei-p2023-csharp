using System.Collections.Generic;
using Shard.Shared.Core;

namespace Shard.UNGNUNES.Entities
{
    public class UnitLocation
    {
        public string Description { get; }
        public IDictionary<string, int> ResourcesQuantity { get; private set; }
        public string System { get; }
        public string Planet { get; }

        public UnitLocation(string description, string starSystem)
        {
            Description = description;
            System = starSystem;
            Planet = null;
            ResourcesQuantity = null;
        }
        
        public UnitLocation(string description, string starSystem, string planet,
            IDictionary<ResourceKind, int> resourcesQuantity) : this (description, starSystem)
        {
            Planet = planet;
            if (resourcesQuantity is not null)
                SetResourcesQuantity(resourcesQuantity);
        }
        
        public static IDictionary<string, int> CastDictionary(IDictionary<ResourceKind, int> resourcesQuantity)
        {
            Dictionary<string, int> castResourcesQuantity = new Dictionary<string, int>();

            foreach (KeyValuePair<ResourceKind, int> resourceQuantity in resourcesQuantity)
            {
                castResourcesQuantity.Add(resourceQuantity.Key.ToString().ToLower(), resourceQuantity.Value);
            }

            return castResourcesQuantity;
        }
        
        public void SetResourcesQuantity(IDictionary<ResourceKind, int> resourcesQuantity)
        {
            ResourcesQuantity = CastDictionary(resourcesQuantity);
        }
    }
}