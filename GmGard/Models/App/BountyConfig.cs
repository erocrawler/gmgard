using System;

namespace GmGard.Models.App
{
    public class BountyConfig
    {
        // Minimum best answer reward (最佳答案奖励)
        public int MinPrize { get; set; } = 40;

        // Minimum helpful reward pool (热心奖励最低)
        public int MinHelpfulReward { get; set; } = 20;

        // Minimum total cost (Best *2 + Helpful) to prevent spam
        public int MinTotal { get; set; } = 100;

        // Days until bounty expires if not closed
        public int ExpireDays { get; set; } = 30;

        // Hours after which best answer author can report if OP doesn't close
        public int ReportAfterHours { get; set; } = 48;

        // Maximum multiplier for report reward (3x)
        public int ReportRewardMultiplier { get; set; } = 3;

        // Title max length
        public int MaxTitleLength { get; set; } = 120;

        // Content min/max
        public int MinContentLength { get; set; } = 5;
        public int MaxContentLength { get; set; } = 10000;

        // List page size
        public int PageSize { get; set; } = 20;

        // Whether helpful reward can be 0 (disable helpful pool)
        public bool AllowZeroHelpfulReward { get; set; } = true;

        // Auto-accept policy on expiration: if OP didn't act, system picks best
        public bool AutoAcceptOnExpire { get; set; } = true;

        // How to pick best when auto-accepting: "Earliest", "MostDiscussion"
        public string AutoPickStrategy { get; set; } = "Earliest";
    }
}
