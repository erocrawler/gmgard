using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class MessageDisplay
    {
        public int MessageId { get; set; }
        public bool IsRead { get; set; }
        public string Title { get; set; }
        public DateTime SendDate { get; set; }
        public string QuickLink { get; set; }
        public string QuickText { get; set; }
        public string Sender { get; set; }
        public string SenderNickName { get; set; }
        public string Recipient { get; set; }
        public string RecipientNickName { get; set; }
    }

    public class MessageDetails : MessageDisplay
    {
        public string Content { get; set; }
        public string SenderAvatar { get; set; }
        public string SenderLink { get; set; }
        public string RecipientAvatar { get; set; }
        public string RecipientLink { get; set; }
    }
}
