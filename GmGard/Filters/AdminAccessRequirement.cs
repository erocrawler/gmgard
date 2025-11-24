using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using GmGard.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace GmGard.Filters
{
    public class AdminAccessHandler : AuthorizationHandler<AdminAccessRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminAccessHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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
                var httpContext = _httpContextAccessor.HttpContext;
                var routeData = httpContext?.GetRouteData();
                
                if (routeData != null)
                {
                    var action = routeData.Values["Action"]?.ToString();
                    if (action == "Manage")
                    {
                        var manageContext = "Data";
                        if (routeData.Values.ContainsKey("context"))
                        {
                            manageContext = routeData.Values["context"]?.ToString();
                        }
                        if (context.User.IsInRole("Moderator") && (new string[] { "Data", "Users" }).Contains(manageContext))
                        {
                            succeed = true;
                        }
                    }
                    else if (context.User.IsInRole("Moderator") && (new string[] { "Log", "ManageRole", "ManageBan", "ManageExp" }).Contains(action))
                    {
                        succeed = true;
                    }
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