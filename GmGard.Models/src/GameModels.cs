using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GmGard.Models
{

    public class Game
    {
        [Key]
        public int GameID { get; set; }
        public string GameName { get; set; }
        public string GameChapters { get; set; }
        public string ItemList { get; set; }
    }
    public class GameScenario
    {
        [Key, Column(Order = 0)]
        public int ScenarioID { get; set; }
        [ForeignKey("Game")]
        public int GameID { get; set; }
        public string Dialogs { get; set; }
        public string Narrators { get; set; }
        public string Data { get; set; }

        public virtual Game Game { get; set; }

        public virtual ICollection<ScenarioChoice> Choices { get; set; }
    }

    public class ScenarioChoice
    {
        [Key, ForeignKey("Scenario"), Column(Order = 0)]
        public int ScenarioID { get; set; }
        [Key, ForeignKey("NextScenario"), Column(Order = 1)]
        public int NextScenarioID { get; set; }
        public string Title { get; set; }
        public string ChoiceData { get; set; }

        [InverseProperty("Choices")]
        public virtual GameScenario Scenario { get; set; }
        public virtual GameScenario NextScenario { get; set; }
    }

    public class UserGameData
    {
        [Key, ForeignKey("User"), Column(Order = 0)]
        public int UserID { get; set; }
        public virtual UserProfile User { get; set; }

        [Key, Column(Order = 1), ForeignKey("Game")]
        public int GameID { get; set; }
        public virtual Game Game { get; set; }

        [ForeignKey("CurrentScenario")]
        public int CurrentScenarioID { get; set; }
        public virtual GameScenario CurrentScenario { get; set; }
        public int RetryCount { get; set; }
        public ICollection<UserVisitedScenario> VisitedScenarios { get; set; }
        public string Inventory { get; set; }

    }

    public class UserVisitedScenario
    {
        [Key, ForeignKey("UserGameData"), Column(Order = 0)]
        public int UserID { get; set; }

        [Key, ForeignKey("UserGameData"), Column(Order = 1)]
        public int GameID { get; set; }
        public virtual UserGameData UserGameData { get; set; }

        [Key, Column(Order = 2), ForeignKey("Scenario")]
        public int ScenarioID { get; set; }
        [Key, Column(Order = 3)]
        public int Attempt { get; set; }
        public virtual GameScenario Scenario { get; set; }

        public DateTimeOffset VisitDate { get; set; }
    }
}