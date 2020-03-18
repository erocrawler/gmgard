using GmGard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System;

namespace GmGard.Filters
{
    public class RequireSslAttribute : TypeFilterAttribute
    {
        public RequireSslAttribute() : base(typeof(RequireSslImpl))
        {
        }

        private class RequireSslImpl : IActionFilter
        {
            private string port;

            public RequireSslImpl(IOptions<AppSettingsModel> settings)
            {
                port = settings.Value.HttpsPort;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }

            public void OnActionExecuting(ActionExecutingContext filterContext)
            {
                if (filterContext == null)
                {
                    throw new ArgumentNullException("filterContext");
                }

                if (!filterContext.HttpContext.Request.IsHttps)
                {
                    HandleNonHttpsRequest(filterContext);
                }
            }

            protected virtual void HandleNonHttpsRequest(ActionExecutingContext filterContext)
            {
                // only redirect for GET requests, otherwise the browser might not propagate the verb and request
                // body correctly.

                if (!string.Equals(filterContext.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // redirect to HTTPS version of page
                string url = "https://" + filterContext.HttpContext.Request.Host;
                if (!string.IsNullOrEmpty(port))
                {
                    url += ":" + port;
                }
                url += filterContext.HttpContext.Request.PathBase.Value;
                filterContext.Result = new RedirectResult(url);
            }
        }
    }
}