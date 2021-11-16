using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models
{
    public class RaffleConfig
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        public int RaffleCost { get; set; }
        public string Image { get; set; }
    }
}
