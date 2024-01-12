using System.Collections.Generic;
using System.Linq;

namespace Shard.UNGNUNES.Entities
{
    public class StarSystem
    {
        public StarSystem(string name, List<Planet> planets)
        {
            Name = name;
            Planets = planets;
        }

        public string Name { get; }
        public List<Planet> Planets { get; }
        public Planet SearchPlanet(string name)
        {
            return Planets.FirstOrDefault(planet => Equals(planet.Name, name));
        }
    }
}