using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Models;
using GmGard.Models.App;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Authorize]
    [EnableCors("GmAppOrigin")]
    [Route("api/[controller]")]
    [ApiController]
    public class RaffleController : AppControllerBase
    {

        private readonly ExpUtil _expUtil;
        private readonly UsersContext _udb;
        private readonly UserManager<UserProfile> _userManager;
        private readonly IOptionsSnapshot<RaffleConfig> _config;

        public RaffleController(
            IOptionsSnapshot<RaffleConfig> config,
            UsersContext udb,
            UserManager<UserProfile> userManager,
            ExpUtil expUtil)
        {
            _udb = udb;
            _expUtil = expUtil;
            _userManager = userManager;
            _config = config;
        }

        bool IsActive => _config.Value != null && _config.Value.EventStart < DateTime.Now && DateTime.Now < _config.Value.EventEnd;
        bool HasRaffle => _udb.UserRaffles.Any(r => r.User.UserName == User.Identity.Name && _config.Value.EventStart <= r.TimeStamp && r.TimeStamp <= _config.Value.EventEnd);

        // GET: api/<controller>
        [HttpGet]
        public async Task<JsonResult> GetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return Json(new { 
                StartTime = _config.Value.EventStart,
                EndTime = _config.Value.EventEnd,
                _config.Value.Title,
                _config.Value.Image,
                IsActive,
                HasRaffle, 
                user.Points, 
                Cost = _config.Value.RaffleCost });
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<ActionResult> PostAsync()
        {
            if (!IsActive)
            {
                return BadRequest();
            }
            if (HasRaffle)
            {
                return Conflict();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user.Points < _config.Value.RaffleCost)
            {
                return BadRequest();
            }
            _expUtil.AddPoint(user, -_config.Value.RaffleCost);
            var r = new UserRaffle
            {
                User = user,
                RaffleID = Guid.NewGuid(),
                TimeStamp = DateTime.Now,
            };
            _udb.UserRaffles.Add(r);
            await _udb.SaveChangesAsync();
            return Ok();
        }
    }
}
