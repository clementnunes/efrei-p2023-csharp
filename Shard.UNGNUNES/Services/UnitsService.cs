using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shard.Shared.Core;
using Shard.UNGNUNES.Blueprints;
using Shard.UNGNUNES.Entities;
using Shard.UNGNUNES.Entities.Ships;

namespace Shard.UNGNUNES.Services
{
    public class UnitsService
    {
        public Dictionary<User, List<Unit>> UnitsList { get; }
        private readonly SectorService _sectorService;
        private readonly StarSystemService _starSystemService;
        private readonly IClock _clock;

        public UnitsService(SectorService sectorService, StarSystemService starSystemService, IClock clock)
        {
            UnitsList = new Dictionary<User, List<Unit>>();
            _starSystemService = starSystemService;
            _sectorService = sectorService;
            _clock = clock;
        }

        public UnitLocation CreateUnitLocation(Boolean planetNull)
        {
            StarSystem starSystem = _sectorService.GetRandomSystem();
            UnitLocation unitLocation;
            if (!planetNull)
            {
                Planet planet = _starSystemService.GetRandomPlanet(starSystem);

                unitLocation = new UnitLocation("Initial Location", starSystem.Name, planet.Name,
                    planet.GetResourcesQuantity());
            }
            else
            {
                unitLocation = new UnitLocation("Initial Location", starSystem.Name);
            }

            return unitLocation;
        }

        #nullable enable
        public UnitLocation CreateUnitLocation(StarSystem starSystem, Planet? planet)
        {
            return planet is not null
                ? new UnitLocation("", starSystem.Name, planet.Name, planet.GetResourcesQuantity())
                : new UnitLocation("", starSystem.Name);
        }

        private Unit AddHealthToUnit(Unit unit)
        {
            switch (unit.Type)
            {
                case "fighter":
                    unit.Health = 80;
                    break;
                
                case "bomber":
                    unit.Health = 50;
                    break;
                
                case "cruiser":
                    unit.Health = 400;
                    break;
            }

            return unit;
        }

        public Unit CreateUnit(string id, string type, User owner, UnitLocation? unitLocation)
        {
            Unit createdUnit = unitLocation is not null ? new Unit(id, type, unitLocation) : new Unit(id, type);
            
            createdUnit = AddHealthToUnit(createdUnit);

            if (!UnitsList.ContainsKey(owner))
            {
                List<Unit> unitsList = new List<Unit>();
                unitsList.Add(createdUnit);
                UnitsList.Add(owner, unitsList);
            }
            else
            {
                List<Unit>? userUnits = SearchUnitsByPlayer(owner.Id);

                if (userUnits is not null)
                    userUnits.Add(createdUnit);
            }

            if (createdUnit.Type != "scout" && createdUnit.Type != "builder")
                AddHealthToUnit(createdUnit);
            if (createdUnit.Type == "cargo")
                createdUnit.ResourcesQuantity = new Dictionary<string, int>();

            return createdUnit;
        }

#nullable enable
        public List<Unit>? SearchUnitsByPlayer(string playerId)
        {
            return (from userUnit in UnitsList where String.Equals(userUnit.Key.Id, playerId) select userUnit.Value)
                .FirstOrDefault();
        }

#nullable enable
        public Unit? SearchUnitByPlayer(string playerId, string unitId)
        {
            List<Unit>? fetchedUnits = SearchUnitsByPlayer(playerId);

            if (fetchedUnits is null)
                return null;

            return fetchedUnits.FirstOrDefault(unit => String.Equals(unitId, unit.Id));
        }

#nullable enable
        public UnitLocation? SearchUnitLocationByPlayer(string playerId, string unitId)
        {
            Unit? fetchedUnit = SearchUnitByPlayer(playerId, unitId);

            if (fetchedUnit is null)
                return null;

            if (fetchedUnit.Type == "scout")
                return fetchedUnit.GetUnitLocation();

            return fetchedUnit.GetHiddenUnitLocation();
        }

        public void DeleteUnit(Unit unit)
        {
            foreach (var userUnits in UnitsList)
            {
                userUnits.Value.Remove(unit);
            }
        }

        // TODO: Trigger ship fights
        public Unit MoveUnit(User user, Unit fetchedUnit, StarSystem? starSystem, Planet? planet)
        {
            UnitLocation unitLocation = CreateUnitLocation(false);

            if (user is null || fetchedUnit is null)
            {
                throw new ArgumentNullException();
            }
            
            if (starSystem is not null)
            {
                unitLocation = CreateUnitLocation(starSystem, planet);
            }
            else
            {
                if (starSystem is null)
                {
                    throw new ArgumentNullException();
                }
                
                fetchedUnit.DestinationSystem = starSystem.Name;
            }

            if (fetchedUnit.System != fetchedUnit.DestinationSystem)
            {
                unitLocation = CreateUnitLocation(starSystem, null);
                fetchedUnit.SetUnitLocation(unitLocation);
                fetchedUnit.EstimatedTimeOfArrival = (_clock.Now + TimeSpan.FromSeconds(60)).ToString();
            }

            if (fetchedUnit.GetUnitLocation().Planet != fetchedUnit.DestinationPlanet)
            {
                unitLocation = CreateUnitLocation(starSystem, planet);
                fetchedUnit.SetUnitLocation(unitLocation);
                if (fetchedUnit.DestinationPlanet == null)
                    fetchedUnit.Planet = null;
                DateTime estimated;

                if (fetchedUnit.EstimatedTimeOfArrival is not null)
                {
                    estimated = DateTime.Parse(fetchedUnit.EstimatedTimeOfArrival) + TimeSpan.FromSeconds(15);
                }
                else
                {
                    estimated = _clock.Now + TimeSpan.FromSeconds(15);
                }
                
                fetchedUnit.EstimatedTimeOfArrival = estimated.ToString();
            }

            return fetchedUnit;
        }
        
        public List<Unit> GetAllUnits()
        {
            List<Unit> unitList = new List<Unit>();
            List<List<Unit>> fetchedUnits = (from units in UnitsList select units.Value).ToList();

            return fetchedUnits.Aggregate(unitList, (current, units) => current.Concat(units).ToList());
        }

        public List<Unit> GetUnitsFromLocation(UnitLocation unitLocation)
        {
            return (from unit in GetAllUnits() where unit.System == unitLocation.System && unit.Planet == unitLocation.Planet select unit).ToList();
        }
        
        public void IncreaseResource(Unit unit, ResourceKind resourceKind, int value)
        {
            if (unit.ResourcesQuantity.ContainsKey(resourceKind.ToString().ToLower()))
            {
                unit.ResourcesQuantity[resourceKind.ToString().ToLower()] += value;
            }
            else unit.ResourcesQuantity.Add(resourceKind.ToString().ToLower(),value);
        }
        
        public bool DecreaseResource(Unit unit, ResourceKind resourceKind, int value)
        {
            if (unit.ResourcesQuantity.ContainsKey(resourceKind.ToString().ToLower()))
            {
                unit.ResourcesQuantity[resourceKind.ToString().ToLower()] -= value;
                return true;
            }
            else return false;
        }

        public bool CompareResources(Unit unit1, UnitBlueprint unit2)
        {
            if (unit1.ResourcesQuantity.Count != unit2.ResourcesQuantity.Count)
                return false;
            
            if (unit1.ResourcesQuantity.Any(resource => !unit2.ResourcesQuantity.ContainsKey(resource.Key)))
                return false;

            return unit1.ResourcesQuantity.All(resource => resource.Value == unit2.ResourcesQuantity[resource.Key]);
        }
    }
}