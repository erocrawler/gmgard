using System;
using System.Collections.Generic;

namespace GmGard.Client.Models;

public class CurrentUser
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string NickName { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Experience { get; set; }
    public int Level { get; set; }
    public string Avatar { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public int ConsecutiveSign { get; set; }
}

public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string Captcha { get; set; } = string.Empty;
}

public class LoginResult
{
    public bool Success { get; set; }
    public bool Require2fa { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class AuthStatus
{
    public bool IsAuthenticated { get; set; }
}

public class PunchInCost
{
    public int CurrentPoints { get; set; }
    public int Cost { get; set; }
    public int Tickets { get; set; }
}

public class PunchInResult
{
    public bool Success { get; set; }
    public int ConsecutiveDays { get; set; }
    public int Points { get; set; }
    public int ExpBonus { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PunchInHistoryItem
{
    public string TimeStamp { get; set; } = string.Empty;
    public bool IsMakeup { get; set; }
}

public class PunchInHistoryResponse
{
    public List<PunchInHistoryItem> PunchIns { get; set; } = new();
    public int? LegacySignDays { get; set; }
    public DateTime? MinSignDate { get; set; }
}

public class BlogPreview
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Brief { get; set; } = string.Empty;
    public string ThumbUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
    public User? Author { get; set; }
}

public class User
{
    public string Name { get; set; } = string.Empty;
}

public class DLsite
{
    public string RjCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Circle { get; set; } = string.Empty;
    public string CircleUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// ── Message ────────────────────────────────────────────────────────────
public class MessageDisplay
{
    public int MessageId { get; set; }
    public bool IsRead { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime SendDate { get; set; }
    public string? QuickLink { get; set; }
    public string? QuickText { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string SenderNickName { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string RecipientNickName { get; set; } = string.Empty;
}

public class MessageDetails : MessageDisplay
{
    public string Content { get; set; } = string.Empty;
    public string SenderAvatar { get; set; } = string.Empty;
    public string SenderLink { get; set; } = string.Empty;
    public string RecipientAvatar { get; set; } = string.Empty;
    public string RecipientLink { get; set; } = string.Empty;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageCount { get; set; }
    public int TotalItemCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class SendMessageRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
}

public class UserSuggestion
{
    public string UserName { get; set; } = string.Empty;
    public string? NickName { get; set; }
}

// ── Raffle ─────────────────────────────────────────────────────────────
public class RaffleConfig
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime EventStart { get; set; }
    public DateTime EventEnd { get; set; }
    public bool IsActive { get; set; }
    public bool HasRaffle { get; set; }
    public int Points { get; set; }
    public int RaffleCost { get; set; }
    public string Image { get; set; } = string.Empty;
}

// ── 2FA Account ────────────────────────────────────────────────────────
public class TwoFactorAuthenticationModel
{
    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public bool Is2faEnabled { get; set; }
    public bool IsMachineRemembered { get; set; }
}

public class TwoFactorAuthSharedKey
{
    public string SharedKey { get; set; } = string.Empty;
    public string AuthenticatorUri { get; set; } = string.Empty;
}

public class TwoFactorLoginRequest
{
    public bool RememberMe { get; set; }
    public bool RememberMachine { get; set; }
    public string TwoFactorCode { get; set; } = string.Empty;
}

public class RecoveryCodeLoginRequest
{
    public string RecoveryCode { get; set; } = string.Empty;
}

// ── Audit Exam ─────────────────────────────────────────────────────────
public class Exam
{
    public string Version { get; set; } = string.Empty;
    public List<Question> Questions { get; set; } = new();
}

public class Question
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Choices { get; set; } = new();
    public string Type { get; set; } = string.Empty;
}

public class ExamSubmission
{
    public List<QuestionSubmission> ExamAnswers { get; set; } = new();
    public string ExamVersion { get; set; } = string.Empty;
}

public class QuestionSubmission
{
    public int QuestionId { get; set; }
    public string Answer { get; set; } = string.Empty;
}

public class ExamAnswer
{
    public int QuestionId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public int Point { get; set; }
}

public class ExamResult
{
    public List<ExamAnswer> ExamAnswers { get; set; } = new();
    public string ExamVersion { get; set; } = string.Empty;
    public bool HasPassed { get; set; }
    public DateTime SubmitTime { get; set; }
    public bool IsSubmitted { get; set; }
    public int Score { get; set; }
}

// ── Admin ──────────────────────────────────────────────────────────────
public class InvitationCodeRequest
{
    public string? UserName { get; set; }
    public string? Code { get; set; }
}

public class CodeDetail
{
    public string Code { get; set; } = string.Empty;
    public AppUser? UsedBy { get; set; }
}

public class AppUser
{
    public string UserName { get; set; } = string.Empty;
    public string? NickName { get; set; }
    public string Avatar { get; set; } = string.Empty;
}

public class InvitationCodeResponse
{
    public List<CodeDetail> Codes { get; set; } = new();
    public AppUser User { get; set; } = new();
    public AppUser? InvitedBy { get; set; }
}

public class CategoryAdmin
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool LinkOptional { get; set; }
    public bool DisableRanking { get; set; }
    public bool DisableRating { get; set; }
    public bool HideFromHomePage { get; set; }
    public int? ParentCategoryID { get; set; }
}

public class RaffleAdminConfig : RaffleConfig
{
    // same fields plus admin extras handled by AdminService
}

public class DraftResult
{
    public int Total { get; set; }
    public List<DraftResultItem> Result { get; set; } = new();
}

public class DraftResultItem
{
    public AppUser User { get; set; } = new();
    public string Code { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
