using GmGard.Models;
using GmGard.Models.App;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        private ILogger _logger;

        public GameController(UsersContext udb, ExpUtil expUtil, ILoggerFactory loggerFactory)
        {
            _udb = udb;
            _expUtil = expUtil;
            _logger = loggerFactory.CreateLogger(nameof(GameController));
        }

        [HttpGet]
        public async Task<ActionResult> Start(int id, bool restart = false)
        {
            var gameStart = await _udb.GameScenarios
                        .Where(g => g.GameID == id)
                        .OrderBy(g => g.ScenarioID)
                        .FirstOrDefaultAsync();
            if (gameStart == null)
            {
                return NotFound();
            }
            var user = await _udb.Users.SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var currentScene = gameStart;
            var gamedata = await _udb.UserGameDatas.FindAsync(user.Id, id);
            if (gamedata == null)
            {
                gamedata = new UserGameData
                {
                    UserID = user.Id,
                    GameID = id,
                    CurrentScenarioID = gameStart.ScenarioID,
                    VisitedScenarios = new List<UserVisitedScenario>()
                    {
                        new UserVisitedScenario
                        {
                            Scenario = gameStart,
                            GameID = id,
                            VisitDate = DateTimeOffset.Now,
                        }
                    }
                };
                _udb.UserGameDatas.Add(gamedata);
                await _udb.SaveChangesAsync();
            }
            else if (restart)
            {
                gamedata.CurrentScenarioID = gameStart.ScenarioID;
                gamedata.RetryCount++;
                await _udb.SaveChangesAsync();
            }
            else
            {
                currentScene = gamedata.CurrentScenario;
            }
            return Json(new GameStatus
            {
                Progress = gamedata.CurrentScenarioID,
                NewGameScenarioId = gameStart.ScenarioID,
                RetryCount = gamedata.RetryCount,
                CurrentScenario = Models.App.GameScenario.Create(currentScene),
            });
        }
        [HttpPost]
        public async Task<ActionResult> Prev(int id)
        {
            var gamedata = await _udb.UserGameDatas
                .Include(u => u.CurrentScenario.Choices)
                .FirstOrDefaultAsync(u => u.User.UserName == User.Identity.Name && u.GameID == id);
            if (gamedata == null || gamedata.CurrentScenario.Choices.Count > 0)
            {
                return NotFound();
            }
            var prev = await _udb.UserVisitedScenarios
                .Where(v => v.Attempt == gamedata.RetryCount &&
                        v.GameID == id &&
                        v.Scenario.Choices.Any(c => c.NextScenarioID == gamedata.CurrentScenarioID))
                .OrderByDescending(v => v.VisitDate)
                .Select(v => v.Scenario)
                .FirstOrDefaultAsync();
            if (prev == null)
            {
                _logger.LogError($"Invalid state for user ${User.Identity.Name}, current scene ${gamedata.CurrentScenarioID}, retry count ${gamedata.RetryCount}");
                return StatusCode(500);
            }
            gamedata.RetryCount++;
            gamedata.CurrentScenario = prev;
            gamedata.VisitedScenarios.Add(new UserVisitedScenario
            {
                Scenario = prev,
                GameID = id,
                Attempt = gamedata.RetryCount,
                VisitDate = DateTimeOffset.Now,
            });
            await _udb.SaveChangesAsync();
            return Json(Models.App.GameScenario.Create(prev));
        }


        [HttpPost]
        public async Task<ActionResult> Next(int progress, int id)
        {
            var gamedata = await _udb.UserGameDatas
                .Include(u => u.CurrentScenario.Choices.Select(c => c.Scenario))
                .FirstOrDefaultAsync(u => u.User.UserName == User.Identity.Name && u.GameID == id);
            if (gamedata == null)
            {
                return NotFound();
            }
            var choice = gamedata.CurrentScenario.Choices.FirstOrDefault(c => c.NextScenarioID == progress);
            if (choice == null)
            {
                return NotFound();
            }
            gamedata.CurrentScenario = choice.NextScenario;
            if (gamedata.VisitedScenarios == null)
            {
                gamedata.VisitedScenarios = new List<UserVisitedScenario>();
            }
            gamedata.VisitedScenarios.Add(new UserVisitedScenario
            {
                Scenario = gamedata.CurrentScenario,
                GameID = id,
                Attempt = gamedata.RetryCount,
                VisitDate = DateTimeOffset.Now,
            });
            await _udb.SaveChangesAsync();
            return Json(Models.App.GameScenario.Create(gamedata.CurrentScenario));
        }
    }
}