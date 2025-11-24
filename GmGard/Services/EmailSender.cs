using GmGard.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public class EmailSender
    {
        public class EmailSettings
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public string Email { get; set; }
            public string DisplayName { get; set; }
            public string Password { get; set; }
            public bool EnableSsl { get; set; }
        }

        private readonly EmailSettings _settings;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostEnvironment _env;

        public EmailSender(IOptions<EmailSettings> config, ILoggerFactory logger, IHttpContextAccessor httpContextAccessor, IHostEnvironment env)
        {
            _settings = config.Value;
            _logger = logger.CreateLogger<EmailSender>();
            _httpContextAccessor = httpContextAccessor;
            _env = env;
        }

        private string GetSiteHost()
        {
            if (_env.IsDevelopment())
            {
                return SiteConstant.DevSite.Host;
            }
            var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
            if (host != null && SiteConstant.Sites.TryGetValue(host, out var site))
            {
                return site.Host;
            }
            return SiteConstant.DefaultSite.Host;
        }

        public Task SendPWEmailForUserAsync(UserProfile user, string reseturl)
        {
            if (_settings == null)
            {
                throw new ApplicationException("Null settings");
            }
            MailMessage mail = new MailMessage(new MailAddress(_settings.Email, _settings.DisplayName), new MailAddress(user.Email, user.UserName));
            //mail.Sender = mail.From;
            mail.Subject = "重设密码请求";
            mail.Body = $@"{user.NickName}，您好！<br>您在绅士之庭（{GetSiteHost()}）申请了重设密码。请点击下面的链接继续您的重设密码操作。<br><br>
                <ul><li>用户名：{user.UserName}</li><li><a href='{reseturl}'>{reseturl}</a></li><br><br>
                如果您没有进行重设密码申请，那么可能是其他人输错了邮箱地址。敬请忽略此邮件，您的密码将不会改变。<br>
                请不要公开您的账号密码，并将密码记录在安全的位置。如有任何疑问，请直接回复此邮件以联系管理员。<br>绅士之庭随时欢迎您的光临！<br>";
            mail.IsBodyHtml = true;
            var client = new SmtpClient(_settings.Host, _settings.Port);
            client.Credentials = new NetworkCredential(_settings.Email, _settings.Password);
            client.EnableSsl = _settings.EnableSsl;
            var t = new TaskCompletionSource<bool>();
            client.SendCompleted += (sender, arg) =>
            {
                if (arg.Error != null)
                {
                    _logger.LogError(new EventId(3), arg.Error, arg.Error.Message);
                }
                t.SetResult(true);
            };
            client.SendAsync(mail, null);
            return t.Task;
        }
    }
}
