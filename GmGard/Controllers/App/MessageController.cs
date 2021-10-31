using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Extensions;
using GmGard.Filters;
using GmGard.Models;
using GmGard.Models.App;
using GmGard.Services;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [EnableCors("GmAppOrigin")]
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class MessageController : AppControllerBase
    {
        private UsersContext _udb;
        private AppSettingsModel _appSettings;
        private IMemoryCache _cache;
        private BlogUtil _blogUtil;
        private MessageUtil _msgUtil;
        private ConstantUtil _constantUtil;
        private readonly HtmlSanitizerService _sanitizerService;

        public MessageController(
            IOptions<AppSettingsModel> appSettings,
            UsersContext udb,
            BlogUtil blogUtil,
            MessageUtil msgUtil,
            HtmlSanitizerService htmlSanitizerService,
            ConstantUtil constantUtil,
            IMemoryCache cache)
        {
            _udb = udb;
            _blogUtil = blogUtil;
            _msgUtil = msgUtil;
            _appSettings = appSettings.Value;
            _cache = cache;
            _sanitizerService = htmlSanitizerService;
            _constantUtil = constantUtil;
        }

        private int MsgPageSize => _appSettings.MsgPageSize;

        private Paged<MessageDisplay> ToPaged(X.PagedList.IPagedList<Message> messages, bool quickLinks = false)
        {
            return new Paged<MessageDisplay>(new X.PagedList.StaticPagedList<MessageDisplay>(messages.Select(m =>
            {
                var d = new MessageDisplay
                {
                    IsRead = m.IsRead,
                    MessageId = m.MsgId,
                    Recipient = m.Recipient,
                    RecipientNickName = _blogUtil.GetNickName(m.Recipient),
                    Sender = m.Sender,
                    SenderNickName = _blogUtil.GetNickName(m.Sender),
                    SendDate = m.MsgDate,
                    Title = m.MsgTitle,
                };
                if (quickLinks && (m.MsgTitle == "新回复通知" || m.MsgTitle == "提及通知"))
                {
                    var link = BlogHelper.getNthLink(2, m.MsgContent, out string txt);
                    d.QuickLink = $"//{_constantUtil.SiteHost}{link}";
                    d.QuickText = txt;
                }
                return d;
            }), messages.GetMetaData()));
        }

        public async Task<JsonResult> Inbox(int pagenum = 1, bool unreadOnly = false, bool reportOnly = false)
        {
            string username = User.Identity.Name;
            var messages = _udb.Messages.Where(m => m.Recipient == username && !m.IsRecipientDelete);
            if (unreadOnly)
            {
                messages = messages.Where(m => m.IsRead == false);
            }
            if (reportOnly)
            {
                messages = messages.Where(m => m.MsgTitle == "汇报投稿问题");
            }
            var result = await messages.OrderByDescending(m => m.MsgDate).ToPagedListAsync(pagenum, MsgPageSize);
            return Json(ToPaged(result, quickLinks: true));
        }

        public async Task<JsonResult> Outbox(int pagenum = 1)
        {
            string username = User.Identity.Name;
            IQueryable<Message> messages = _udb.Messages.Where(m => m.Sender == username && !m.IsSenderDelete).OrderByDescending(m => m.MsgDate);
            var result = await messages.ToPagedListAsync(pagenum, MsgPageSize);
            return Json(ToPaged(result));
        }

        private Task<bool> UserExists(string user)
        {
            return _udb.Users.AnyAsync(u => u.UserName == user);
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromServices] IOptions<ApiBehaviorOptions> apiBehaviorOptions, SendMessageRequest req)
        {
            if (!await UserExists(req.Recipient))
            {
                ModelState.AddModelError(nameof(req.Recipient), "查无此人");
                return apiBehaviorOptions.Value.InvalidModelStateResponseFactory(ControllerContext);
            }
            Message message = new();
            message.MsgTitle = req.Title;
            message.Recipient = req.Recipient;
            message.MsgContent = System.Net.WebUtility.HtmlEncode(req.Content);
            message.MsgDate = DateTime.Now;
            message.Sender = User.Identity.Name;
            _udb.Messages.Add(message);
            await _udb.SaveChangesAsync();
            _cache.Remove("unreadmsg" + message.Recipient.ToLower());
            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            Message m = await _udb.Messages.FindAsync(id);
            if (m == null)
            {
                return NotFound();
            }
            if (!CheckUserOwnsMessage(m))
            {
                return Forbid();
            }
            if (!m.IsRead)
            {
                _cache.Remove("unreadmsg" + m.Recipient.ToLower());
            }
            DeleteMessage(m);
            await _udb.SaveChangesAsync();
            return Ok();
        }

        public async Task<ActionResult> Content(int id, bool markRead = false)
        {
            Message m = await _udb.Messages.FindAsync(id);
            if (m == null)
            {
                return NotFound();
            }
            if (!CheckUserOwnsMessage(m))
            {
                return Forbid();
            }
            if (markRead && !m.IsRead)
            {
                m.IsRead = true;
                await _udb.SaveChangesAsync();
                _cache.Remove("unreadmsg" + User.Identity.Name.ToLower());
            }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(m.MsgContent);
            var nodes = doc.DocumentNode.SelectNodes("//a");
            if (nodes != null)
            {
                foreach (var n in nodes)
                {
                    if (Uri.TryCreate(n.GetAttributeValue("href", null), UriKind.Relative, out Uri uri))
                    {
                        n.SetAttributeValue("href", $"//{_constantUtil.SiteHost}{uri}");
                        n.SetAttributeValue("target", "_blank");
                    }
                }
            }
            return Json(new MessageDetails
            {
                Sender = m.Sender,
                SenderNickName = _blogUtil.GetNickName(m.Sender),
                SenderAvatar = Url.Action("Show", "Avatar", new { name = m.Sender }, Request.Scheme),
                SenderLink = Url.Action("UserInfo", "Home", new { name = m.Sender }, Request.Scheme),
                Recipient = m.Recipient,
                RecipientNickName = _blogUtil.GetNickName(m.Recipient),
                RecipientAvatar = Url.Action("Show", "Avatar", new { name = m.Recipient }, Request.Scheme),
                RecipientLink = Url.Action("UserInfo", "Home", new { name = m.Recipient }, Request.Scheme),

                MessageId = m.MsgId,
                SendDate = m.MsgDate,
                Title = m.MsgTitle,
                Content = doc.DocumentNode.InnerHtml,
            });
        }

        private bool CheckUserOwnsMessage(Message m)
        {
            return m.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                || m.Sender.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                || (m.Recipient.Equals("admin", StringComparison.OrdinalIgnoreCase) && _blogUtil.CheckAdmin());
        }

        private void DeleteMessage(Message m)
        {
            string username = User.Identity.Name;
            if (m.Sender.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                m.IsSenderDelete = true;
            }
            if (m.Recipient.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                m.IsRecipientDelete = true;
            }

            if (m.IsSenderDelete && m.IsRecipientDelete)
            {
                _udb.Messages.Remove(m);
            }
        }

        public async Task<ActionResult> Batch(MessageBatchRequest request)
        {
            if (request.MsgIds == null)
            {
                return NoContent();
            }
            var msgs = await _udb.Messages.Where(m => request.MsgIds.Contains(m.MsgId)).ToListAsync();
            if (!msgs.All(CheckUserOwnsMessage))
            {
                return Forbid();
            }
            switch (request.Action)
            {
                case MessageBatchRequest.BatchAction.Delete:
                    msgs.ForEach(DeleteMessage);
                    break;
                case MessageBatchRequest.BatchAction.MarkRead:
                    foreach (var msg in msgs)
                    {
                        msg.IsRead = true;
                    }
                    break;
            }
            await _udb.SaveChangesAsync();
            _cache.Remove("unreadmsg" + User.Identity.Name.ToLower());
            return Ok();
        }
    }
}
