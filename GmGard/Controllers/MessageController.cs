using GmGard.Filters;
using GmGard.Models;
using GmGard.Services;
using GmGard.ViewComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using X.PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using static GmGard.ViewComponents.MessageView;

namespace GmGard.Controllers
{
    [Authorize, ResponseCache(CacheProfileName = "Never")]
    public class MessageController : Controller
    {
        private BlogContext _db;
        private UsersContext _udb;
        private AppSettingsModel _appSettings;
        private IMemoryCache _cache;
        private BlogUtil _blogUtil;
        private MessageUtil _msgUtil;

        public MessageController(
            IOptions<AppSettingsModel> appSettings,
            BlogContext db,
            UsersContext udb,
            BlogUtil blogUtil,
            MessageUtil msgUtil,
            IMemoryCache cache)
        {
            _db = db;
            _udb = udb;
            _blogUtil = blogUtil;
            _msgUtil = msgUtil;
            _appSettings = appSettings.Value;
            _cache = cache;
        }
        
        private int pagesize => _appSettings.MsgPageSize;
        //
        // GET: /Message/

        public ActionResult Index()
        {
            object context = null;
            TempData.TryGetValue("DisplayTab", out context);
            if (context != null)
            {
                ViewBag.DisplayTab = context;
            }
            return View();
        }

        public ActionResult Inbox(bool unreadOnly = false, int pagenum = 1)
        {
            return ViewComponent(nameof(MessageView), new { context = "inbox", pagenum, unreadOnly });
        }

        public ActionResult ReportInbox(int pagenum = 1)
        {
            return ViewComponent(nameof(MessageView), new { context = "ReportInbox", pagenum });
        }

        public ActionResult Outbox(int pagenum = 1)
        {
            return ViewComponent(nameof(MessageView), new { context = "outbox", pagenum });
        }

        [HttpGet]
        public ActionResult Write()
        {
            return ViewComponent(nameof(MessageView), new { context = "write" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken, ValidateCaptcha]
        public ActionResult Write(Message message)
        {
            if (ModelState.IsValid)
            {
                message.MsgDate = DateTime.Now;
                message.Sender = User.Identity.Name;
                _udb.Messages.Add(message);
                _udb.SaveChanges();
                _cache.Remove("unreadmsg" + message.Recipient.ToLower());
                TempData["DisplayTab"] = "outbox";
                return RedirectToAction("Index");
            }
            TempData["WriteMessage"] = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            return ViewComponent(nameof(MessageView), new { context = "write" });
        }

        [HttpPost]
        public ActionResult ReadMsg(string id)
        {
            int msgid;
            int unread;
            if (int.TryParse(id, out msgid))
            {
                string username = User.Identity.Name;
                Message message = _udb.Messages.Find(msgid);
                unread = _udb.Messages.Count(m => m.Recipient == username && !m.IsRead && !m.IsRecipientDelete);
                if (message == null || message.IsRecipientDelete || !message.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound();
                }
                if (!message.IsRead)
                {
                    message.IsRead = true;
                    unread--;
                    _udb.SaveChanges();
                }
                _cache.Set("unreadmsg" + username.ToLower(), unread);
                return Json(new
                {
                    MsgId = message.MsgId,
                    MsgTitle = message.MsgTitle,
                    Sender = message.Sender,
                    Recipient = message.Recipient,
                    MsgDate = message.MsgDate,
                    MsgContent = message.MsgContent,
                    unreadcount = unread,
                    SenderNick = _blogUtil.GetNickName(message.Sender),
                    RecipientNick = _blogUtil.GetNickName(message.Recipient)
                });
            }
            return NotFound();
        }

        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult ShowAdminInbox(int pagenum = 1)
        {
            return ViewComponent(nameof(MessageView), new { context = "AdminInbox", pagenum = pagenum });
        }

        [Authorize(Roles = "Administrator, Moderator")]
        [HttpPost]
        public ActionResult GetAdminMsg(string id)
        {
            int msgid;
            if (int.TryParse(id, out msgid))
            {
                Message message = _udb.Messages.Find(msgid);
                if (message == null || message.IsRecipientDelete || !message.Recipient.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound();
                }
                return Json(new
                {
                    MsgId = message.MsgId,
                    MsgTitle = message.MsgTitle,
                    Sender = message.Sender,
                    Recipient = message.Recipient,
                    MsgDate = message.MsgDate,
                    MsgContent = message.MsgContent,
                    SenderNick = _blogUtil.GetNickName(message.Sender),
                    RecipientNick = _blogUtil.GetNickName(message.Recipient)
                });
            }
            return NotFound();
        }

        [HttpPost]
        public ActionResult GetMsg(string id)
        {
            int msgid;
            if (int.TryParse(id, out msgid))
            {
                Message message = _udb.Messages.Find(msgid);
                if (message == null || message.IsSenderDelete || !message.Sender.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound();
                }
                return Json(new
                {
                    MsgId = message.MsgId,
                    MsgTitle = message.MsgTitle,
                    Sender = message.Sender,
                    Recipient = message.Recipient,
                    MsgDate = message.MsgDate,
                    MsgContent = message.MsgContent,
                    SenderNick = _blogUtil.GetNickName(message.Sender),
                    RecipientNick = _blogUtil.GetNickName(message.Recipient)
                });
            }
            return NotFound();
        }

        //
        // GET: /Message/Delete/5

        public ActionResult MsgAction(string msgid, string act)
        {
            int id = int.Parse(msgid);
            Message message = _udb.Messages.Find(id);
            if (!CheckUserOwnsMessage(message))
            {
                return NotFound();
            }
            switch (act)
            {
                case "delete":
                    bool isrecipient = DeleteMessage(message);
                    if (!isrecipient)
                    {
                        TempData["DisplayTab"] = "outbox";
                    }
                    return RedirectToAction("Index");

                case "reply":
                    TempData["WriteAction"] = WriteAction.reply;
                    TempData["WriteMessage"] = JsonConvert.SerializeObject(message);
                    TempData["DisplayTab"] = "write";
                    return RedirectToAction("Index");

                case "forward":
                    TempData["WriteAction"] = WriteAction.forward;
                    TempData["WriteMessage"] = JsonConvert.SerializeObject(message);
                    TempData["DisplayTab"] = "write";
                    return RedirectToAction("Index");
            }
            return NotFound();
        }

        public ActionResult ReportAction(int id, string act, int pagenum = 1)
        {
            Message msg = _udb.Messages.Find(id);
            if (!CheckUserOwnsMessage(msg))
            {
                return NotFound();
            }
            switch (act)
            {
                case "delete":
                    DeleteMessage(msg);
                    break;

                case "solve":
                    if (msg.MsgTitle.Equals("汇报投稿问题"))
                    {
                        msg.IsRead = true;
                        msg.MsgTitle = "汇报投稿问题（已解决）";
                        _udb.SaveChanges();
                    }
                    break;
            }
            _cache.Remove("unreadmsg" + User.Identity.Name.ToLower());
            return new EmptyResult();
        }

        private bool CheckUserOwnsMessage(Message m)
        {
            return m != null && (m.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                || m.Sender.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                || (m.Recipient.Equals("admin", StringComparison.OrdinalIgnoreCase) && _blogUtil.CheckAdmin()));
        }

        private bool DeleteMessage(Message m, bool save = true)
        {
            string username = User.Identity.Name;
            bool isrecipient = true;
            if (m.Sender.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                m.IsSenderDelete = true;
                isrecipient = false;
            }
            if (m.Recipient.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                m.IsRecipientDelete = true;
                isrecipient = true;
            }

            if (m.IsSenderDelete && m.IsRecipientDelete)
            {
                _udb.Messages.Remove(m);
            }
            if (save)
                _udb.SaveChanges();
            _cache.Remove("unreadmsg" + username.ToLower());
            return isrecipient;
        }

        [HttpPost]
        public ActionResult CheckUsername(string Recipient)
        {
            if (_udb.Users.Any(u => u.UserName == Recipient))
            {
                return Json(true);
            }
            else
                return Json("查无此人");
        }

        [HttpPost]
        public async Task<ActionResult> MultiAct(string action, IEnumerable<string> op, string pos)
        {
            string user = User.Identity.Name;
            var sqlUser = new SqlParameter("user", user);
            List<int> opids = new List<int>();
            List<Message> msgs = null;
            if (op != null)
            {
                foreach (string opid in op)
                {
                    int id;
                    if (int.TryParse(opid, out id))
                    {
                        opids.Add(id);
                    }
                }
                msgs = await _udb.Messages.Where(m => opids.Contains(m.MsgId)).ToListAsync();
            }
            if (pos == "inbox")
            {
                switch (action)
                {
                    case "del-all":
                        await _udb.Database.ExecuteSqlCommandAsync(
                                @"Delete from Messages where IsSenderDelete='true' and Recipient=@user;
                                  Delete from Messages where Recipient=@user and Sender=@user;
                                  Update Messages set IsRecipientDelete='true' where Recipient=@user;", sqlUser);
                        break;

                    case "del-sel":
                        if (msgs != null)
                        {
                            foreach (var msg in msgs)
                            {
                                DeleteMessage(msg, false);
                            }
                        }
                        break;

                    case "read-sel":
                        if (msgs != null)
                            foreach (var msg in msgs)
                            {
                                if (CheckUserOwnsMessage(msg))
                                {
                                    msg.IsRead = true;
                                }
                            }
                        break;

                    case "read-all":
                        await _udb.Database.ExecuteSqlCommandAsync("Update Messages set IsRead='true' where Recipient=@user and IsRead='false'", sqlUser);
                        break;
                }
                await _udb.SaveChangesAsync();
                _cache.Set("unreadmsg" + user.ToLower(), await _udb.Messages.CountAsync(m => m.Recipient == user && !m.IsRead && !m.IsRecipientDelete));
            }
            else if (pos == "outbox")
            {
                switch (action)
                {
                    case "del-all":
                        await _udb.Database.ExecuteSqlCommandAsync(
                                @"Delete from Messages where IsRecipientDelete='true' and Sender=@user;
                                  Delete from Messages where Recipient=@user and Sender=@user;
                                  Update Messages set IsSenderDelete='true' where Sender=@user;", sqlUser);
                        break;

                    case "del-sel":
                        if (msgs != null)
                        {
                            foreach (var msg in msgs)
                            {
                                DeleteMessage(msg, false);
                            }
                            await _udb.SaveChangesAsync();
                        }
                        break;
                }
            }
            else
                return RedirectToAction("Index");
            TempData["DisplayTab"] = pos;
            return RedirectToAction("Index");
        }



        [Authorize]
        public ActionResult Report(int? id, ItemType itemType, int? postid, string MsgContent, string type)
        {
            if (id.HasValue)
            {
                string hashtag = null;
                if (postid.HasValue)
                {
                    hashtag = "#listpost" + postid;
                }
                string controller = itemType == ItemType.Topic ? "Topic" : "Blog";
                string url = Url.Action("Details", controller, new { id = id }) + hashtag;
                string content = System.Net.WebUtility.HtmlEncode(MsgContent) + "<br>地址：<br><a href='" + url + "'>" + url + "</a>";
                if (type == "rpt-author")
                {
                    string author;
                    if (itemType == ItemType.Topic)
                    {
                        var t = _db.Topics.Find(id.Value);
                        author = t?.Author;
                    }
                    else
                    {
                        var blog = _db.Blogs.Find(id.Value);
                        author = blog?.Author;
                    }
                    if (author == null)
                    {
                        return NotFound();
                    }
                    _msgUtil.AddMsg(User.Identity.Name, author, "汇报投稿问题", content);
                }
                else
                {
                    _blogUtil.AddBlogPost(-1, User.Identity.Name, content);
                }
                return Json(new { msg = "已成功汇报" });
            }
            return NotFound();
        }
    }
}