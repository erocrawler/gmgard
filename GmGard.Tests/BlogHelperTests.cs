using GmGard.Models;
using Xunit;

namespace GmGard.Tests
{
    public class BlogHelperTests
    {
        // ---------- appendPassToContent ----------

        [Fact]
        public void appendPassToContent_encodes_script_in_name()
        {
            var output = BlogHelper.appendPassToContent("<p>body</p>", new[]
            {
                new BlogLink { name = "<script>alert(1)</script>", url = "abc123" }
            });

            Assert.DoesNotContain("<script", output);
            Assert.Contains("&lt;script&gt;", output);
        }

        [Fact]
        public void appendPassToContent_encodes_img_onerror_in_url()
        {
            var output = BlogHelper.appendPassToContent("<p>body</p>", new[]
            {
                new BlogLink { name = "密码", url = "<img src=x onerror=alert(1)>" }
            });

            Assert.DoesNotContain("<img", output);
            Assert.Contains("&lt;img", output);
        }

        [Fact]
        public void appendPassToContent_encodes_quotes_in_name_and_url()
        {
            var output = BlogHelper.appendPassToContent("", new[]
            {
                new BlogLink { name = "\"quoted\" 'name'", url = "\"x\" 'y'" }
            });

            // 属性值 / 文本里不能留下原始引号,否则可以逃逸出 HTML 上下文
            Assert.DoesNotContain("\"quoted\"", output);
            Assert.DoesNotContain("'name'", output);
            Assert.Contains("&quot;quoted&quot;", output);
            Assert.Contains("&#39;name&#39;", output);
        }

        [Fact]
        public void appendPassToContent_uses_default_label_when_name_empty()
        {
            var output = BlogHelper.appendPassToContent("", new[]
            {
                new BlogLink { name = "", url = "secret" },
                new BlogLink { name = null, url = "secret2" },
            });

            Assert.Equal(2, CountOccurrences(output, "密码："));
            Assert.Contains("secret", output);
            Assert.Contains("secret2", output);
        }

        [Fact]
        public void appendPassToContent_skips_entries_with_empty_url()
        {
            var output = BlogHelper.appendPassToContent("", new[]
            {
                new BlogLink { name = "keep", url = "real-pass" },
                new BlogLink { name = "drop", url = "" },
                new BlogLink { name = "drop2", url = "   " },
                new BlogLink { name = "drop3", url = null },
            });

            Assert.Contains("keep", output);
            Assert.Contains("real-pass", output);
            Assert.DoesNotContain("drop", output);
            Assert.Equal(1, CountOccurrences(output, "label label-inverse"));
        }

        [Fact]
        public void appendPassToContent_returns_content_unchanged_when_no_passwords()
        {
            Assert.Equal("<p>body</p>", BlogHelper.appendPassToContent("<p>body</p>", null));
            Assert.Equal("<p>body</p>", BlogHelper.appendPassToContent("<p>body</p>", new BlogLink[0]));
        }

        [Fact]
        public void appendPassToContent_keeps_original_content_prefix()
        {
            // content 本身是博客正文 HTML,由保存流程负责清洗,这里只负责追加,不做转义
            const string body = "<p>正文</p>";
            var output = BlogHelper.appendPassToContent(body, new[]
            {
                new BlogLink { name = "密码", url = "pw" }
            });

            Assert.StartsWith(body, output);
        }

        // ---------- checkBlogLinks ----------

        [Theory]
        [InlineData("javascript:alert(1)")]
        [InlineData("JavaScript:alert(1)")]
        [InlineData("JAVASCRIPT:alert(1)")]
        [InlineData("jAvAsCrIpT:alert(1)")]
        [InlineData("https://example.com/?q=javascript")]
        public void checkBlogLinks_rejects_javascript_in_any_case(string url)
        {
            var links = new[] { new BlogLink { name = "x", url = url } };

            Assert.False(BlogHelper.checkBlogLinks(links));
        }

        // 疑似绕过:校验只看原始字符串里是否含 "javascript" 子串。WHATWG URL 解析会去掉 URL 中的 TAB/换行,
        // 所以 "java\tscript:" 被浏览器当作 javascript: 协议执行。此测试期望被拒绝;若失败即为生产代码问题,保留不改。
        [Theory]
        [InlineData("java\tscript:alert(1)")]
        [InlineData("java\nscript:alert(1)")]
        [InlineData("java\rscript:alert(1)")]
        public void checkBlogLinks_rejects_javascript_scheme_split_by_control_characters(string url)
        {
            var links = new[] { new BlogLink { name = "x", url = url } };

            Assert.False(BlogHelper.checkBlogLinks(links));
        }

        [Fact]
        public void checkBlogLinks_rejects_when_any_one_link_is_javascript()
        {
            var links = new[]
            {
                new BlogLink { name = "ok", url = "https://example.com" },
                new BlogLink { name = "bad", url = "javascript:void(0)" },
            };

            Assert.False(BlogHelper.checkBlogLinks(links));
        }

        [Fact]
        public void checkBlogLinks_returns_false_for_null_or_empty_array()
        {
            Assert.False(BlogHelper.checkBlogLinks(null));
            Assert.False(BlogHelper.checkBlogLinks(new BlogLink[0]));
        }

        [Fact]
        public void checkBlogLinks_returns_false_when_url_is_empty()
        {
            var links = new[] { new BlogLink { name = "x", url = "" } };

            Assert.False(BlogHelper.checkBlogLinks(links));
        }

        [Theory]
        [InlineData("http://example.com")]
        [InlineData("https://example.com/path?x=1")]
        [InlineData("https://pan.example.org/s/abc")]
        public void checkBlogLinks_accepts_normal_http_and_https(string url)
        {
            var links = new[] { new BlogLink { name = "x", url = url } };

            Assert.True(BlogHelper.checkBlogLinks(links));
        }

        [Fact]
        public void checkBlogLinks_normalizes_url_through_UriBuilder()
        {
            // 源码行为:返回 true 的同时把 url 替换为 UriBuilder 规范化后的字符串(例如补全末尾斜杠)
            var links = new[] { new BlogLink { name = "x", url = "http://example.com" } };

            Assert.True(BlogHelper.checkBlogLinks(links));
            Assert.Equal("http://example.com/", links[0].url);
        }

        private static int CountOccurrences(string text, string needle)
        {
            int count = 0, index = 0;
            while ((index = text.IndexOf(needle, index, System.StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += needle.Length;
            }
            return count;
        }
    }
}
