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

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/TreasureHunt/[action]")]
    [EnableCors("GmAppOrigin")]
    [ApiController]
    [Authorize]
    public class TreasureHuntController : AppControllerBase
    {
        private static readonly TreasureHuntPuzzle[] AllPuzzles = new[] 
        {
            new TreasureHuntPuzzle
            {
                Position = 0,
                Image = "/assets/puzzle/0胖次测量.png",
                Hint = "胖次测量 2:13",
                Answer = "前93895",
            },
            new TreasureHuntPuzzle
            {
                Position = 1,
                Image = "/assets/puzzle/1白花发卡.png",
                Hint = "白花发卡 5:6",
                Answer = "有19228",
            },
            new TreasureHuntPuzzle
            {
                Position = 2,
                Image = "/assets/puzzle/2杜若的鞋.png",
                Hint = "杜若的鞋 2:3",
                Answer = "不93096",
            },
            new TreasureHuntPuzzle
            {
                Position = 3,
                Image = "/assets/puzzle/3舔美咲脚.png",
                Hint = "舔美咲脚 1:8",
                Answer = "纯91251",
            },
            new TreasureHuntPuzzle
            {
                Position = 4,
                Image = "/assets/puzzle/4一起睡觉.png",
                Hint = "一起睡觉 1:1",
                Answer = "萝43079",
            },
            new TreasureHuntPuzzle
            {
                Position = 5,
                Image = "/assets/puzzle/6突然舔脚.png",
                Hint = "突然舔脚 1:40",
                Answer = "莉85263",
            },
            new TreasureHuntPuzzle
            {
                Position = 6,
                Image = "/assets/puzzle/5蓝毛法师.png",
                Hint = "蓝毛法师 2:20",
                Answer = "足81975",
            },
            new TreasureHuntPuzzle
            {
                Position = 7,
                Image = "/assets/puzzle/7脚和法杖.png",
                Hint = "脚和法杖 2:13",
                Answer = "交52004",
            },
        };
        private static int PuzzleCount => AllPuzzles.Length;

        private readonly UsersContext _udb;
        private readonly IMemoryCache _cache;
        private readonly INickNameProvider _nickNameProvider;
        private readonly ExpUtil _expUtil;
        private const int DailyAttemptLimit = 5;
        private static readonly object _attemptLock = new object();

        public TreasureHuntController(
            UsersContext udb, IMemoryCache cache, INickNameProvider nickNameProvider, ExpUtil expUtil)
        {
            _udb = udb;
            _cache = cache;
            _nickNameProvider = nickNameProvider;
            _expUtil = expUtil;
        }

        public async Task<ActionResult> Status()
        {
            var userData = await _udb.Users.Where(u => u.UserName == User.Identity.Name)
                .GroupJoin(_udb.TreasureHuntAttempts, u => u.Id, u => u.UserID, (u, t) => new { User = u, Attemps = t })
                .SingleOrDefaultAsync();
            int correctCount = userData.Attemps.Where(a => a.IsCorrect).GroupBy(a => a.TargetPuzzle).Count();
            var response = await GetCommonStatusAsync();
            int ticketCount = userData.Attemps.Where(a => a.IsCorrect && a.AttemptTime <= response.EndTime).GroupBy(a => a.TargetPuzzle).Count();
            response.CurrentPlayer = new TreasureHuntStatus.Player
            {
                Avatar = Url.Action("Show", "Avatar", new { name = userData.User.UserName }, Request.Scheme),
                UserId = userData.User.Id,
                UserName = userData.User.UserName,
                NickName = userData.User.NickName,
                Reward = ticketCount * 100
            };
            if (correctCount >= AllPuzzles.Length)
            {
                response.CurrentPlayer.CompletionTime = userData.Attemps.Where(a => a.IsCorrect).Max(t => t.AttemptTime);
                if (ticketCount == correctCount)
                {
                    int rankReward = response.TopPlayers.Where(u => u.UserId == response.CurrentPlayer.UserId).Select(u => u.Reward).SingleOrDefault();
                    if (rankReward == 0)
                    {
                        rankReward = 500;
                    }
                    response.CurrentPlayer.Reward += rankReward;
                }
            }
            if (DateTime.Now < response.EndTime)
            {
                response.TopPlayers = response.TopPlayers.Select(u => new TreasureHuntStatus.Player
                {
                    Avatar = "//gmgard.com/Images/nazoshinshi.jpg",
                    CompletionTime = u.CompletionTime,
                    Reward = u.Reward
                });
            }
            if (correctCount < 4)
            {
                response.Puzzles = FilteredPuzzles(userData.User.Id);
            }
            foreach (var puzzle in response.Puzzles)
            {
                puzzle.Attempts = userData.Attemps.Where(t => t.TargetPuzzle == puzzle.Position).ToDictionary(a => a.AttemptTime, a => a.AttemptAnswer);
                if (!userData.Attemps.Where(t => t.TargetPuzzle == puzzle.Position).Any(a => a.IsCorrect))
                {
                    puzzle.Answer = null;
                }
                else
                {
                    puzzle.Completed = true;
                }
            }
            return Json(response);
        }

        [HttpPost]
        public async Task<ActionResult> Attempt(TreasureHuntAttemptRequest req)
        {
            if (string.IsNullOrEmpty(req.Answer))
            {
                return BadRequest(new { error = "请输入回答。" });
            }
            var userData = await _udb.Users.Where(u => u.UserName == User.Identity.Name)
                .GroupJoin(_udb.TreasureHuntAttempts, u => u.Id, u => u.UserID, (u, t) => new { User = u, Attemps = t })
                .SingleOrDefaultAsync();
            int correctCount = userData.Attemps.Where(a => a.IsCorrect).GroupBy(a => a.TargetPuzzle).Count();
            int dailyAttemptCount = userData.Attemps.Count(a => !a.IsCorrect && a.AttemptTime.Date == DateTime.Today);
            if (dailyAttemptCount >= DailyAttemptLimit)
            {
                return BadRequest(new { error = "已达每日尝试上限。" });
            }
            if (correctCount < 4)
            {
                var puzzles = FilteredPuzzles(userData.User.Id);
                if (!puzzles.Any(p => p.Position == req.Id))
                {
                    return BadRequest(new { error = "无效的答题序号。" });
                }
            }
            if (req.Id < 0 || req.Id >= AllPuzzles.Length)
            {
                return BadRequest(new { error = "无效的答题序号。" });
            }
            bool correct = AllPuzzles[req.Id].Answer.Equals(req.Answer);
            var attemptTime = DateTime.Now;
            _udb.TreasureHuntAttempts.Add(new TreasureHuntAttempt
            {
                AttemptAnswer = req.Answer,
                AttemptTime = attemptTime,
                IsCorrect = correct,
                TargetPuzzle = req.Id,
                UserID = userData.User.Id,
            });
            int reward = 0;
            int rank = 0;
            int? finalRank = null;
            if (correct)
            {
                lock (_attemptLock)
                {
                    if (_udb.TreasureHuntAttempts.Count(a => a.UserID == userData.User.Id && a.IsCorrect && a.TargetPuzzle == req.Id) != 0)
                    {
                        return BadRequest(new { error = "本题已经回答过了。"});
                    }
                    rank = _udb.TreasureHuntAttempts
                        .Where(u => u.TargetPuzzle == req.Id && u.IsCorrect && !u.User.Roles.Any(r => r.RoleId == 1 || r.RoleId == 3)) // 1 = Admin, 3 = Moderator
                        .GroupBy(u => u.UserID)
                        .Count() + 1;
                    if (correctCount == 7)  // final answer correct.
                    {
                        var completedUserCount = _udb.TreasureHuntAttempts
                            .Where(u => u.IsCorrect && !u.User.Roles.Any(r => r.RoleId == 1 || r.RoleId == 3)) // 1 = Admin, 3 = Moderator
                            .GroupBy(u => u.UserID)
                            .Where(g => g.GroupBy(gg => gg.TargetPuzzle).Count() == PuzzleCount)
                            .Count();
                        finalRank = completedUserCount + 1;
                    }
                }
            }
            await _udb.SaveChangesAsync();
            return Json(new TreasureHuntAttemptResult
            {
                IsCorrect = correct,
                DailyAttemptCount = correct ? dailyAttemptCount : dailyAttemptCount + 1,
                Reward = reward,
                Rank = rank,
                FinalRank = finalRank,
            });
        }

        private TreasureHuntPuzzle[] FilteredPuzzles(int userid)
        {
            Random rnd = new Random(userid);
            return AllPuzzles.OrderBy(_ => rnd.Next()).Take(4).Select(a => new TreasureHuntPuzzle
            {
                Position = a.Position,
                Image = a.Image,
                Hint = a.Hint,
                Answer = a.Answer
            }).ToArray();
        }

        private async Task<TreasureHuntStatus> GetCommonStatusAsync()
        {
            var completedUsers = _udb.TreasureHuntAttempts
                .Where(u => u.IsCorrect && !u.User.Roles.Any(r => r.RoleId == 1 || r.RoleId == 3)) // 1 = Admin, 3 = Moderator
                .GroupBy(u => u.UserID)
                .Where(g => g.GroupBy(gg => gg.TargetPuzzle).Count() == PuzzleCount);
            var completedUserCount = await completedUsers.CountAsync();
            var top10User = _cache.Get<IEnumerable<TreasureHuntStatus.Player>>("~thTop");
            if (top10User == null)
            {
                var users = await completedUsers
                    .Select(g => new { user = g.FirstOrDefault().User, completeTime = g.Max(gg => gg.AttemptTime) })
                    .OrderBy(g => g.completeTime)
                    .Take(10).ToListAsync();
                top10User = users.Select((u, i) => new TreasureHuntStatus.Player
                {
                    Avatar = Url.Action("Show", "Avatar", new { name = u.user.UserName }, Request.Scheme),
                    UserId = u.user.Id,
                    UserName = u.user.UserName,
                    NickName = u.user.NickName,
                    CompletionTime = u.completeTime,
                    Reward = RankReward(i),
                });
                if (users.Count == 3)
                {
                    _cache.Set("~thTop3", users);
                }
            }
            return new TreasureHuntStatus
            {
                Puzzles = AllPuzzles.Select(a => new TreasureHuntPuzzle
                {
                    Position = a.Position,
                    Image = a.Image,
                    Hint = a.Hint,
                    Answer = a.Answer
                }).ToList(),
                DailyAttemptLimit = DailyAttemptLimit,
                TopPlayers = top10User,
                CompletedUserCount = completedUserCount,
                EndTime = new DateTime(2018, 9, 15, 23, 59, 59),
            }; ;
        }

        private int RankReward(int rank)
        {
            switch (rank)
            {
                case 0:
                    return 5000;
                case 1:
                    return 3000;
                case 2:
                    return 1000;
                default:
                    return 500;
            }
        }
    }
}