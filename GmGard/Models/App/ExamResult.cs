using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class ExamResult
    {
        public class ExamAnswer
        {
            public int QuestionId { get; set; }
            public string Answer { get; set; }
            public string Comment { get; set; }
            public decimal Point { get; set; }
            public bool Correct { get; set; }
        }

        public List<ExamAnswer> ExamAnswers { get; set; }

        public string ExamVersion { get; set; }

        public bool HasPassed { get; set; }

        public DateTime SubmitTime { get; set; }

        public bool IsSubmitted { get; set; }

        public decimal Score { get; set; }
    }
}
