using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class GameStatus
    {
        public int Progress { get; set; }
        public int RetryCount { get; set; }
        public int NewGameScenarioId { get; set; }
        public IEnumerable<GameInventory> Inventory { get; set; }
        public GameScenario CurrentScenario { get; set; }
        public IEnumerable<GameChapter> Chapters { get; set; }
    }
    public class GameChapter
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class GameInventory
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class GameScenario
    {
        public int Id { get; set; }
        public IEnumerable<Narrator> Narrators { get; set; }
        public IEnumerable<Dialog> Dialogs { get; set; }
        public IEnumerable<Choice> Next { get; set; }

        public object Data { get; set; }

        public static GameScenario Create(GmGard.Models.GameScenario s, IEnumerable<string> Inventory)
        {
            return new GameScenario
            {
                Id = s.ScenarioID,
                Narrators = JsonConvert.DeserializeObject<IEnumerable<Narrator>>(s.Narrators),
                Dialogs = JsonConvert.DeserializeObject<IEnumerable<Dialog>>(s.Dialogs),
                Data = JsonConvert.DeserializeObject(s.Data),
                Next = s.Choices.Select(c =>
                {
                    bool locked = false;
                    if (!string.IsNullOrEmpty(c.ChoiceData))
                    {
                        var data = JsonConvert.DeserializeObject<ChoiceData>(c.ChoiceData);
                        if (data.RequireItems != null)
                        {
                            locked = data.RequireItems.Any(i => !Inventory.Contains(i));
                        }
                    }
                    return new Choice
                    {
                        Text = c.Title,
                        Result = c.NextScenarioID,
                        Locked = locked,
                    };
                })
            };
        }
    }
    public class Choice
    {
        public string Text { get; set; }
        public int Result { get; set; }
        public bool Locked { get; set; }
    }
    public class ChoiceData
    {
        public IEnumerable<string> GetItems { get; set; }
        public IEnumerable<string> RequireItems { get; set; }
        public IEnumerable<string> GetTitles { get; set; }
        public QuestionResult QuestionResult { get; set; }
    }
    public class QuestionResult
    {
        public class ScoreResult
        {
            public IEnumerable<int> Score { get; set; }
            public int Next { get; set; }
        }
        public IEnumerable<int> Answers { get; set; }
        public IEnumerable<ScoreResult> Results { get; set; }
    }
    public class Narrator
    {
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string Display { get; set; }
    }
    public class Dialog
    {
        public string BgImg { get; set; }
        public IEnumerable<IEnumerable<string>> Texts { get; set; }
        public IEnumerable<DialogEffect> Effect { get; set; }
    }
    public class DialogEffect
    {
        public int Pos { get; set; }
        public Effects Kind { get; set; }
    }
    public enum Effects
    {
        NONE,
        SPARK,
        SHAKE,
        FLASH,
        TITLE,
        CENTER_BG,
    }
}
