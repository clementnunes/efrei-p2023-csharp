using System;
using System.Linq;
using Shard.Shared.Core;
using Shard.UNGNUNES.Controllers;
using Shard.UNGNUNES.Entities;

namespace Shard.UNGNUNES.Services
{
    public class SectorService
    {
        public SectorSystem SectorSystem { get; }
        private StarSystemService StarSystemService { get; }
        
        public SectorService(StarSystemService starSystemService, MapGenerator mapGenerator)
        {
            SectorSystem = new SectorSystem();
            StarSystemService = starSystemService;
            
            GenerateSystems(mapGenerator);
        }
        
        public void GenerateSystems(MapGenerator generator)
        {
            var sectorSpec = generator.Generate();

            foreach (var systemSpec in sectorSpec.Systems)
            {
                SectorSystem.StarSystems.Add(StarSystemService.GenerateStarSystem(systemSpec));
            }
        }

        public StarSystem SearchSystem(string name)
        {
            return SectorSystem.StarSystems.First(system => Equals(system.Name, name));
        }

        public StarSystem GetRandomSystem()
        {
            Random rnd = new Random();
            int randomId = rnd.Next(SectorSystem.StarSystems.Count);

            return SectorSystem.StarSystems[randomId];
        }

        public Planet GetRandomPlanet()
        {
            StarSystem starSystem = GetRandomSystem();
            Planet randomPlanet = StarSystemService.GetRandomPlanet(starSystem);
            return randomPlanet;
        }
        
        public Planet GetRandomPlanet(StarSystem starSystem)
        {
            Planet randomPlanet = StarSystemService.GetRandomPlanet(starSystem);
            return randomPlanet;
        }
    }
}