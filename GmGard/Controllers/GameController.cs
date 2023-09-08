using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GmGard.Models;
using GmGard.Services;
using System.Data.Entity;

namespace GmGard.Controllers
{
    [Route("api/[controller]/[action]"), Authorize, ResponseCache(CacheProfileName = "Never")]
    public class GameController : Controller
    {
        private UserManager<UserProfile> _userManager;
        private UsersContext _udb;
        private ExpUtil _expUtil;
        private IAuthorizationService _authorizationService;

        public GameController(UserManager<UserProfile> userManager, UsersContext udb, ExpUtil expUtil, IAuthorizationService authorizationService)
        {
            _userManager = userManager;
            _udb = udb;
            _expUtil = expUtil;
            _authorizationService = authorizationService;
        }
        
        [HttpGet]
        public async Task<JsonResult> Data()
        {
            var user = await _udb.Users.Include(u => u.quest).SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user.quest == null)
            {
                user.quest = new UserQuest();
                await _udb.SaveChangesAsync();
            }
            var desc = new GameChoiceDescriptor(user.quest);
            return Json(new {
                nickname = user.NickName,
                progress = user.quest.Progress,
                profession = UserQuest.ProfessionName(user.quest.Profession),
                stats = user.quest.Progress == UserQuest.GameProgress.Act1AfterChoose ? await GetProfessionStatsAsync() : null,
                isdead = user.quest.IsDead,
                deathcount = user.quest.DeathCount,
                act2choice = desc.GetAct2Choice(),
                act3choice = desc.GetAct3Choice(),
                act4choice = desc.GetAct4Choice(),
                act5choice = desc.GetAct5Choice(),
                act5bchoice = desc.GetAct5bChoice(),
                act6choice = desc.GetAct6Choice() == "true",
                question = desc.GetCurrentQuestion(),
            });
        }

        [HttpPut]
        public async Task<JsonResult> Reset()
        {
            var user = await _udb.Users.Include(u => u.quest).SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user.quest == null)
            {
                user.quest = new UserQuest();
            }
            user.quest.Progress = 0;
            user.quest.Profession = UserQuest.UserProfession.None;
            user.quest.IsDead = false;
            user.quest.DeathCount = 0;
            await _udb.SaveChangesAsync();
            return Json(new
            {
                nickname = user.NickName,
                progress = user.quest.Progress,
                profession = "",
                isdead = user.quest.IsDead,
                deathcount = user.quest.DeathCount,
            });
        }

        [HttpPut]
        public async Task<ActionResult> Act(string actname, string choice)
        {
            var quest = await GetQuestAsync(true);
            GameFlow flow;

            switch (actname)
            {
                case "Act1BeforeChoose":
                    flow = new Act1BeforeChoose(quest);
                    break;
                case "Act1Choose":
                    flow = new Act1Choose(quest, choice, await GetProfessionStatsAsync());
                    break;
                case "Act2Start":
                    flow = new Act2Start(quest);
                    break;
                case "Act2Choose":
                    flow = new Act2Choose(quest, choice);
                    break;
                case "Act3Start":
                    flow = new Act3Start(quest);
                    break;
                case "Act3Choose":
                    flow = new Act3Choose(quest, choice);
                    break;
                case "Act4Start":
                    flow = new Act4Start(quest);
                    break;
                case "Act4Choose":
                    flow = new Act4Choose(quest, choice);
                    break;
                case "Act5Start":
                    flow = new Act5Start(quest);
                    break;
                case "Act5Extra":
                    flow = new Act5Extra(quest, choice);
                    break;
                case "Act5BeforeChoose":
                    flow = new Act5BeforeChoose(quest);
                    break;
                case "Act5Choose":
                    flow = new Act5Choose(quest, choice);
                    break;
                case "Act5bStart":
                    flow = new Act5bStart(quest);
                    break;
                case "Act5bChoose":
                    flow = new Act5bChoose(quest, choice);
                    break;
                case "Act6Start":
                    flow = new Act6Start(quest);
                    break;
                case "Act6Choose":
                    flow = new Act6Choose(quest, choice);
                    break;
                case "Act6Q1":
                    flow = new Act6Q1(quest);
                    break;
                case "Act6Q2":
                    flow = new Act6Q2(quest, choice);
                    break;
                case "Act6Q3":
                    flow = new Act6Q3(quest, choice);
                    break;
                case "Act6AfterQ":
                    flow = new Act6AfterQ(quest, choice);
                    break;
                case "Act6Leave":
                    flow = new Act6Leave(quest);
                    break;
                case "Act6Stay":
                    flow = new Act6Stay(quest, choice);
                    break;
                default:
                    return BadRequest();
            }
            if (!flow.CheckPrecondition() || !flow.ProcessFlow())
            {
                return BadRequest();
            }
            await _udb.SaveChangesAsync();
            return Json(flow.JsonResponse());
        }

        [HttpPost]
        public async Task<ActionResult> Revive(string choice)
        {
            var quest = await GetQuestAsync(true);
            if (!quest.IsDead)
            {
                return BadRequest();
            }
            var profession = UserQuest.GetProfession(choice);
            if (profession == UserQuest.UserProfession.None || profession == UserQuest.UserProfession.Shiro)
            {
                return BadRequest();
            }
            quest.Profession = profession;
            quest.IsDead = false;
            quest.AddTitle((int)quest.Profession);
            await _udb.SaveChangesAsync();
            return Json(new { success = true });
        }

        private Task<UserQuest> GetQuestAsync(bool includeUser = false) =>
            (includeUser ? (_udb.UserQuests.Include(u => u.user)) : _udb.UserQuests)
                .SingleOrDefaultAsync(u => u.user.UserName == User.Identity.Name);

        private async Task<Dictionary<string, int>> GetProfessionStatsAsync()
        {
            var dict = await _udb.UserQuests.Where(u => u.Profession != UserQuest.UserProfession.None)
                .GroupBy(u => u.Profession)
                .Select(g => new { g.Key, Count = g.Count() }).ToListAsync();
            return dict.ToDictionary(a => a.Key.ToString().ToLower(), a => a.Count);
        }
    }
}
