using System.Collections.Generic;
using System.Runtime.Serialization;
using Shard.Shared.Core;

namespace Shard.UNGNUNES.Entities
{
    public class Planet
    {
        public string Name { get; }
        public int Size { get; }

        [IgnoreDataMember] 
        private IDictionary<ResourceKind, int> ResourcesQuantity;
        
        public Planet(string name, int size)
        {
            Name = name;
            Size = size;
        }
        
        public Planet(string name, int size, IDictionary<ResourceKind, int> resourceQuantity)
        {
            Name = name;
            Size = size;
            ResourcesQuantity = resourceQuantity;
        }

        public IDictionary<ResourceKind, int> GetResourcesQuantity()
        {
            return ResourcesQuantity;
        }
    }
}