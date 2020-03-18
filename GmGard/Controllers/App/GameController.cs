using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/Game/[action]")]
    [EnableCors("GmAppOrigin")]
    [Authorize]
    [ApiController]
    public class GameController : AppControllerBase
    {
        private UsersContext _udb;
        private ExpUtil _expUtil;

        public GameController(UsersContext udb, ExpUtil expUtil)
        {
            _udb = udb;
            _expUtil = expUtil;
        }

        [HttpGet]
        public async Task<JsonResult> EternalCircle()
        {
            var user = await _udb.Users.Include(u => u.quest).SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user.quest == null)
            {
                user.quest = new UserQuest();
                await _udb.SaveChangesAsync();
            }
            return Json(new
            {
                progress = user.quest.EternalCircleProgress,
                retryCount = user.quest.EternalCircleRetryCount,
            });
        }

        [HttpPost]
        public async Task<ActionResult> EternalCircle(int progress)
        {
            if (!Enum.IsDefined(typeof(UserQuest.ECGameProgress), progress))
            {
                return NotFound();
            }
            UserQuest.ECGameProgress p = (UserQuest.ECGameProgress)progress;
            var user = await _udb.Users.Include(u => u.quest).SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user.quest == null)
            {
                user.quest = new UserQuest();
            }
            if (new[]{
                UserQuest.ECGameProgress.BE1,
                UserQuest.ECGameProgress.BE2,
                UserQuest.ECGameProgress.BE3,
                UserQuest.ECGameProgress.BE4,
                UserQuest.ECGameProgress.BE5,
                UserQuest.ECGameProgress.BE6,
                UserQuest.ECGameProgress.BE7}.Contains(p))
            {
                user.quest.EternalCircleRetryCount++;
            }
            if (p == UserQuest.ECGameProgress.GE && DateTime.Now <= new DateTime(2020, 2, 28, 23, 59, 59))
            {
                user.quest.AddTitle(UserQuest.UserProfession.轮回独断);
            }
            user.quest.EternalCircleProgress = p;
            await _udb.SaveChangesAsync();
            return Ok();
        }
    }
}