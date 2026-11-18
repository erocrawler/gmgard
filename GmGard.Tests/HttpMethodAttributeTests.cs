using System;
using System.Linq;
using System.Reflection;
using GmGard.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GmGard.Tests
{
    /// <summary>
    /// 回归:这些会改变状态的 action 必须是 POST(防止 GET 触发 CSRF / 误链接触发)。
    /// 用反射直接检查 attribute,不需要起 HTTP 服务。
    /// </summary>
    public class HttpMethodAttributeTests
    {
        private static MethodInfo GetAction(Type controller, string name, int parameterCount)
        {
            var candidates = controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == name && m.GetParameters().Length == parameterCount)
                .ToArray();

            Assert.Single(candidates);
            return candidates[0];
        }

        [Theory]
        [InlineData(typeof(MessageController), "Report", 5)]
        [InlineData(typeof(MessageController), "ReportAction", 3)]
        [InlineData(typeof(ReplyController), "RatePost", 2)]
        [InlineData(typeof(BlogController), "Patch", 3)]
        public void State_changing_action_requires_HttpPost(Type controller, string name, int parameterCount)
        {
            var method = GetAction(controller, name, parameterCount);

            Assert.NotNull(method.GetCustomAttribute<HttpPostAttribute>(inherit: true));
        }

        [Fact]
        public void MsgAction_is_not_restricted_to_POST()
        {
            // MsgAction 故意同时允许 GET(用于读取消息/展示),只有 act=delete 时在方法体里要求 POST。
            var method = GetAction(typeof(MessageController), "MsgAction", 2);

            Assert.Null(method.GetCustomAttribute<HttpPostAttribute>(inherit: true));
        }
    }
}
