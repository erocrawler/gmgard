using GmGard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Extensions;

namespace GmGard.ViewComponents
{
    public class MessageView : ViewComponent
    {
        internal enum WriteAction
        {
            none = 0,
            reply,
            forward,
            write
        }

        private readonly UsersContext _udb;
        private readonly DataSettingsModel _dataSettings;
        private readonly IMemoryCache _cache;
        private readonly int _msgPageSize;

        public MessageView(UsersContext udb, IOptionsSnapshot<DataSettingsModel> dataSettings, IOptions<AppSettingsModel> appSettings, IMemoryCache cache)
        {
            _udb = udb;
            _dataSettings = dataSettings.Value;
            _cache = cache;
            _msgPageSize = appSettings.Value.MsgPageSize;
        }

        public IViewComponentResult Invoke(string context, int pagenum = 1, bool unreadOnly = false)
        {
            if (context == "outbox")
            {
                return Outbox(pagenum);
            }
            else if (context == "write")
            {
                return Write();
            }
            else if (context == "ReportInbox")
            {
                return ReportInbox(pagenum);
            }
            else if (context == "AdminInbox")
            {
                return AdminInbox(pagenum);
            }
            else
            {
                return Inbox(pagenum, unreadOnly);
            }
        }

        private void SetUnreadCount()
        {
            ViewBag.UnreadCount = _udb.Messages.Count(m => m.Recipient == User.Identity.Name && !m.IsRead && !m.IsRecipientDelete);
        }

        public IViewComponentResult Inbox(int pagenum = 1, bool unreadOnly = false)
        {
            string username = User.Identity.Name;
            IQueryable<Message> messages = _udb.Messages.Where(m => m.Recipient == username && !m.IsRecipientDelete && (m.IsRead == false || !unreadOnly)).OrderByDescending(m => m.MsgDate);
            SetUnreadCount();
            ViewBag.UnreadOnly = unreadOnly;
            return View("Inbox", messages.ToPagedList(pagenum, _msgPageSize));
        }

        public IViewComponentResult ReportInbox(int pagenum = 1)
        {
            string username = User.Identity.Name;
            IQueryable<Message> messages = _udb.Messages.Where(m => m.Recipient == username && !m.IsRecipientDelete && m.MsgTitle == "汇报投稿问题")
                .OrderByDescending(m => m.MsgDate);
            SetUnreadCount();
            return View("ReportInbox", messages.ToPagedList(pagenum, _msgPageSize));
        }

        public IViewComponentResult Outbox(int pagenum = 1)
        {
            string username = User.Identity.Name;
            IQueryable<Message> messages = _udb.Messages.Where(m => m.Sender == username && !m.IsSenderDelete).OrderByDescending(m => m.MsgDate);
            SetUnreadCount();
            return View("Outbox", messages.ToPagedList(pagenum, _msgPageSize));
        }

        public IViewComponentResult AdminInbox(int pagenum = 1)
        {
            string username = "admin";
            IQueryable<Message> messages = _udb.Messages.Where(m => m.Recipient == username && !m.IsRecipientDelete).OrderByDescending(m => m.MsgDate);
            return View("ReadOnlyInbox", messages.ToPagedList(pagenum, _msgPageSize));
        }

        public IViewComponentResult Write()
        {
            WriteAction a = ViewContext.TempData.ContainsKey("WriteAction") ? (WriteAction)ViewContext.TempData["WriteAction"] : WriteAction.none;
            Message m = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(ViewContext.TempData["WriteMessage"] as string ?? string.Empty);
            string username = User.Identity.Name;

            switch (a)
            {
                case WriteAction.reply:
                    if (username != m.Sender)
                    {
                        m.Recipient = m.Sender;
                    }
                    m.MsgTitle = "回复：" + m.MsgTitle;
                    m.MsgContent = string.Format("\n\n---------{0}于{1}写道---------\n", m.Sender, m.MsgDate) + m.MsgContent;
                    break;

                case WriteAction.forward:
                    m.Recipient = "";
                    m.MsgTitle = "转发：" + m.MsgTitle;
                    m.MsgContent = string.Format("\n\n---------{0}于{1}写道---------\n", m.Sender, m.MsgDate) + m.MsgContent;
                    break;
                default:
                    m = new Message();
                    m.Recipient = ViewContext.TempData["WriteTo"] as string;
                    break;
            }
            return View("Write", m);
        }
    }
}
