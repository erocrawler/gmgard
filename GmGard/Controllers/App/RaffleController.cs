using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Extensions;
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

        public RaffleController(
            UsersContext udb,
            UserManager<UserProfile> userManager,
            ExpUtil expUtil)
        {
            _udb = udb;
            _expUtil = expUtil;
            _userManager = userManager;
        }

        bool IsActive(RaffleConfig config) => config.EventStart < DateTime.Now && DateTime.Now < config.EventEnd;
        Task<bool> HasRaffleAsync(RaffleConfig config) => _udb.UserRaffles.AnyAsync(r => r.User.UserName == User.Identity.Name && r.Config.Id == config.Id);

        // GET: api/<controller>
        [HttpGet]
        public async Task<ActionResult> GetAsync(int id)
        {
            var config = await _udb.RaffleConfigs.FindAsync(id);
            if (config == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(User);
            return Json(new {
                config.EventStart,
                config.EventEnd,
                config.Title,
                config.Image,
                config.RaffleCost,
                IsActive = IsActive(config),
                HasRaffle = await HasRaffleAsync(config),
                user.Points,
            });
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<ActionResult> PostAsync(int id)
        {
            var config = await _udb.RaffleConfigs.FindAsync(id);
            if (config == null)
            {
                return NotFound();
            }
            if (!IsActive(config))
            {
                return BadRequest();
            }
            if (await HasRaffleAsync(config))
            {
                return Conflict();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user.Points < config.RaffleCost)
            {
                return BadRequest();
            }
            _expUtil.AddPoint(user, -config.RaffleCost);
            var r = new UserRaffle
            {
                User = user,
                RaffleID = Guid.NewGuid(),
                TimeStamp = DateTime.Now,
                Config = config,
            };
            _udb.UserRaffles.Add(r);
            await _udb.SaveChangesAsync();
            return Ok();
        }

        [HttpPut, Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> PutAsync(RaffleConfig config)
        {
            _udb.RaffleConfigs.Add(config);
            await _udb.SaveChangesAsync();
            return Ok();
        }


        [HttpPatch, Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> PatchAsync(RaffleConfig config) {
            var existing = await _udb.RaffleConfigs.FindAsync(config.Id);
            if (existing == null)
            {
                return NotFound();
            }
            existing.EventEnd = config.EventEnd;
            existing.EventStart = config.EventStart;
            existing.Image = config.Image;
            existing.RaffleCost = config.RaffleCost;
            existing.Title = config.Title;
            await _udb.SaveChangesAsync();
            return Ok();
        }


        [HttpGet, Route("All"), Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> AllAsync(int page = 1)
        {
            var all = await _udb.RaffleConfigs.OrderByDescending(r => r.Id).ToPagedListAsync(page, 100);
            return Json(new Paged<RaffleConfig>(all));
        }


        [HttpGet, Route("Draft"), Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> DraftAsync(int id, int amount = 10)
        {
            var results = await _udb.UserRaffles
                .Include(u => u.User)
                .Where(u => u.Config.Id == id)
                .OrderBy(_ => Guid.NewGuid())
                .Take(amount)
                .ToListAsync();
            return Json(new
            {
                result = results.Select(ur => new
                {
                    user = Models.App.User.FromUserProfile(ur.User),
                    code = ur.RaffleID.ToString(),
                    date = ur.TimeStamp,
                }),
                total = await _udb.UserRaffles.CountAsync(u => u.Config.Id == id),
            });
        }
    }
}
