using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Shard.Shared.Core;
using Shard.UNGNUNES.Blueprints;
using Shard.UNGNUNES.Services;
using Shard.UNGNUNES.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Shard.UNGNUNES.Controllers
{
    [Route("users/{userId}")]
    [ApiController]
    public class UnitsController : ControllerBase
    {
        private readonly UnitsService _unitsService;
        private readonly UsersService _usersService;
        private readonly SectorService _sectorService;
        private readonly BuildingService _buildingService;
        private readonly ShipFightingService _shipFightingService;
        private readonly IClock _clock;

        public UnitsController(UnitsService unitsService, UsersService usersService, ShipFightingService shipFightingService,
            SectorService sectorService, BuildingService buildingService, IClock clock)
        {
            _unitsService = unitsService;
            _usersService = usersService;
            _sectorService = sectorService;
            _buildingService = buildingService;
            _shipFightingService = shipFightingService;
            _clock = clock;
        }

        [HttpGet("[controller]")]
        [SwaggerOperation(Summary = "Returns all units of a user.")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<Unit>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.NotFound)]
        public ActionResult<List<Unit>> GetUserUnits(string userId)
        {
            List<Unit> units = _unitsService.SearchUnitsByPlayer(userId);
            return (units is null) ? new List<Unit>() : units;
        }
        

        [HttpGet("[controller]/{unitId}")]
        [SwaggerOperation(Summary = "Return information about one single unit of a user.")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Unit), 200)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.NotFound)]
        #nullable enable
        public async Task<ActionResult<Unit>> GetUserUnit(string userId, string unitId)
        {
            Unit? unit = _unitsService.SearchUnitByPlayer(userId, unitId);

            if (unit is null)
                return NotFound();

            if (unit.Type != "scout" && unit.Type != "builder" && unit.Type != "cargo" && unit.Health <= 0)
            {
                _unitsService.DeleteUnit(unit);
                return NotFound();
            }
            
            string estimatedArrival = unit.EstimatedTimeOfArrival;

            if (estimatedArrival is null) return unit;

            TimeSpan interval = DateTime.Parse(estimatedArrival) - _clock.Now;
            
            if (interval <= TimeSpan.Zero)
            {
                unit.System = unit.GetUnitLocation().System;
                unit.Planet = unit.GetUnitLocation().Planet;
                unit.DestinationPlanet = null;
                unit.DestinationSystem = null;
                
                return unit;
            }

            if (interval < TimeSpan.FromSeconds(17) 
                && unit.DestinationSystem is not null && unit.DestinationPlanet is not null)
            {
                unit.System = unit.GetUnitLocation().System;
                unit.DestinationSystem = null;
            }
                
            if (interval > TimeSpan.FromSeconds(2))
            {
                return unit;
            }

            await _clock.Delay(interval);

            unit.Planet = unit.GetUnitLocation().Planet;
            unit.DestinationPlanet = null;

            return unit;
        }
        
        
        [HttpGet("[controller]/{unitId}/location")]
        [SwaggerOperation(Summary =
            "Returns more detailed information about the location a unit of user currently is about")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(UnitLocation), 200)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.NotFound)]
        public ActionResult<UnitLocation> GetUserUnitLocation(string userId, string unitId)
        {
            UnitLocation? unitLocation = _unitsService.SearchUnitLocationByPlayer(userId, unitId);

            if (unitLocation is null)
                return NotFound();

            return unitLocation;
        }
        
        [HttpPut("[controller]/{unitId}")]
        [SwaggerOperation(Summary =
            "Change the status of a unit of a user. Right now, only its position (system and planet) can be changed - which is akin to moving it.")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Unit), 200)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.NotFound)]
        #nullable enable
        public ActionResult<Unit> Put(string userId, string unitId, [FromBody] UnitBlueprint body)
        {
            UnitLocation unitLocation = _unitsService.CreateUnitLocation(false);
            StarSystem? starSystem = null;
            Planet? planet = null;
            User? user = _usersService.SearchUser(userId);
            
            if (user is null)
                return NotFound();

            Unit? fetchedUnit = _unitsService.SearchUnitByPlayer(userId, unitId);
            
            if (fetchedUnit is not null)
            {
                if (fetchedUnit.Type != "cargo" || _unitsService.CompareResources(fetchedUnit, body))
                {
                    if (fetchedUnit.Type != "cargo" && body.ResourcesQuantity is not null)
                        return BadRequest();
                    
                    starSystem = _sectorService.SearchSystem(body.System);
                    planet = starSystem.SearchPlanet(body.Planet);

                    /*if (starSystem is not null)
                    {
                        unitLocation = _unitsService.CreateUnitLocation(starSystem, planet);
                    }*/

                    if (body.DestinationSystem is not null)
                        starSystem = _sectorService.SearchSystem(body.DestinationSystem);

                    if (starSystem is null)
                        return BadRequest();

                    fetchedUnit.DestinationSystem = starSystem.Name;

                    if (body.DestinationPlanet is not null)
                    {
                        if (starSystem is null)
                            return BadRequest();

                        planet = starSystem.SearchPlanet(body.DestinationPlanet);

                        fetchedUnit.DestinationPlanet = planet.Name;
                    }

                    if (fetchedUnit.DestinationPlanet != fetchedUnit.Planet ||
                        fetchedUnit.DestinationSystem != fetchedUnit.System)
                    {
                        _unitsService.MoveUnit(user, fetchedUnit, starSystem, planet);
                        _buildingService.CancelUnbuiltBuildingsOfUnit(user.Id, fetchedUnit.Id);
                    }
                }
                else
                {
                    List<Building> sameLocationBuildings = new List<Building>();
                    sameLocationBuildings = _buildingService.SearchBuildingsByPlayer(user.Id);
                    sameLocationBuildings = (from building in sameLocationBuildings
                        where building.IsBuilt == true && building.Type == "starport" &&
                              building.System == fetchedUnit.System && building.Planet == fetchedUnit.Planet
                        select building).ToList();

                    if (sameLocationBuildings.Count > 0)
                    {
                        foreach (KeyValuePair<string, int> resource in body.ResourcesQuantity)
                        {
                            if (ResourceKind.TryParse(resource.Key, true, out ResourceKind parsedResource))
                            {
                                if (!fetchedUnit.ResourcesQuantity.ContainsKey(parsedResource.ToString().ToLower()))
                                {
                                    if (_usersService.DecreaseResource(user, parsedResource, resource.Value))
                                        _unitsService.IncreaseResource(fetchedUnit, parsedResource, resource.Value);
                                    else return BadRequest();
                                }
                                else
                                {
                                    int amount = 0;
                                    if (fetchedUnit.ResourcesQuantity[parsedResource.ToString().ToLower()] > 0)
                                    {
                                        amount = fetchedUnit.ResourcesQuantity[parsedResource.ToString().ToLower()] -
                                                 resource.Value;
                                        if (_unitsService.DecreaseResource(fetchedUnit, parsedResource, amount))
                                            _usersService.IncreaseResource(user, parsedResource, amount);
                                        else return BadRequest();
                                    }
                                    else
                                    {
                                        if (_usersService.DecreaseResource(user, parsedResource, resource.Value))
                                            _unitsService.IncreaseResource(fetchedUnit, parsedResource, resource.Value);
                                        else return BadRequest();
                                    }

                                }
                            }
                        }
                    }
                    else return BadRequest();
                }
            }

            if (fetchedUnit is null)
            {
                if (HttpContext.User.IsInRole("admin"))
                {
                    starSystem = _sectorService.SearchSystem(body.System);

                    planet = starSystem.SearchPlanet(body.Planet);

                    string createdId = Guid.NewGuid().ToString();

                    unitLocation = _unitsService.CreateUnitLocation(starSystem, planet);
                    fetchedUnit = _unitsService.CreateUnit(createdId, body.Type, user, unitLocation);

                    if (starSystem is null)
                        return BadRequest();

                    fetchedUnit.DestinationSystem = starSystem.Name;

                    if (planet is not null)
                        fetchedUnit.DestinationPlanet = planet.Name;

                    _unitsService.MoveUnit(user, fetchedUnit, starSystem, planet);
                }
                else if (HttpContext.User.IsInRole("shard"))
                {
                    string createdId = body.Id;
                    starSystem = _sectorService.SearchSystem("80ad7191-ef3c-14f0-7be8-e875dad4cfa6");
                    
                    unitLocation = _unitsService.CreateUnitLocation(starSystem, planet);
                    fetchedUnit = _unitsService.CreateUnit(createdId, body.Type, user, unitLocation);
                    if (body.Health is not null)
                        fetchedUnit.Health = body.Health.Value;
                    fetchedUnit.ResourcesQuantity = body.ResourcesQuantity;
                }
                else 
                    return Unauthorized();
            }

            return fetchedUnit;
        }
    }
}