using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Shard.UNGNUNES.Entities;
using Shard.UNGNUNES.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Shard.UNGNUNES.Controllers
{
    [Route("[controller]/")]
    [ApiController]
    public class SystemsController : ControllerBase
    {
        private readonly SectorService _sectorService;

        public SystemsController(SectorService sectorService)
        {
            _sectorService = sectorService;
        }

        [SwaggerOperation(Summary = "Fetches all systems and their planets")]
        [Produces("application/json")]
        [HttpGet]
        [ProducesResponseType(typeof(List<StarSystem>), 200)]
        public ActionResult<List<StarSystem>> GetSystems()
        {
            return _sectorService.SectorSystem.StarSystems;
        }

        [SwaggerOperation(Summary = "Fetches a single system and all its planets")]
        [HttpGet("{systemName}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StarSystem), 200)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.NotFound)]
        public ActionResult<StarSystem> GetSystem(string systemName)
        {
            StarSystem starSystem = _sectorService.SearchSystem(systemName);

            if (starSystem == null)
            {
                return NotFound();
            }

            return _sectorService.SearchSystem(systemName);
        }

        [SwaggerOperation(Summary = "Fetches all planets of a single system")]
        [HttpGet("{systemName}/planets")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<Planet>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.NotFound)]
        public ActionResult<List<Planet>> GetPlanets(string systemName)
        {
            StarSystem starSystem = _sectorService.SearchSystem(systemName);

            if (starSystem == null)
            {
                return NotFound();
            }

            return starSystem.Planets;
        }

        [SwaggerOperation(Summary = "Fetches a single planet of a system")]
        [HttpGet("{systemName}/planets/{planetName}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Planet), 200)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.NotFound)]
        public ActionResult<Planet> GetPlanet(string systemName, string planetName)
        {
            StarSystem starSystem = _sectorService.SearchSystem(systemName);

            if (starSystem == null)
            {
                return NotFound();
            }

            Planet planet = starSystem.SearchPlanet(planetName);

            if (planet == null)
            {
                return NotFound();
            }

            return planet;
        }
    }
}