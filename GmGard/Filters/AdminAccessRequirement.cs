using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using GmGard.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GmGard.Filters
{
    public class AdminAccessHandler : AuthorizationHandler<AdminAccessRequirement>
    {
        private readonly IActionContextAccessor _actionAccessor;
        private ActionContext ActionContext => _actionAccessor.ActionContext;
        private HttpContext HttpContext => _actionAccessor.ActionContext.HttpContext;

        public AdminAccessHandler(IActionContextAccessor actionAccessor)
        {
            _actionAccessor = actionAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminAccessRequirement requirement)
        {
            bool succeed = false;
            if(context.User.IsInRole("Administrator"))
            {
                succeed = true;
            }
            else if (context.User.Identity.IsAuthenticated)
            {
                var action = ActionContext.RouteData.Values["Action"].ToString();
                if (action == "Manage")
                {
                    var manageContext = "Data";
                    if (ActionContext.RouteData.Values.ContainsKey("context"))
                    {
                        manageContext = ActionContext.RouteData.Values["context"].ToString();
                    }
                    if (context.User.IsInRole("Moderator") && (new string[] { "Data", "Users" }).Contains(manageContext))
                    {
                        succeed = true;
                    }
                    else if (context.User.IsInRole("AdManager") && (new string[] { "Data", "AdManage" }).Contains(manageContext)) 
                    {
                        succeed = true;
                    }
                }
                else if (context.User.IsInRole("Moderator") && (new string[] { "Log", "ManageRole", "ManageBan", "ManageExp" }).Contains(action))
                {
                    succeed = true;
                }
                else if (context.User.IsInRole("AdManager") && (new string[] { "Log", "AdManage" }).Contains(action))
                {
                    succeed = true;
                }
            }
            if (succeed)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
            return Task.FromResult(0);
        }
    }

    public class AdminAccessRequirement : IAuthorizationRequirement {}
}