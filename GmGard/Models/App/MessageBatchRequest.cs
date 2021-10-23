using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class MessageBatchRequest
    {
        public enum BatchAction
        {
            None,
            MarkRead,
            Delete,
        }
        public BatchAction Action { get; set; }
        public IEnumerable<int> MsgIds { get; set; }
    }
}
