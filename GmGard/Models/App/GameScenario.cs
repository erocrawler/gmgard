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
        public GameScenario CurrentScenario { get; set; }
    }
    public class GameScenario
    {
        public int Id { get; set; }
        public IEnumerable<Narrator> Narrators { get; set; }
        public IEnumerable<Dialog> Dialogs { get; set; }
        public IEnumerable<Choice> Next { get; set; }

        public static GameScenario Create(GmGard.Models.GameScenario s)
        {
            return new GameScenario
            {
                Id = s.ScenarioID,
                Narrators = JsonConvert.DeserializeObject<IEnumerable<Narrator>>(s.Narrators),
                Dialogs = JsonConvert.DeserializeObject<IEnumerable<Dialog>>(s.Dialogs),
                Next = s.Choices.Select(c => new Choice { Text = c.Title, Result = c.NextScenarioID })
            };
        }
    }
    public class Choice
    {
        public string Text { get; set; }
        public int Result { get; set; }
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
