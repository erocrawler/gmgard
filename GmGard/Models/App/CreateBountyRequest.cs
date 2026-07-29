using System.ComponentModel.DataAnnotations;

namespace GmGard.Models.App
{
    public class CreateBountyRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required, MinLength(5)]
        public string Content { get; set; }

        public string ImageUrls { get; set; }

        [Range(40, 100000, ErrorMessage = "最佳答案奖励至少40棒棒糖")]
        public int Prize { get; set; } = 40;

        [Range(0, 100000)]
        public int HelpfulReward { get; set; } = 20;
    }

    public class CreateAnswerRequest
    {
        [Required]
        public int BountyId { get; set; }

        [Required, MinLength(2, ErrorMessage = "回答内容太短")]
        public string Content { get; set; }

        public string ImageUrl { get; set; }
    }

    public class AcceptRequest
    {
        [Required]
        public int BountyId { get; set; }

        [Required]
        public int BestAnswerId { get; set; }

        public int[] HelpfulAnswerIds { get; set; }
    }

    public class ReplyAnswerRequest
    {
        [Required]
        public int AnswerId { get; set; }
        [Required, MinLength(1)]
        public string Content { get; set; }
    }

    public class ReplyPostRequest
    {
        [Required]
        public int PostId { get; set; }
        [Required, MinLength(1)]
        public string Content { get; set; }
    }

    public class BountyCommentRequest
    {
        [Required]
        public int BountyId { get; set; }
        [Required, MinLength(1)]
        public string Content { get; set; }
    }
}
