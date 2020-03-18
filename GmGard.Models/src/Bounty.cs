using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmGard.Models
{
    public class Bounty
    {
        [Key]
        public int BountyId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public string ImageUrls { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsDeleted { get; set; }
        public int Prize { get; set; }
        public bool IsAccepted { get; set; }
        public int ViewCount { get; set; }

        public virtual Answer AcceptedAnswer { get; set; }

        [InverseProperty("Bounty")]
        public virtual List<Answer> Answers { get; set; }
    }
}
