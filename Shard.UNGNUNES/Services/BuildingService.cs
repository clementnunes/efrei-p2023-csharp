using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Shard.Shared.Core;
using Shard.UNGNUNES.Entities;
using Shard.UNGNUNES.Entities.Ships;

namespace Shard.UNGNUNES.Services
{
    public class BuildingService
    {
        private readonly UsersService _usersService;
        private readonly UnitsService _unitsService;
        private readonly PlanetService _planetService;
        private readonly SectorService _sectorService;
        private readonly IClock _clock;
        private Dictionary<User, List<Building>> BuildingsList {get; }
        private Dictionary< Building,CancellationTokenSource> TokensList { get; }

        public BuildingService(UsersService usersService, UnitsService unitsService, PlanetService planetService, SectorService sectorService, IClock clock)
        {
            BuildingsList = new Dictionary<User, List<Building>>();
            TokensList = new Dictionary<Building, CancellationTokenSource>();
            _usersService = usersService;
            _unitsService = unitsService;
            _planetService = planetService;
            _sectorService = sectorService;
            _clock = clock;
        }
        
        #nullable enable
        public Building? CreateBuilding(string userId, string? builderId, string? resourceCategory, string buildingId, string? type)
        {
            if (builderId is null || type is null || resourceCategory is null)
                throw new ArgumentNullException();
            
            User? user = _usersService.SearchUser(userId);

            if (user is null)
                throw new KeyNotFoundException();
            
            Unit? fetchedUnit = _unitsService.SearchUnitByPlayer(userId, builderId);

            if (fetchedUnit is null || fetchedUnit.Type != "builder" || fetchedUnit.Planet is null)
                return null;

            Building createdBuilding = new Building(buildingId, type, resourceCategory, builderId, fetchedUnit.System,
                fetchedUnit.Planet, _clock.Now);
            
            if (!BuildingsList.ContainsKey(user))
            {
                List<Building> buildingsList = new List<Building>();
                buildingsList.Add(createdBuilding);
                BuildingsList.Add(user, buildingsList);
            }
            else
            {
                List<Building>? userBuildings = SearchBuildingsByPlayer(user.Id);

                if (userBuildings is not null)
                    userBuildings.Add(createdBuilding);
            }

            return createdBuilding;
        }
        
        public List<Building>? SearchBuildingsByPlayer(string playerId)
        {
            return (from userBuilding in BuildingsList where String.Equals(userBuilding.Key.Id, playerId) select userBuilding.Value)
                .FirstOrDefault();
        }
        
        #nullable enable
        public List<Building>? SearchBuildingsByPlayerAndBuilderId(string playerId, string builderId)
        {
            List<Building>? buildings = SearchBuildingsByPlayer(playerId);

            if (buildings is null)
                return null;
            
            foreach (var building in buildings)
            {
                if (building.BuilderId != builderId)
                    buildings.Remove(building);
            }

            return buildings;
        }
        
        #nullable enable
        public Building? SearchBuildingByPlayer(string playerId, string buildingId)
        {
            List<Building>? fetchedBuildings = SearchBuildingsByPlayer(playerId);

            if (fetchedBuildings is null)
                return null;

            return fetchedBuildings.FirstOrDefault(building => String.Equals(buildingId, building.Id));
        }
        
        public void CancelUnbuiltBuildingsOfUnit(string userId, string unitId)
        {
            List<Building>? fetchedBuildings = SearchBuildingsByPlayer(userId);

            if (fetchedBuildings is not null)
            {
                foreach (Building building in  BuildingsList[_usersService.SearchUser(userId)])
                {
                    if (building.IsBuilt == false && building.BuilderId == unitId)
                    {
                        cancelBuilding(building);
                    }
                }
                BuildingsList[_usersService.SearchUser(userId)].RemoveAll(building => building.IsBuilt == false && building.BuilderId == unitId);
            }
        }

        public void TransferResource(Planet planet, Building mine, User user)
        {
            switch (mine.ResourceCategory)
            {
                case ("liquid"):
                    if (_planetService.DecreaseResource(planet, ResourceKind.Water))
                        _usersService.IncreaseResource(user, ResourceKind.Water);
                    break;
                case ("gaseous"):
                    if (_planetService.DecreaseResource(planet, ResourceKind.Oxygen))
                        _usersService.IncreaseResource(user, ResourceKind.Oxygen);
                    break;
                case ("solid"):
                    ResourceKind mostPresentResource = _planetService.GetMostPresentSolidResourceKind(planet);
                    if (_planetService.DecreaseResource(planet, mostPresentResource))
                        _usersService.IncreaseResource(user, mostPresentResource);

                    break;
            }
        }

        public void StartExtracting(Building mine, string userId)
        {
            Planet planet = _sectorService.SearchSystem(mine.System).SearchPlanet(mine.Planet);
            User user = _usersService.SearchUser(userId);
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            _clock.CreateTimer(_ => TransferResource(planet, mine, user), autoResetEvent, TimeSpan.FromMinutes(6),TimeSpan.FromMinutes(1));
        }

        public Building? SearchBuildingByUserAndBuildingId(string userId, string buildingId)
        {
            List<Building>? buildings = SearchBuildingsByPlayer(userId);

            if (buildings is null)
                return null;

            foreach (var building in buildings)
            {
                if (String.Equals(building.Id, buildingId))
                    return building;
            }

            return null;
        }

        public bool ConsumeResources(string unitType, User user)
        {
            Dictionary<ResourceKind, int> requiredQuantities = new Dictionary<ResourceKind, int>();

            switch (unitType)
            {
                case "scout":
                    requiredQuantities.Add(ResourceKind.Carbon, 5);
                    requiredQuantities.Add(ResourceKind.Iron, 5);
                    break;
                case "builder":
                    requiredQuantities.Add(ResourceKind.Carbon, 5);
                    requiredQuantities.Add(ResourceKind.Iron, 10);
                    break;
                case "fighter":
                    requiredQuantities.Add(ResourceKind.Iron, 20);
                    requiredQuantities.Add(ResourceKind.Aluminium, 10);
                    break;
                case "bomber":
                    requiredQuantities.Add(ResourceKind.Iron, 30);
                    requiredQuantities.Add(ResourceKind.Titanium, 10);
                    break;
                case "cruiser":
                    requiredQuantities.Add(ResourceKind.Iron, 60);
                    requiredQuantities.Add(ResourceKind.Gold, 20);
                    break;
                case "cargo":
                    requiredQuantities.Add(ResourceKind.Carbon, 10);
                    requiredQuantities.Add(ResourceKind.Iron, 10);
                    requiredQuantities.Add(ResourceKind.Gold, 5);
                    break;

                default:
                    return false;
            }

            if (_usersService.HasEnoughResources(user, requiredQuantities) is false)
                return false;
            
            foreach (var requiredQuantity in requiredQuantities)
            {
                _usersService.DecreaseResource(user, requiredQuantity.Key, requiredQuantity.Value);
            }

            return true;
        }

        public void addToken(Building building, CancellationTokenSource tokenSource)
        {
            if (TokensList.ContainsKey(building)) return;
            
            TokensList.Add(building, tokenSource);

        }

        private void cancelBuilding(Building building)
        {
            if (!TokensList.ContainsKey(building)) return;
            
            CancellationTokenSource tokenSource = TokensList[building];
            tokenSource.Cancel();
            TokensList.Remove(building);
        }
    }
}