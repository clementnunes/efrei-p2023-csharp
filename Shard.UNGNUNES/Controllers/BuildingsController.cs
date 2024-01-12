using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shard.Shared.Core;
using Shard.UNGNUNES.Blueprints;
using Shard.UNGNUNES.Entities;
using Shard.UNGNUNES.Entities.Buildings;
using Shard.UNGNUNES.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Shard.UNGNUNES.Controllers
{
    [Route("users/{userId}")]
    [ApiController]
    public class BuildingsController : ControllerBase
    {
        private readonly BuildingService _buildingService;
        private readonly UsersService _usersService;
        private readonly SectorService _sectorService;
        private readonly UnitsService _unitsService;
        private readonly IClock _clock;

        public BuildingsController(BuildingService buildingService, UsersService usersService,
            UnitsService unitsService,
            SectorService sectorService, IClock clock)
        {
            _buildingService = buildingService;
            _usersService = usersService;
            _sectorService = sectorService;
            _unitsService = unitsService;
            _clock = clock;
        }

        [HttpPost("[controller]")]
        [SwaggerOperation(Summary = "Creates a building at a location")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Building), 201)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.NotFound)]
        #nullable enable
        public ActionResult<Building> PostBuilding(string userId, [FromBody] BuildingBlueprint body)
        {
            Building? building = null;

            try
            {
                building = _buildingService.CreateBuilding(userId, body.BuilderId, body.ResourceCategory,
                    "building_" + userId, body.Type);
            }
            catch (Exception e)
            {
                if (e is KeyNotFoundException)
                    return NotFound();

                if (e is ArgumentNullException)
                    return BadRequest();

                Console.WriteLine(e);
                throw;
            }

            if (building is null)
                return BadRequest();

            _buildingService.StartExtracting(building, userId);

            return building;
        }

        [HttpPost("[controller]/{starportId}/queue")]
        [SwaggerOperation(Summary =
            "Add a unit to the build queue of the starport. Currently immediately returns the unit")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Unit), 200)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.NotFound)]
        #nullable enable
        public ActionResult<Unit> PostBuildingQueue(string userId, string starportId, [FromBody] BuildingBlueprint body)
        {
            Building? building = null;
            StarPort? starPort = null;
            var user = _usersService.SearchUser(userId);
            UnitLocation? unitLocation = null;
            
            building = _buildingService.SearchBuildingByUserAndBuildingId(userId, starportId);

            if (user is null || building is null)
                return NotFound();

            if (building.Type != "starport")
                return BadRequest();

            if (building.IsBuilt is false)
            {
                if (DateTime.Parse(building.EstimatedBuildTime) - _clock.Now < TimeSpan.FromSeconds(2))
                {
                    building.IsBuilt = true;
                    building.EstimatedBuildTime = null;
                }
                else
                {
                    return BadRequest();
                }
            }

            if (body.Type is null)
                return BadRequest();
            
            string serializedBuilding = JsonConvert.SerializeObject(building);
            starPort = JsonConvert.DeserializeObject<StarPort>(serializedBuilding);

            StarSystem starSystem = _sectorService.SearchSystem(building.System);
            Planet planet = starSystem.SearchPlanet(building.Planet);

            var enoughResources = _buildingService.ConsumeResources(body.Type, user);

            if (enoughResources is false)
                return BadRequest();

            unitLocation = _unitsService.CreateUnitLocation(starSystem, planet);

            string unitId = Guid.NewGuid().ToString();
            
            Unit unit = _unitsService.CreateUnit(unitId, body.Type, user, unitLocation);

            starPort.Queue.Add(unit);

            return unit;
        }

        [HttpGet("[controller]")]
        [SwaggerOperation(Summary = "Returns all buildings of a user.")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<Building>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.NotFound)]
        public ActionResult<List<Building>> GetUserBuildings(string userId)
        {
            var buildings = _buildingService.SearchBuildingsByPlayer(userId);
            return buildings is null ? NotFound() : buildings;
        }


        [HttpGet("[controller]/{buildingId}")]
        [SwaggerOperation(Summary = "Return information about one single building of a user.")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Building), 200)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.NotFound)]
#nullable enable
        public async Task<ActionResult<Building>> GetUserBuilding(string userId, string buildingId)
        {
            var building = _buildingService.SearchBuildingByPlayer(userId, buildingId);

            if (building is null)
                return NotFound();

            string estimatedBuild = building.EstimatedBuildTime;

            if (estimatedBuild is null) return building;

            var interval = DateTime.Parse(estimatedBuild) - _clock.Now;

            if (interval <= TimeSpan.Zero)
            {
                building.IsBuilt = true;

                building.EstimatedBuildTime = null;
                return building;
            }

            if (interval > TimeSpan.FromSeconds(2)) return building;

            try
            {
                var tokenSource = new CancellationTokenSource();

                _buildingService.addToken(building, tokenSource);

                await _clock.Delay(interval, tokenSource.Token);
                
                building.IsBuilt = true;

                building.EstimatedBuildTime = null;
                
                return building;
            }
            catch (Exception)
            {
                return NotFound();
            }
            
        }
    }
}