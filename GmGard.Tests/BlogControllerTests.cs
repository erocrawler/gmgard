using System;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Controllers;
using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GmGard.Tests
{
    public class BlogControllerTests : IDisposable
    {
        private readonly TestApp _app = new TestApp();
        public void Dispose() => _app.Dispose();

        private BlogEdit MakeEdit(Blog b, string newTitle, string tags = "tag1")
        {
            var e = new BlogEdit(new[] { tags }, b);
            e.BlogTitle = newTitle;
            e.BlogTags = tags;
            e.BlogLinks = new[] { new BlogLink { name = "n", url = "http://example.com/a" } };
            e.Option = new BlogOption();
            return e;
        }

        private static string Errors(ControllerBase c) =>
            string.Join("; ", c.ModelState.SelectMany(kv => kv.Value.Errors.Select(e => kv.Key + ": " + e.ErrorMessage + e.Exception)));

        private async Task<ActionResult> PostEdit(BlogController c, int id, BlogEdit model)
        {
            var sanitizer = c.HttpContext.RequestServices.GetRequiredService<HtmlSanitizerService>();
            return await c.Edit(sanitizer, id, model, null);
        }

        [SkippableFact]
        public async Task Edit_post_by_non_author_is_forbidden_and_changes_nothing()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("owner"); _app.SeedUser("mallory");
            int cat = _app.SeedCategory();
            int id = _app.SeedBlog("owner", cat);
            var blog = _app.QueryData(db => db.Blogs.Single(b => b.BlogID == id));

            var c = _app.Controller<BlogController>("mallory");
            var result = await PostEdit(c, id, MakeEdit(blog, "被篡改"));

            Assert.IsType<ForbidResult>(result);
            Assert.Equal("原标题", _app.QueryData(db => db.Blogs.Single(b => b.BlogID == id).BlogTitle));
        }

        [SkippableFact]
        public async Task Edit_post_by_author_succeeds()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("owner");
            int cat = _app.SeedCategory();
            int id = _app.SeedBlog("owner", cat);
            var blog = _app.QueryData(db => db.Blogs.Single(b => b.BlogID == id));

            var c = _app.Controller<BlogController>("owner");
            var result = await PostEdit(c, id, MakeEdit(blog, "作者改的标题"));

            Assert.True(result is RedirectToActionResult, "实际:" + result.GetType().Name + " 错误:" + Errors(c));
            Assert.Equal("作者改的标题", _app.QueryData(db => db.Blogs.Single(b => b.BlogID == id).BlogTitle));
        }

        [SkippableFact]
        public async Task Edit_post_by_moderator_succeeds()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("owner"); _app.SeedUser("mod");
            int cat = _app.SeedCategory();
            int id = _app.SeedBlog("owner", cat);
            var blog = _app.QueryData(db => db.Blogs.Single(b => b.BlogID == id));

            var c = _app.Controller<BlogController>("mod", "Moderator");
            var result = await PostEdit(c, id, MakeEdit(blog, "版主改的标题"));

            Assert.True(result is RedirectToActionResult, "实际:" + result.GetType().Name + " 错误:" + Errors(c));
            Assert.Equal("版主改的标题", _app.QueryData(db => db.Blogs.Single(b => b.BlogID == id).BlogTitle));
        }

        [SkippableFact]
        public async Task Edit_post_unknown_blog_is_404()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("owner");
            int cat = _app.SeedCategory();
            int id = _app.SeedBlog("owner", cat);
            var blog = _app.QueryData(db => db.Blogs.Single(b => b.BlogID == id));

            var c = _app.Controller<BlogController>("owner");
            var result = await PostEdit(c, 999999, MakeEdit(blog, "x"));

            Assert.IsType<NotFoundResult>(result);
        }

        [SkippableFact]
        public async Task Edit_post_rejects_javascript_link_even_with_tab_inside()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("owner");
            int cat = _app.SeedCategory();
            int id = _app.SeedBlog("owner", cat);
            var blog = _app.QueryData(db => db.Blogs.Single(b => b.BlogID == id));
            var model = MakeEdit(blog, "新标题");
            model.BlogLinks = new[] { new BlogLink { name = "bad", url = "java\tscript:alert(1)" } };

            var c = _app.Controller<BlogController>("owner");
            var result = await PostEdit(c, id, model);

            Assert.IsType<ViewResult>(result);   // 校验失败:回到编辑页
            Assert.False(c.ModelState.IsValid);
            Assert.Equal("原标题", _app.QueryData(db => db.Blogs.Single(b => b.BlogID == id).BlogTitle));
        }

        [SkippableFact]
        public async Task Edit_post_in_link_optional_category_also_validates_supplied_links()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("owner");
            int cat = _app.SeedCategory(linkOptional: true);
            int id = _app.SeedBlog("owner", cat);
            var blog = _app.QueryData(db => db.Blogs.Single(b => b.BlogID == id));
            var model = MakeEdit(blog, "新标题");
            model.BlogLinks = new[] { new BlogLink { name = "bad", url = "javascript:alert(1)" } };

            var c = _app.Controller<BlogController>("owner");
            var result = await PostEdit(c, id, model);

            Assert.IsType<ViewResult>(result);
            Assert.Equal("原标题", _app.QueryData(db => db.Blogs.Single(b => b.BlogID == id).BlogTitle));
        }

        [SkippableFact]
        public void Patch_by_non_author_is_404()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("owner"); _app.SeedUser("mallory");
            int cat = _app.SeedCategory();
            int id = _app.SeedBlog("owner", cat);

            var c = _app.Controller<BlogController>("mallory");
            var result = c.Patch(id, new[] { new BlogLink { name = "密码", url = "123" } }, null);

            Assert.IsType<NotFoundResult>(result);
        }

        [SkippableFact]
        public void Patch_encodes_password_name_and_value_in_content()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("owner");
            int cat = _app.SeedCategory();
            int id = _app.SeedBlog("owner", cat);

            var c = _app.Controller<BlogController>("owner");
            var result = c.Patch(id, new[] { new BlogLink { name = "<img src=x onerror=alert(1)>", url = "<script>alert(2)</script>" } }, null);

            Assert.IsType<JsonResult>(result);
            string content = _app.QueryData(db => db.Blogs.Single(b => b.BlogID == id).Content);
            Assert.DoesNotContain("<img src=x", content);
            Assert.DoesNotContain("<script>alert(2)", content);
            Assert.Contains("&lt;script&gt;", content);
        }

        [SkippableFact]
        public void Patch_rejects_javascript_links()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("owner");
            int cat = _app.SeedCategory();
            int id = _app.SeedBlog("owner", cat, linksJson: "[]");

            var c = _app.Controller<BlogController>("owner");
            var result = c.Patch(id, null, new[] { new BlogLink { name = "x", url = "java\nscript:alert(1)" } });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.DoesNotContain("script", _app.QueryData(db => db.Blogs.Single(b => b.BlogID == id).Links ?? ""));
        }
    }
}
