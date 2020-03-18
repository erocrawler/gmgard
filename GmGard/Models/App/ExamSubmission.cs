using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class ExamSubmission
    {
        public class QuestionSubmission
        {
            public int QuestionId { get; set; }
            public string Answer { get; set; }
        }

        public List<QuestionSubmission> ExamAnswers { get; set; }

        public string ExamVersion { get; set; }
    }
}
