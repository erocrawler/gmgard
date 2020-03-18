using System;

namespace GmGard.Models.App
{
    public class NewItemCount
    {
        public class ByCategory
        {
            public int Id { get; set; }
            public int Count { get; set; }
        }

        public DateTime Since { get; set; }
        public int Total { get; set; }
        public ByCategory[] ByCategories { get; set; }
    }
}
