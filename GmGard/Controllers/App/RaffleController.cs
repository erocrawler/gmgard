using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        private static readonly DateTime RaffleStart = new DateTime(2020, 3, 7);
        private static readonly DateTime RaffleEnd = new DateTime(2020, 3, 18, 23, 59, 59);
        private const int RaffleCost = 200;

        private readonly ExpUtil _expUtil;
        private readonly UsersContext _udb;
        private readonly UserManager<UserProfile> _userManager;

        public RaffleController(
            UsersContext udb,
            UserManager<UserProfile> userManager,
            ExpUtil expUtil)
        {
            _udb = udb;
            _expUtil = expUtil;
            _userManager = userManager;
        }

        bool IsActive => RaffleStart < DateTime.Now && DateTime.Now < RaffleEnd;
        bool HasRaffle => _udb.UserRaffles.Any(r => r.User.UserName == User.Identity.Name);

        // GET: api/<controller>
        [HttpGet]
        public async Task<JsonResult> GetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return Json(new { IsActive, HasRaffle, user.Points, Cost = RaffleCost });
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
            if (user.Points < RaffleCost)
            {
                return BadRequest();
            }
            _expUtil.AddPoint(user, -RaffleCost);
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
