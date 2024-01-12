using System.Collections.Generic;
using System.Linq;
using Shard.Shared.Core;
using Shard.UNGNUNES.Entities;

namespace Shard.UNGNUNES.Services
{
    public class PlanetService
    {
        public Planet CreatePlanet(string name, int size)
        {
            return new Planet(name, size);
        }
        
        public Planet CreatePlanet(string name, int size, IReadOnlyDictionary<ResourceKind, int> resourcesQuantity)
        {
            Dictionary<ResourceKind, int> resourceBundle = 
                resourcesQuantity
                    .ToDictionary(resourceQuantity => resourceQuantity.Key, resourceQuantity => resourceQuantity.Value);

            return new Planet(name, size, resourceBundle);
        }

        public bool DecreaseResource(Planet planet, ResourceKind resourceKind)
        {
            if (planet.GetResourcesQuantity().ContainsKey(resourceKind))
            {
                if (planet.GetResourcesQuantity()[resourceKind] > 0)
                    planet.GetResourcesQuantity()[resourceKind] -= 1;
                else return false;
            }

            return true;
        }

        public ResourceKind GetMostPresentSolidResourceKind(Planet planet)
        {
            IDictionary<ResourceKind, int> resources = planet.GetResourcesQuantity();

            resources.Remove(ResourceKind.Water);
            resources.Remove(ResourceKind.Oxygen);
            
            ResourceKind rarestResource = ResourceKind.Carbon;

            List<ResourceKind> fetchedResources =  (from resource in resources where resource.Value == resources.Values.Max() select resource.Key)
                .ToList();

            if (fetchedResources.Count > 1)
            {
                bool exitForeach = false;
                foreach (var resource in fetchedResources)
                {
                    switch (rarestResource)
                    {
                        case ResourceKind.Carbon:
                            rarestResource = resource;
                            break;
                        case ResourceKind.Iron:
                            if (resource != ResourceKind.Carbon)
                                rarestResource = resource;
                            break;
                        case ResourceKind.Aluminium:
                            if (resource != ResourceKind.Carbon && resource != ResourceKind.Iron)
                                rarestResource = resource;
                            break;
                        case ResourceKind.Gold:
                            if (resource == ResourceKind.Titanium)
                                rarestResource = ResourceKind.Titanium;
                            exitForeach = true;
                            break;
                        case ResourceKind.Titanium:
                            exitForeach = true;
                            break;
                    }

                    if (exitForeach)
                        break;
                }
            }
            else if (fetchedResources.Count > 0)
                rarestResource = fetchedResources[0];

            return rarestResource;
        }
    }
}