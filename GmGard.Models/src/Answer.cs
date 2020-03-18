using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GmGard.Models
{
    public class Answer
    {
        [Key]
        public int AnswerId { get; set; }

        public int BountyId { get; set; }

        public string Author { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreateDate { get; set; }

        [ForeignKey("BountyId")]
        public virtual Bounty Bounty { get; set;}
    }
}