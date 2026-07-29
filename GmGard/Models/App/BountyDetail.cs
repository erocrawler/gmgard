using System;
using System.Collections.Generic;

namespace GmGard.Models.App
{
    public class BountyDetail
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string[] ImageUrls { get; set; }
        public string Author { get; set; }
        public string AuthorAvatar { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? CloseDate { get; set; }
        public int Prize { get; set; }
        public int Deposit { get; set; }
        public int HelpfulReward { get; set; }
        public bool IsAccepted { get; set; }
        public int ViewCount { get; set; }
        public int? BestAnswerId { get; set; }
        public int AnswerCount { get; set; }
        public bool CanAccept { get; set; }
        public bool CanAnswer { get; set; }
        public List<AnswerDto> Answers { get; set; }
        // Bounty-level comments: Post IdType=Bounty, ItemId=BountyId – allowed even after closed/expired
        public List<PostDto> BountyComments { get; set; } = new();
    }

    public class AnswerDto
    {
        public int AnswerId { get; set; }
        public int BountyId { get; set; }
        public string Author { get; set; }
        public string AuthorAvatar { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsBest { get; set; }
        public bool IsHelpful { get; set; }
        // Replies via Post system (ItemType=Answer)
        public List<PostDto> Replies { get; set; } = new();
    }

    public class PostDto
    {
        public int PostId { get; set; }
        public string Author { get; set; }
        public string AuthorAvatar { get; set; }
        public string Content { get; set; }
        public DateTime CreateDate { get; set; }
        public List<ReplyDto> Replies { get; set; } = new(); // Reply to Post (nested)
    }

    public class ReplyDto
    {
        public int ReplyId { get; set; }
        public string Author { get; set; }
        public string AuthorAvatar { get; set; }
        public string Content { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
