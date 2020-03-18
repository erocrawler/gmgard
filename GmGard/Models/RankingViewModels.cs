using System;
using System.Collections.Generic;

namespace GmGard.Models
{
    public class RankingDisplay : HistoryRanking
    {
        public string BlogUrl { get; set; }
        public bool Deleted { get; set; }
    }

    public class MonthRanking
    {
        public List<RankingDisplay> DailyRankings { get; set; }
        public List<RankingDisplay> WeeklyRankings { get; set; }
        public List<RankingDisplay> MonthlyRankings { get; set; }
    }
}