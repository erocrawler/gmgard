using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class TreasureHuntPuzzle
    {
        public int Position { get; set; }
        public string Image { get; set; }
        public string Hint { get; set; }
        public string Answer { get; set; }
        public Dictionary<DateTime, string> Attempts { get; set; }
        public bool Completed { get; set; }
    }

    public class TreasureHuntStatus
    {
        public class Player
        {
            public string Avatar { get; set; }
            public int UserId { get; set; }
            public string UserName { get; set; }
            public string NickName { get; set; }
            public DateTime? CompletionTime { get; set; }
            public int Reward { get; set; }
        }
        public IEnumerable<Player> TopPlayers { get; set; }
        public int CompletedUserCount { get; set; }
        public Player CurrentPlayer { get; set; }
        public IEnumerable<TreasureHuntPuzzle> Puzzles { get; set; }
        public int DailyAttemptLimit { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class TreasureHuntAttemptRequest
    {
        public int Id { get; set; }
        public string Answer { get; set; }
    }

    public class TreasureHuntAttemptResult
    {
        public bool IsCorrect { get; set; }
        public int DailyAttemptCount { get; set; }
        public int Reward { get; set; }
        public int Rank { get; set; }
        public int? FinalRank { get; set; }
    }
}
