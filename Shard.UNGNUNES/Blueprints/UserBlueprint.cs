using System.Collections.Generic;
using Shard.Shared.Core;

namespace Shard.UNGNUNES.Blueprints
{
    public class UserBlueprint
    {
        public string Id { get; set; }
        public string Pseudo { get; set; }
        
        public string DateOfCreation { get; set; }
        public Dictionary<ResourceKind, int> ResourcesQuantity { get; set; }
    }
}