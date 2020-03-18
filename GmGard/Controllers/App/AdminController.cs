using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Models;
using GmGard.Models.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        public AdminController(UsersContext udb)
        {
            udb_ = udb;
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
    }
}