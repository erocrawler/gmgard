using GmGard.Models;
using GmGard.Models.App;
using GmGard.Services;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SixLabors.ImageSharp.Formats;
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
        private TitleService _titleService;

        public GameController(UsersContext udb, ExpUtil expUtil, ILoggerFactory loggerFactory, TitleService titleService)
        {
            _udb = udb;
            _expUtil = expUtil;
            _logger = loggerFactory.CreateLogger(nameof(GameController));
            _titleService = titleService;
        }

        private void DeserializeGameData(Game game, out IEnumerable<GameChapter> chapters, out IEnumerable<GameInventory> inventoryData)
        {
            chapters = JsonConvert.DeserializeObject<IEnumerable<GameChapter>>(game.GameChapters ?? "");
            inventoryData = JsonConvert.DeserializeObject<IEnumerable<GameInventory>>(game.ItemList ?? "");
        }

        [HttpGet]
        public async Task<ActionResult> Start(int id, bool restart = false, int jump = 0)
        {
            var gameStart = await _udb.GameScenarios
                        .Include(s => s.Game)
                        .Where(g => g.GameID == id)
                        .OrderBy(g => g.ScenarioID)
                        .FirstOrDefaultAsync();
            if (gameStart == null)
            {
                return NotFound();
            }
            var game = gameStart.Game;
            DeserializeGameData(game, out IEnumerable<GameChapter> chapters, out IEnumerable<GameInventory> inventoryData);
            var userId = await _udb.Users.Where(u => u.UserName == User.Identity.Name).Select(u => u.Id).SingleOrDefaultAsync();
            var currentScene = gameStart;
            var gamedata = await _udb.UserGameDatas
                .Include(v => v.VisitedScenarios)
                .SingleOrDefaultAsync(d => d.GameID == id && d.UserID == userId);
            if (gamedata == null)
            {
                gamedata = new UserGameData
                {
                    UserID = userId,
                    GameID = id,
                    CurrentScenarioID = gameStart.ScenarioID,
                    VisitedScenarios = new List<UserVisitedScenario>()
                    {
                        new UserVisitedScenario
                        {
                            UserID = userId,
                            Scenario = gameStart,
                            GameID = id,
                            VisitDate = DateTimeOffset.Now,
                        }
                    },
                    Inventory = "[]",
                };
                _udb.UserGameDatas.Add(gamedata);
                await _udb.SaveChangesAsync();
            }  
            var visitedChapters = chapters.Where(c => gamedata.VisitedScenarios.Any(v => v.ScenarioID == c.Id));

            if (restart)
            {
                gamedata.CurrentScenarioID = gameStart.ScenarioID;
                gamedata.RetryCount++;
                await _udb.SaveChangesAsync();
            }
            else if (jump > 0)
            {
                var chapter = visitedChapters.FirstOrDefault(c => c.Id == jump);
                if (chapter == null)
                {
                    return NotFound();
                }
                gamedata.CurrentScenarioID = jump;
                gamedata.RetryCount++;
                await _udb.SaveChangesAsync();
                currentScene = await _udb.GameScenarios.FindAsync(jump);
            }
            else
            {
                currentScene = gamedata.CurrentScenario;
            }
            var userInventory = JsonConvert.DeserializeObject<IEnumerable<string>>(gamedata.Inventory) ?? Enumerable.Empty<string>();
            return Json(new GameStatus
            {
                Progress = gamedata.CurrentScenarioID,
                NewGameScenarioId = gameStart.ScenarioID,
                RetryCount = gamedata.RetryCount,
                CurrentScenario = Models.App.GameScenario.Create(currentScene, userInventory),
                Inventory = inventoryData.Where(i => userInventory.Contains(i.Name)),
                Chapters = visitedChapters,
            });
        }
        [HttpPost]
        public async Task<ActionResult> Prev(int id)
        {
            var gamedata = await _udb.UserGameDatas
                .Include(u => u.CurrentScenario.Choices)
                .Include(u => u.VisitedScenarios)
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
            return Json(Models.App.GameScenario.Create(prev, JsonConvert.DeserializeObject<IEnumerable<string>>(gamedata.Inventory)));
        }

        [HttpPost] 
        public async Task<ActionResult> Questionare(int id, [FromBody] IEnumerable<int> answers)
        {
            var gamedata = await _udb.UserGameDatas
                .Include(u => u.CurrentScenario.Choices)
                .Include(u => u.VisitedScenarios)
                .FirstOrDefaultAsync(u => u.User.UserName == User.Identity.Name && u.GameID == id);
            if (gamedata == null)
            {
                return NotFound();
            }
            if (gamedata.CurrentScenario.Choices.Count != 1)
            {
                return NotFound();
            }
            var choice = gamedata.CurrentScenario.Choices.First();
            if (choice.ChoiceData == null)
            {
                return NotFound();
            }
            var data = JsonConvert.DeserializeObject<ChoiceData>(choice.ChoiceData);
            if (data.QuestionResult == null)
            {
                return NotFound();
            }
            int correctCount = 0;
            for (int i = 0; i < data.QuestionResult.Answers.Count(); i++)
            {
                if (data.QuestionResult.Answers.ElementAt(i) == answers.ElementAtOrDefault(i))
                {
                    correctCount++;
                }
            }
            var nextId = data.QuestionResult.Results.First(r => r.Score.Contains(correctCount)).Next;
            var next = await _udb.GameScenarios.FindAsync(nextId);
            gamedata.CurrentScenario = next;
            gamedata.VisitedScenarios.Add(new UserVisitedScenario
            {
                Scenario = gamedata.CurrentScenario,
                GameID = id,
                Attempt = gamedata.RetryCount,
                VisitDate = DateTimeOffset.Now,
            });
            await _udb.SaveChangesAsync();
            return Json(Models.App.GameScenario.Create(gamedata.CurrentScenario, JsonConvert.DeserializeObject<IEnumerable<string>>(gamedata.Inventory)));
        }

        [HttpPost]
        public async Task<ActionResult> Next(int progress, int id)
        {
            var gamedata = await _udb.UserGameDatas
                .Include(u => u.CurrentScenario.Choices.Select(c => c.Scenario))
                .Include(u => u.VisitedScenarios)
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
            var inv = JsonConvert.DeserializeObject<IEnumerable<string>>(gamedata.Inventory);
            var choiceData = choice.ChoiceData == null ? null : JsonConvert.DeserializeObject<ChoiceData>(choice.ChoiceData);
            if (choiceData != null)
            {
                if (choiceData.RequireItems != null && choiceData.RequireItems.Any(i => !inv.Contains(i)))
                { 
                    return BadRequest();
                }
                if (choiceData.GetItems != null)
                {
                    if (inv == null)
                    {
                        inv = choiceData.GetItems;
                    }
                    else
                    {
                        inv = inv.Concat(choiceData.GetItems).Distinct();
                    }
                    gamedata.Inventory = JsonConvert.SerializeObject(inv);
                }
                if (choiceData.GetTitles != null && choiceData.GetTitles.Count() > 0)
                {
                    var userTitle = await _udb.UserQuests.FindAsync(gamedata.UserID);
                    if (userTitle == null)
                    {
                        userTitle = new UserQuest
                        {
                            UserId = gamedata.UserID,
                        };
                        _udb.UserQuests.Add(userTitle);
                    }
                    foreach (var title in choiceData.GetTitles)
                    {
                        int? titleId = _titleService.GetTitleId(title);
                        if (titleId.HasValue)
                        {
                            userTitle.AddTitle(titleId.Value);
                        }
                    }
                }
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
            return Json(Models.App.GameScenario.Create(gamedata.CurrentScenario, inv));
        }
    }
}