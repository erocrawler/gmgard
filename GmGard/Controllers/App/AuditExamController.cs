using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using GmGard.Models;
using GmGard.Models.App;
using System.Data.Entity;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/AuditExam/[action]")]
    [EnableCors("GmAppOrigin")]
    [Authorize]
    [ApiController]
    public class AuditExamController : AppControllerBase
    {
        private readonly UsersContext db_;
        private readonly AuditExamConfig config_;

        public AuditExamController(UsersContext db, IOptionsSnapshot<AuditExamConfig> examConfig)
        {
            db_ = db;
            config_ = examConfig.Value;
        }

        [HttpGet]
        public async Task<ActionResult> Draft(string version)
        {
            if (!IsValidVersion(version))
            {
                return NotFound();
            }
            var draft = await db_.AuditExamSubmissions.SingleOrDefaultAsync(u => u.User.UserName == User.Identity.Name && u.Version == version && !u.IsSubmitted);
            ExamSubmission submission = null;
            if (draft != null)
            {
                submission = JsonConvert.DeserializeObject<ExamSubmission>(draft.RawSubmission);
            }
            return Json(submission);
        }

        [HttpPut]
        public async Task<ActionResult> Draft([FromBody]ExamSubmission submission)
        {
            if (submission == null || submission.ExamAnswers == null 
                || submission.ExamAnswers.Count == 0 || string.IsNullOrWhiteSpace(submission.ExamVersion))
            {
                return BadRequest();
            }
            if (!IsValidVersion(submission.ExamVersion))
            {
                return NotFound();
            }

            var drafts = await UserSubmissionsAsync();
            var draft = drafts.Value.SingleOrDefault(s => s.Version == submission.ExamVersion);
            if (draft == null)
            {
                draft = new AuditExamSubmission
                {
                    IsSubmitted = false,
                    UserID = drafts.Key.Id,
                    Version = submission.ExamVersion,
                };
                db_.AuditExamSubmissions.Add(draft);
            }
            draft.RawSubmission = JsonConvert.SerializeObject(submission);
            draft.SubmitTime = DateTime.Now;
            await db_.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult> Result(string version)
        {
            return await ResultForUser(version, User.Identity.Name);
        }

        [HttpGet]
        public async Task<ActionResult> Results()
        {
            var submissions = await db_.AuditExamSubmissions.Where(u => u.User.UserName == User.Identity.Name).ToListAsync();
            IEnumerable<ExamResult> results = null;
            if (submissions != null)
            {
                results = submissions.Select(submission =>
                {
                    ExamResult result;
                    if (submission.RawResult == null)
                    {
                        result = new ExamResult();
                    }
                    else
                    {
                        result = JsonConvert.DeserializeObject<ExamResult>(submission.RawResult);
                    }
                    result.ExamVersion = submission.Version;
                    result.IsSubmitted = submission.IsSubmitted;
                    result.SubmitTime = submission.SubmitTime;
                    result.Score = submission.Score;
                    result.ExamAnswers = null;
                    return result;
                });
            }
            return Json(results);
        }

        [HttpGet, Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> ResultForUser(string version, string user)
        {
            if (!IsValidVersion(version))
            {
                return NotFound();
            }
            var submission = await db_.AuditExamSubmissions.SingleOrDefaultAsync(u => u.User.UserName == user && u.Version == version && u.IsSubmitted);
            ExamResult result = null;
            if (submission != null)
            {
                result = JsonConvert.DeserializeObject<ExamResult>(submission.RawResult);
                result.SubmitTime = submission.SubmitTime;
                result.Score = submission.Score;
                result.IsSubmitted = submission.IsSubmitted;
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<ActionResult> Submit([FromBody]ExamSubmission submission)
        {
            if (submission == null || submission.ExamAnswers == null || submission.ExamAnswers.Count == 0)
            {
                return BadRequest();
            }
            if (!IsValidVersion(submission.ExamVersion))
            {
                return NotFound();
            }
            var submissions = await UserSubmissionsAsync();
            var submit = submissions.Value.SingleOrDefault(s => s.Version == submission.ExamVersion);
            if (submit == null)
            {
                submit = new AuditExamSubmission
                {
                    UserID = submissions.Key.Id,
                    Version = submission.ExamVersion,
                };
            }
            else if (submit.IsSubmitted)
            {
                return BadRequest();
            }
            submit.IsSubmitted = true;
            submit.RawSubmission = JsonConvert.SerializeObject(submission);

            ExamResult result = GetResult(submission);
            submit.SubmitTime = result.SubmitTime;
            submit.HasPassed = result.HasPassed;
            submit.Score = result.Score;
            submit.RawResult = JsonConvert.SerializeObject(result);
            await db_.SaveChangesAsync();

            return Json(result);
        }

        private bool IsValidVersion(string version)
        {
            return config_.Exams.Any(e => e.Version == version);
        }

        private ExamResult GetResult(ExamSubmission submission)
        {
            var submissionDict = submission.ExamAnswers.ToDictionary(e => e.QuestionId);
            var exam = config_.Exams.FirstOrDefault(e => e.Version == submission.ExamVersion);

            var examResult = exam.Answers.Aggregate(new ExamResult
            {
                ExamAnswers = new List<ExamResult.ExamAnswer>(),
                ExamVersion = exam.Version,
                Score = 0,
                SubmitTime = DateTime.Now,
                IsSubmitted = true,
            }, (results, actual) =>
            {
                var result = new ExamResult.ExamAnswer
                {
                    Point = 0,
                    QuestionId = actual.Id,
                    Comment = actual.Comment,
                    Correct = false,
                };
                if (submissionDict.ContainsKey(actual.Id))
                {
                    var submittedAnswer = submissionDict[actual.Id];
                    result.Answer = submittedAnswer.Answer;
                    result.Correct = AnswerEquals(submittedAnswer.Answer, actual.Answer);
                    result.Point = result.Correct ? actual.Point : 0;
                }
                results.ExamAnswers.Add(result);
                results.Score += result.Point;
                return results;
            });
            examResult.HasPassed = exam.PassScore <= examResult.ExamAnswers.Sum(e => e.Point);
            return examResult;
        }

        private bool AnswerEquals(string a, string b)
        {
            return a.Replace(" ", "").Equals(b.Replace(" ", ""), StringComparison.OrdinalIgnoreCase);
        }

        private async Task<KeyValuePair<UserProfile, IEnumerable<AuditExamSubmission>>> UserSubmissionsAsync()
        {
            var group = await db_.Users
                .GroupJoin(db_.AuditExamSubmissions, u => u.Id, u => u.UserID, (u, a) => new { submissions = a.DefaultIfEmpty(), user = u })
                .SingleOrDefaultAsync(g => g.user.UserName == User.Identity.Name);
            return new KeyValuePair<UserProfile, IEnumerable<AuditExamSubmission>>(group.user, group.submissions.Where(s => s != null));
        }
    }
}