using GmGard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace GmGard.Filters
{
    public class HanMemberFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            if (filterContext.HttpContext.User.IsInRole("Administrator"))
                return;
            var db = filterContext.HttpContext.RequestServices.GetService<BlogContext>();
            var id = (int)filterContext.ActionArguments["id"];
            var memberExist = db.HanGroupMembers.Any(m => m.HanGroupID == id && m.Username.ToLower() == filterContext.HttpContext.User.Identity.Name.ToLower());
            if (memberExist)
            {
                return;
            }
            
            filterContext.Result = new UnauthorizedResult();
        }
    }
}