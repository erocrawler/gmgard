using System;
using System.Collections.Generic;

namespace GmGard.Client.Models;

public enum BountyShowType
{
    All = 0,
    Answered = 1,
    Pending = 2,
    Deleted = 3,
    Mine = 4
}

// Server returns GmGard.Models.App.Paged<T> with Items, PageCount, TotalItemCount, PageNumber, PageSize, Skip
// Named BountyPaged to avoid conflict with GmGard.Models.App.Paged<T> in server project (server references client)
public class BountyPaged<T>
{
    public List<T> Items { get; set; } = new();
    public int PageCount { get; set; }
    public int TotalItemCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int Skip { get; set; }
}

public class BountyPreview
{
    public int Id { get; set; }
    public DateTime CreateDate { get; set; }
    public string Author { get; set; } = string.Empty;
    public string AuthorAvatar { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int Prize { get; set; }
    public bool IsAccepted { get; set; }
    public int AnswerCount { get; set; }
    public int Deposit { get; set; }
    public int HelpfulReward { get; set; }
    public int ViewCount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? CloseDate { get; set; }
}

public class BountyDetail
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string[] ImageUrls { get; set; } = Array.Empty<string>();
    public string Author { get; set; } = string.Empty;
    public string AuthorAvatar { get; set; } = string.Empty;
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
    public List<BountyAnswerDto> Answers { get; set; } = new();
    public List<PostDto> BountyComments { get; set; } = new();
}

public class BountyAnswerDto
{
    public int AnswerId { get; set; }
    public int BountyId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string AuthorAvatar { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
    public bool IsBest { get; set; }
    public bool IsHelpful { get; set; }
    // Replies are stored as Post (IdType=Answer) with nested Reply
    public List<PostDto> Replies { get; set; } = new();
}

public class PostDto
{
    public int PostId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string AuthorAvatar { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
    public List<ReplyDto> Replies { get; set; } = new();
}

public class ReplyDto
{
    public int ReplyId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string AuthorAvatar { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
}

// Requests / Responses for client use
public class CreateBountyRequestClient
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ImageUrls { get; set; } = string.Empty; // semicolon joined
    public int Prize { get; set; }
    public int HelpfulReward { get; set; }
}

public class CreateAnswerRequestClient
{
    public int BountyId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class AcceptRequestClient
{
    public int BountyId { get; set; }
    public int BestAnswerId { get; set; }
    public int[]? HelpfulAnswerIds { get; set; }
}

public class CreateBountyResponse
{
    public int Id { get; set; }
    public int TotalCost { get; set; }
    public int Remaining { get; set; }
}

public class BountyCreateResult
{
    public int? Id { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int? TotalCost { get; set; }
    public int? Remaining { get; set; }
}

public class BountyConfigDto
{
    public int MinPrize { get; set; } = 40;
    public int MinHelpfulReward { get; set; } = 20;
    public int MinTotal { get; set; } = 100;
    public int ExpireDays { get; set; } = 30;
    public int ReportAfterHours { get; set; } = 48;
    public int ReportRewardMultiplier { get; set; } = 3;
    public int MaxTitleLength { get; set; } = 120;
    public int MinContentLength { get; set; } = 5;
    public int MaxContentLength { get; set; } = 10000;
    public int PageSize { get; set; } = 20;
    public bool AllowZeroHelpfulReward { get; set; } = true;
}

public class BountyAnswerResult
{
    public int? AnswerId { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}
