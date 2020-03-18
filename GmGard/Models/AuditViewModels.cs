namespace GmGard.Models
{
    public class AuditModel
    {
        public Blog blog { get; set; }
        public BlogAudit audit { get; set; }
    }

    public class VoteResult
    {
        public BlogAudit Audit { get; set; }
        public bool? Correct { get; set; }
    }
}