using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shard.Shared.Core;
using Shard.UNGNUNES.Blueprints;
using Shard.UNGNUNES.Services;
using Shard.UNGNUNES.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Shard.UNGNUNES.Controllers
{
    [Route("[controller]/")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly UnitsService _unitsService;

        public UsersController(UsersService usersService, UnitsService unitsService)
        {
            _usersService = usersService;
            _unitsService = unitsService;
        }

        [SwaggerOperation(Summary = "Create a new user")]
        [HttpPut("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        #nullable enable
        public ActionResult<User> Put(string id, [FromBody] UserBlueprint? body)
        {
            string pattern = @"[\.[{(*+?^$|']";
            User? user = _usersService.SearchUser(id);

            if (user is null)
            {
                if (Regex.IsMatch(id, pattern) || body is null)
                    return BadRequest();
                
                if (id != body.Id)
                    return BadRequest();

                User lastUser = _usersService.CreateUser(id, body.Pseudo, body.DateOfCreation);

                if (HttpContext.User.IsInRole("shard"))
                {
                    _usersService.ClearResources(lastUser);
                }
                else
                {
                    UnitLocation unitLocation = _unitsService.CreateUnitLocation(true);

                    string scoutId = Guid.NewGuid().ToString();
                    string builderId = Guid.NewGuid().ToString();

                    _unitsService.CreateUnit(scoutId, "scout", lastUser, unitLocation);
                    _unitsService.CreateUnit(builderId, "builder", lastUser, unitLocation);
                }

                return lastUser;
            }

            if (HttpContext.User.IsInRole("admin"))
            {
                if (body is null)
                    return BadRequest();

                foreach (var (resource, quantity) in body.ResourcesQuantity)
                {
                    _usersService.SetResource(user, resource, quantity);
                }
                
                if (body.Pseudo is not null)
                    user.Pseudo = body.Pseudo;
            }

            return user;
        }

        [SwaggerOperation(Summary = "Returns details of an existing user")]
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.NotFound)]
        public ActionResult<User> Get(string id)
        {
            User searchedUser = _usersService.SearchUser(id);

            if (searchedUser == null)
                return NotFound();

            return searchedUser;
        }
    }
}
