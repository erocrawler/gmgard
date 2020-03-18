using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using GmGard.Models;
using GmGard.Models.App;
using GmGard.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/Bounty/[action]")]
    [EnableCors("GmAppOrigin")]
    [Authorize]
    [ApiController]
    public class BountyController : AppControllerBase
    {
        private const int BountyPageSize = 20;
        private BlogContext db_;

        public BountyController(BlogContext db)
        {
            db_ = db;
        }

        public IActionResult List(int page = 1, BountyShowType showType = BountyShowType.All)
        {
            IQueryable<Bounty> query = db_.Bounties.Where(b => !b.IsDeleted);
            if (showType == BountyShowType.Answered)
            {
                query = query.Where(b => b.IsAccepted);
            }
            else if (showType == BountyShowType.Pending)
            {
                query = query.Where(b => !b.IsAccepted);
            }
            string avatarUrlBase = Url.Action("Show", "Avatar", null, Request.Scheme) + "/";
            var model = query.Select(q => new BountyPreview
            {
                Id = q.BountyId,
                Author = q.Author,
                AuthorAvatar = avatarUrlBase + q.Author,
                Content = q.Content,
                CreateDate = q.CreateDate,
                Prize = q.Prize,
                Title = q.Title,
                AnswerCount = q.Answers.Count,
                IsAccepted = q.IsAccepted,
            }).OrderByDescending(q => q.CreateDate);
            var paged = new Paged<BountyPreview>(model.ToPagedList(page, BountyPageSize));
            return Json(paged);
        }
    }
}