using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Models;
using GmGard.Models.App;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/PunchIn/[action]")]
    [EnableCors("GmAppOrigin")]
    [Authorize]
    [ApiController]
    public class PunchInController : AppControllerBase
    {
        private readonly INickNameProvider _nickNameProvider;
        private readonly ExpUtil _expUtil;
        private readonly UsersContext _udb;
        private readonly AppSettingsModel _appSettings;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _contextAccessor;

        public PunchInController(
            UsersContext udb,
            ExpUtil expUtil,
            IOptions<AppSettingsModel> appSettings,
            IMemoryCache cache,
            IHttpContextAccessor contextAccessor,
            INickNameProvider nickNameProvider)
        {
            _udb = udb;
            _nickNameProvider = nickNameProvider;
            _expUtil = expUtil;
            _appSettings = appSettings.Value;
            _cache = cache;
            _contextAccessor = contextAccessor;
        }

        private int MakeUpCost(DateTime date)
        {
            var diff = DateTime.Today - date.Date;
            if (diff.Days <= 0)
            {
                return 0;
            }
            else if (diff.Days > 0 && diff.Days < 14)
            {
                return 100;
            }
            else
            {
                return 100 + (diff.Days - 7) / 7 * 50;
            }
        }

        [HttpGet]
        public async Task<ActionResult> Cost(DateTime date)
        {
            var cost = await _udb.Users.Where(u => u.UserName == User.Identity.Name).Select(u => new PunchInCost { CurrentPoints = u.Points, Tickets = u.quest == null ? 0 : u.quest.PunchInTicket }).SingleOrDefaultAsync();
            cost.Cost = MakeUpCost(date);
            return Json(cost);
        }

        [HttpPost]
        public async Task<ActionResult> Do([FromBody]PunchInRequest request)
        {
            var user = await _udb.Users.Include(u => u.PunchIns).SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var d = request.Date.GetValueOrDefault(DateTime.Now);
            if (d.Date > DateTime.Today || d.Date < ExpUtil.FirstSignHistoryDate || d.Date < user.CreateDate.Date)
            {
                return BadRequest(new PunchInResult
                {
                    Success = false,
                    ErrorMessage = "日期无效",
                    ConsecutiveDays = user.ConsecutiveSign,
                    Points = user.Points
                });
            }
            if (user.PunchIns == null)
            {
                user.PunchIns = new List<PunchInHistory>();
            }
            else if (user.PunchIns.Any(p => p.TimeStamp.Date == d.Date))
            {
                return BadRequest(new PunchInResult
                {
                    Success = false,
                    ErrorMessage = "此日已签",
                    ConsecutiveDays = user.ConsecutiveSign,
                    Points = user.Points
                });
            }
            var today = DateTime.Now;
            bool isMakeup = d.Date != today.Date;
            int consecutive = 0;
            if (isMakeup)
            {
                if (request.UseTicket)
                {
                    if (user.quest.PunchInTicket > 0)
                    {
                        user.quest.PunchInTicket--;
                    }
                    else
                    {
                        return BadRequest(new PunchInResult
                        {
                            Success = false,
                            ErrorMessage = "补签券不足",
                            ConsecutiveDays = user.ConsecutiveSign,
                            Points = user.Points
                        });
                    }
                }
                else
                {
                    int cost = MakeUpCost(d.Date);
                    if (user.Points < cost)
                    {
                        return BadRequest(new PunchInResult
                        {
                            Success = false,
                            ErrorMessage = "棒棒糖不足",
                            ConsecutiveDays = user.ConsecutiveSign,
                            Points = user.Points
                        });
                    }
                    user.Points -= cost;
                }
            }
            else
            {
                user.LastSignDate = today;
            }
            user.PunchIns.Add(new PunchInHistory { UserID = user.Id, IsMakeup = isMakeup, TimeStamp = isMakeup ? d : today });
            var lastDay = user.PunchIns.Max(u => u.TimeStamp).Date;
            foreach (var t in user.PunchIns.OrderByDescending(h => h.TimeStamp))
            {
                if (t.TimeStamp.Date > lastDay)
                {
                    continue;
                }
                if (t.TimeStamp.Date != lastDay)
                {
                    break;
                }
                consecutive++;
                lastDay = lastDay.AddDays(-1);
            }
            if (lastDay < ExpUtil.FirstSignHistoryDate)
            {
                consecutive += user.HistoryConsecutiveSign;
            }
            user.ConsecutiveSign = consecutive;
            List<int> days = _appSettings.ExpAddOnDay;
            int exp = 0;
            if (!isMakeup && days != null && days.Count > 0)
            {
                if (user.ConsecutiveSign > days.Count)
                {
                    exp = days.Last();
                }
                else
                {
                    exp = days[user.ConsecutiveSign - 1];
                }
                _expUtil.addExp(user, exp);
            }
            user.LastLoginIP = ExpUtil.GetIPAddress(HttpContext);
            await _udb.SaveChangesAsync();
            _cache.Remove(ExpUtil.SignCacheKey + user.UserName.ToLower());
            return Json(new PunchInResult { Success = true, ConsecutiveDays = user.ConsecutiveSign, ExpBonus = exp, Points = user.Points });
        }

        [HttpGet]
        public ActionResult History(int? year, int? month)
        {
            int y = year ?? DateTime.Today.Year;
            int m = month ?? DateTime.Today.Month;
            var minDate = new DateTime(2019, 1, 25);
            var thisMonth = new DateTime(y, m, 1);
            if (thisMonth.Year < 2019 || thisMonth > DateTime.Today)
            {
                return Json(Enumerable.Empty<PunchInHistory>());
            }
            var user = _udb.Users.SingleOrDefault(u => u.UserName == User.Identity.Name);
            var currentMonthData = _udb.PunchInHistories.Where(h => h.User.UserName == User.Identity.Name && DbFunctions.DiffMonths(h.TimeStamp, thisMonth) == 0 && DbFunctions.DiffYears(h.TimeStamp, thisMonth) == 0);
            var response = new PunchInHistoryResponse {
                PunchIns = currentMonthData.Select(u => new PunchInHistoryResponse.PunchIn { TimeStamp = u.TimeStamp, IsMakeUp = u.IsMakeup }).ToList(),
                MinSignDate = user.CreateDate > minDate ? user.CreateDate : minDate,
            };
            if (y == 2019 && m == 1)
            {
                response.LegacySignDays = user.HistoryConsecutiveSign;
            }
            return Json(response);
        }
    }
}