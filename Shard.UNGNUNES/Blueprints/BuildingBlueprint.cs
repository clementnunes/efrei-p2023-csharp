namespace Shard.UNGNUNES.Blueprints
{
    public class BuildingBlueprint
    {
        public string id { get; init; }
        public string Type { get; init; }
        public string ResourceCategory { get; init; }
        public string BuilderId { get; init; }

        public bool isBuilt { get; init; }
        
        public string estimatedBuildTime { get; init; }
    }
}