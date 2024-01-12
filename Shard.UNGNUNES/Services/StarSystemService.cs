using System;
using System.Collections.Generic;
using System.Linq;
using Shard.Shared.Core;
using Shard.UNGNUNES.Entities;

namespace Shard.UNGNUNES.Services
{
    public class StarSystemService
    {
        private readonly PlanetService _planetService;

        public StarSystemService(PlanetService planetService)
        {
            _planetService = planetService;
        }
        public List<Planet> GeneratePlanets(SystemSpecification systemSpecification)
        {
            List<Planet> planets = new List<Planet>();
            var planetsSpec = systemSpecification.Planets;

            /*return planetsSpec
                .Select(planetSpec => _planetService.CreatePlanet(planetSpec.Name, planetSpec.Size, planetSpec.ResourceQuantity))
                .ToList();*/

            foreach (var planetSpec in planetsSpec)
            {
                planets.Add(_planetService.CreatePlanet(planetSpec.Name, planetSpec.Size, planetSpec.ResourceQuantity));
            }

            return planets;
        }

        public StarSystem GenerateStarSystem(SystemSpecification systemSpecification)
        {
            return new StarSystem(systemSpecification.Name, GeneratePlanets(systemSpecification));
        }
        
        public Planet GetRandomPlanet(StarSystem starSystem)
        {
            Random rnd = new Random();
            int randomId = rnd.Next(starSystem.Planets.Count);

            return starSystem.Planets[randomId];
        }
        
        #nullable enable
        public Planet? GetPlanet(StarSystem? starSystem, string planetName)
        {
            Planet? planet = starSystem?.SearchPlanet(planetName);

            return planet;
        }
    }
}