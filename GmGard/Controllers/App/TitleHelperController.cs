using System;
using System.IO;
using System.Threading.Tasks;
using GmGard.Models.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/TitleHelper/[action]")]
    [EnableCors("GmAppOrigin")]
    [ApiController]
    public class TitleHelperController : AppControllerBase
    {
        private readonly IOptionsSnapshot<TitleCategoriesConfig> _config;
        private readonly IWebHostEnvironment _env;

        public TitleHelperController(IOptionsSnapshot<TitleCategoriesConfig> config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        // Public: get editable categories from App_Data/TitleCategories.json via IOptions
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Categories()
        {
            return Json(_config.Value.Categories ?? new System.Collections.Generic.List<TitleHelperCategory>());
        }
    }

    [Area("App")]
    [Produces("application/json")]
    [Route("api/Admin/TitleCategories/[action]")]
    [EnableCors("GmAppOrigin")]
    [ApiController]
    [Authorize(Roles = "Administrator,Moderator")]
    public class AdminTitleCategoriesController : AppControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public AdminTitleCategoriesController(IWebHostEnvironment env)
        {
            _env = env;
        }

        private string FilePath => Path.Combine(_env.ContentRootPath, "App_Data", "TitleCategories.json");

        [HttpGet]
        public async Task<IActionResult> List()
        {
            if (!System.IO.File.Exists(FilePath))
                return Json(new TitleCategoriesConfig());
            var json = await System.IO.File.ReadAllTextAsync(FilePath);
            var cfg = JsonConvert.DeserializeObject<TitleCategoriesConfig>(json) ?? new TitleCategoriesConfig();
            return Json(cfg);
        }

        // Save whole file - editable admin action
        [HttpPut]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] TitleCategoriesConfig config)
        {
            if (config == null) return BadRequest(new { error = "Invalid config" });
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            await System.IO.File.WriteAllTextAsync(FilePath, json);
            return Json(new { success = true, count = config.Categories?.Count ?? 0 });
        }

        [HttpPut]
        [HttpPost]
        public async Task<IActionResult> UpdateCategory([FromBody] TitleHelperCategory category)
        {
            if (category == null || category.Id == 0) return BadRequest(new { error = "Invalid category" });
            TitleCategoriesConfig cfg;
            if (System.IO.File.Exists(FilePath))
            {
                var existing = await System.IO.File.ReadAllTextAsync(FilePath);
                cfg = JsonConvert.DeserializeObject<TitleCategoriesConfig>(existing) ?? new TitleCategoriesConfig();
            }
            else cfg = new TitleCategoriesConfig();

            var idx = cfg.Categories.FindIndex(c => c.Id == category.Id);
            if (idx >= 0) cfg.Categories[idx] = category;
            else cfg.Categories.Add(category);

            var json = JsonConvert.SerializeObject(cfg, Formatting.Indented);
            await System.IO.File.WriteAllTextAsync(FilePath, json);
            return Json(category);
        }

        [HttpDelete]
        public async Task<IActionResult> Category(int id)
        {
            if (!System.IO.File.Exists(FilePath)) return NotFound();
            var existing = await System.IO.File.ReadAllTextAsync(FilePath);
            var cfg = JsonConvert.DeserializeObject<TitleCategoriesConfig>(existing) ?? new TitleCategoriesConfig();
            cfg.Categories.RemoveAll(c => c.Id == id);
            var json = JsonConvert.SerializeObject(cfg, Formatting.Indented);
            await System.IO.File.WriteAllTextAsync(FilePath, json);
            return Ok();
        }
    }
}
