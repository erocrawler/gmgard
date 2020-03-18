using GmGard.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace GmGard.Filters
{
    public class ValidateCaptchaAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var util = filterContext.HttpContext.RequestServices.GetService<BlogUtil>();
            string captcha, prefix;
            if (request.HasFormContentType)
            {
                captcha = request.Form["Captcha"];
                prefix = request.Form["Prefix"];
            }
            else
            {
                captcha = request.Query["Captcha"];
                prefix = request.Query["Prefix"];
            }
            if (util.CheckCaptchaError(captcha, prefix))
            {
                filterContext.ModelState.AddModelError("Captcha", "验证码计算错误，请重试。");
            }
            base.OnActionExecuting(filterContext);
        }
    }
}