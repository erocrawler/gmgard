using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using Amazon.S3.Model;
using GmGard.Models;
using GmGard.Models.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/Admin/[action]")]
    [EnableCors("GmAppOrigin")]
    [ApiController]
    [Authorize(Roles = "Administrator,Moderator")]
    public class AdminController : AppControllerBase
    {
        private readonly UsersContext udb_;
        private readonly BlogContext db_;
        private readonly IMemoryCache cache_;

        public AdminController(UsersContext udb, BlogContext db, IMemoryCache cache)
        {
            udb_ = udb;
            db_ = db;
            cache_ = cache;
        }

        [HttpGet, HttpPost]
        public async Task<ActionResult> InvitationCode(GetInvitationCodeRequest request)
        {
            IQueryable<UserCode> codes = udb_.UserCodes.Include(u => u.UsedByUser).Include(u => u.User);
            if (!string.IsNullOrEmpty(request.UserName))
            {
                codes = codes.Where(uc => uc.User.UserName == request.UserName);
            }
            if (!string.IsNullOrEmpty(request.Code))
            {
                if (!Guid.TryParse(request.Code, out Guid guid))
                {
                    return BadRequest(new { error = "Invalid code" });
                }
                codes = codes.Where(c => c.Code == guid);
            }
            var codeList = await codes.ToListAsync();
            UserProfile user;
            if (codeList.Count == 0)
            {
                user = await udb_.Users.SingleOrDefaultAsync(u => u.UserName == request.UserName);
                if (user == null)
                {
                    return NotFound();
                }
            }
            else
            {
                user = codeList.First().User;
            }
            InvitationCodeResponse response = new InvitationCodeResponse
            {
                Codes = codeList.Select(c => new CodeDetail
                {
                    Code = c.Code.ToString(),
                    UsedBy = c.UsedByUser == null ? null : Models.App.User.FromUserProfile(c.UsedByUser, Url.Action("Show", "Avatar", new { name = c.UsedByUser.UserName }, Request.Scheme))
                }),
                User = Models.App.User.FromUserProfile(user, Url.Action("Show", "Avatar", new { name = user.UserName }, Request.Scheme))
            };
            var invitedBy = await udb_.UserCodes.Include(u => u.User).FirstOrDefaultAsync(uc => uc.UsedBy == user.Id);
            if (invitedBy != null)
            {
                response.InvitedBy = Models.App.User.FromUserProfile(invitedBy.User, Url.Action("Show", "Avatar", new { name = invitedBy.User.UserName }, Request.Scheme));
            }
            return Json(response);
        }

        [HttpDelete]
        public async Task<ActionResult> InvitationCode([FromServices]Services.MessageUtil msgUtil, string code, string reason, bool notice = false)
        {
            if (!Guid.TryParse(code, out Guid guid))
            {
                return BadRequest(new { error = "Invalid code" });
            }
            var userCode = await udb_.UserCodes.SingleOrDefaultAsync(c => c.Code == guid);
            if (userCode == null)
            {
                return NotFound();
            }
            if (userCode.UsedBy.HasValue)
            {
                return BadRequest(new { error = "Already used code" });
            }
            udb_.AdminLogs.Add(new AdminLog
            {
                Action = "deletecode",
                Actor = User.Identity.Name,
                Target = userCode.User.UserName,
                Reason = (reason ?? "").Length > 100 ? reason.Substring(0, 100) : reason,
                LogTime = DateTime.Now
            });
            if (notice)
            {
                msgUtil.AddMsg(
                    User.Identity.Name,
                    userCode.User.UserName,
                    "邀请码删除通知",
                    string.Format("您的邀请码已被管理员删除<br>原因：<br>{0}", reason));
            }
            udb_.UserCodes.Remove(userCode);
            await udb_.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult> Category()
        {
            return Json(await db_.Categories.ToListAsync());
        }


        [HttpPost]
        public async Task<ActionResult> Category([FromBody] Models.Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                return BadRequest(new { err = "请输入栏目名称" });
            }
            if (category.ParentCategoryID.HasValue)
            {
                var parent = await db_.Categories.SingleOrDefaultAsync(cc => cc.CategoryID == category.ParentCategoryID);
                if (parent == null || parent.CategoryID == category.CategoryID)
                {
                    return BadRequest(new { err = "无效的父级栏目ID" });
                }
                category.ParentCategory = parent;
            }
            if (db_.Categories.Any(c => c.CategoryID == category.CategoryID))
            {
                db_.Entry(category).State = EntityState.Modified;
            }
            else
            {
                db_.Categories.Add(category);
            }
            await db_.SaveChangesAsync();
            cache_.Remove("~Categories");
            cache_.Remove("HomeHeader");
            return Json(new {id = category.CategoryID});
        }

        [HttpDelete]
        public async Task<ActionResult> Category(int id)
        {
            var cat = await db_.Categories.SingleOrDefaultAsync(cc => cc.CategoryID == id);
            if (cat == null)
            {
                return NotFound();
            }
            if (cat.ParentCategoryID.HasValue)
            {
                return BadRequest(new { err = "不可删除带有次级栏目的栏目" });
            }
            if (cat.Blogs.Any())
            {
                return BadRequest(new { err = "不可删除非空栏目" });
            }
            db_.Entry(cat).State = EntityState.Deleted;
            await db_.SaveChangesAsync();
            cache_.Remove("~Categories");
            cache_.Remove("HomeHeader");
            return Ok();
        }
    }
}