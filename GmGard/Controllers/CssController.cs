using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;

namespace GmGard.Controllers
{
    [ResponseCache(Duration = 604800, Location = ResponseCacheLocation.Any)]
    public class CssController : Controller
    {
        private BackgroundSetting _bgSetting;
        private ICompositeViewEngine _viewEngine;
        private IActionContextAccessor _actionAccessor;
        private readonly ConstantUtil _constantUtil;
        private readonly TitleService _titleService;

        public CssController(IOptionsSnapshot<BackgroundSetting> bgSetting, ICompositeViewEngine viewEngine, IActionContextAccessor actionAccessor, ConstantUtil constantUtil, TitleService titleService)
        {
            _bgSetting = bgSetting.Value;
            _viewEngine = viewEngine;
            _actionAccessor = actionAccessor;
            _constantUtil = constantUtil;
            _titleService = titleService;
        }

        //
        // GET: /Css/
        public async System.Threading.Tasks.Task<ActionResult> Index(string bg = "")
        {
            string host = _constantUtil.SiteStaticHost;
            if (bg != "" && !_bgSetting.BackgroundClasses.ContainsValue(bg))
            {
                return NotFound();
            }
            else if (_bgSetting.BackgroundClasses.ContainsValue(bg))
            {
                host = _bgSetting.BackgroundClasses.First(p => p.Value == bg).Key;
            }
            ViewBag.HostName = host;
            ViewBag.ClassName = bg;
            return await RenderCssAsync("bg", _bgSetting);
        }

        public async System.Threading.Tasks.Task<ActionResult> UserBg(string bg = "")
        {
            string host = _constantUtil.SiteStaticHost;
            if (bg != "" && !_bgSetting.BackgroundClasses.ContainsValue(bg))
            {
                return NotFound();
            }
            else if (_bgSetting.BackgroundClasses.ContainsValue(bg))
            {
                host = _bgSetting.BackgroundClasses.First(p => p.Value == bg).Key;
            }
            var bgs = _titleService.AllTitleBackgrounds;
            ViewBag.HostName = host;
            return await RenderCssAsync("UserBg", bgs);
        }

        protected async System.Threading.Tasks.Task<ContentResult> RenderCssAsync(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = _viewEngine.FindView(_actionAccessor.ActionContext, viewName, true);
                ViewContext viewContext = new ViewContext(_actionAccessor.ActionContext, viewResult.View, ViewData, TempData, sw, new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext);

                return Content(sw.ToString(), "text/css", System.Text.Encoding.UTF8);
            }
        }
    }
}