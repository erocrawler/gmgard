using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class AuditExamConfig
    {
        public enum QuestionType
        {
            Information,
            MultipleChoice,
            TitleCombination,
            FillInBlank,
        }

        public class ExamAnswer
        {
            public int Id { get; set; }
            public QuestionType Type { get; set; }
            public string Answer { get; set; }
            public string Comment { get; set; }
            public decimal Point { get; set; }
        }

        public class ExamConfig
        {
            public int PassScore { get; set; }
            public string Version { get; set; }
            public IEnumerable<ExamAnswer> Answers { get; set; }
        }

        public IEnumerable<ExamConfig> Exams { get; set; }
    }
}
