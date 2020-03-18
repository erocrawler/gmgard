using GmGard.Filters;
using GmGard.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GmGard.Models
{

    public class CommentChangeModel
    {
        [MaxLength(200, ErrorMessage = "签名不得超过200字符")]
        public string comment { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required(ErrorMessage = "请输入当前密码")]
        [DataType(DataType.Password)]
        [Display(Name = "当前密码")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "请输入新密码")]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认新密码")]
        [Compare("NewPassword", ErrorMessage = "新密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "请输入用户名")]
        [Display(Name = "用户名")]
        [RegularExpression("^[a-zA-Z0-9_\u2E80-\u9FFF]{1,20}$", ErrorMessage = "用户名包含无效字符")]
        [MaxLength(20)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我?")]
        public bool RememberMe { get; set; }

        [Required(ErrorMessage = "请输入验证码")]
        [Display(Name = "等于多少?")]
        public string Captcha { get; set; }
    }

    public class Login2FaModel
    {
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        [Required]
        [StringLength(7, ErrorMessage = "{0}字符数应在{2}到{1}之间。", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "验证码")]
        public string TwoFactorCode { get; set; }

        [Display(Name = "记住本设备")]
        public bool RememberMachine { get; set; }
    }

    public class LoginWithRecoveryCodeModel
    {
        public string ReturnUrl { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "应急密码")]
        public string RecoveryCode { get; set; }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "请输入用户名")]
        [Display(Name = "用户名")]
        [Remote("CheckUsername", "Account", ErrorMessage = "该用户名已注册", HttpMethod = "Post")]
        [RegularExpression("^[a-zA-Z0-9_\u2E80-\u9FFF]{1,20}$", ErrorMessage = "用户名包含无效字符")]
        [MinLength(3, ErrorMessage = "用户名不得少于3个字符"), MaxLength(20, ErrorMessage = "用户名不得超过20字符")]
        public string UserName { get; set; }

        [Display(Name = "昵称（留空则与用户名相同）"),
        RegularExpression("^[\\S+]{1,20}$", ErrorMessage = "昵称不能包含空格"),
        Remote("CheckNicknameUsed", "Account", HttpMethod = "Get", ErrorMessage = "该昵称已被使用")]
        public string NickName { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "请输入邮箱")]
        [DataType(DataType.EmailAddress, ErrorMessage = "请输入正确的电子邮箱")]
        [Display(Name = "电子邮箱")]
        [RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9]+(\\.[a-z0-9]+)*\\.([a-z]{2,4})$", ErrorMessage = "无效的电子邮箱")]
        [Remote("CheckEmail", "Account", HttpMethod = "Post", ErrorMessage = "该邮箱已被注册")]
        public string Email { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "头像（可选）")]
        [ValidateFile(1024 * 1024, ErrorMessage = "头像必须是图片，且不得超过1MB")]
        public IFormFile avatar { get; set; }

        [Required(ErrorMessage = "请输入验证码")]
        [Display(Name = "等于多少?")]
        public string Captcha { get; set; }

        [DefaultValue(0)]
        public int X { get; set; }

        [DefaultValue(0)]
        public int Y { get; set; }

        [DefaultValue(150)]
        public int W { get; set; }

        [DefaultValue(150)]
        public int H { get; set; }

        public int RegisterQuestionIndex { get; set; }

        public string GetRegisterQuestion(RegisterSettingsModel settings)
        {
            if (settings == null || settings.RegisterQuestions == null || RegisterQuestionIndex < 0 || settings.RegisterQuestions.Count <= RegisterQuestionIndex)
            {
                return null;
            }
            else
            {
                return settings.RegisterQuestions[RegisterQuestionIndex].Question;
            }
        }

        [Display(Name = "回答注册问题")]
        public string RegisterAnswer { get; set; }

        [Display(Name = "邀请码")]
        public string RegisterCode { get; set; }
    }

    public class PasswordResetModel
    {
        [Required(ErrorMessage = "请输入邮箱"), DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }
    }

    public class HPSettingsModel
    {
        public CategoryOptionDisplay GetCategoryOptions() => new CategoryOptionDisplay { SelectedIds = new HashSet<int>(CategoryIds ?? Enumerable.Empty<int>()) };

        public bool HideHarmony { get; set; }
        public IEnumerable<int> CategoryIds { get; set; }
        public string BlacklistTagNames { get; set; }
    }

    public class CategoryOptionDisplay
    {
        public ISet<int> SelectedIds { get; set; }
        public IDictionary<int, long> CategoryItemCount { get; set; }
    }

    public class FollowModel
    {
        public string UserName { get; set; }
        public bool FollowEachOther { get; set; }
        public string UserComment { get; set; }
        public int Experience { get; set; }
    }
}
