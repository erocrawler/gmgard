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
