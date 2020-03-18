using GmGard.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System;

namespace GmGard.Services
{
    public class WidgetUtil
    {
        private DataSettingsModel _dataSettings;
        private RatingUtil _ratingUtil;
        private MessageUtil _msgUtil;
        private IHttpContextAccessor _contextAccessor;

        private HttpContext Context => _contextAccessor.HttpContext;

        public WidgetUtil(IOptionsSnapshot<DataSettingsModel> dataSettings, RatingUtil ratingUtil, MessageUtil msgUtil, IHttpContextAccessor contextAccessor)
        {
            _dataSettings = dataSettings.Value;
            _ratingUtil = ratingUtil;
            _msgUtil = msgUtil;
            _contextAccessor = contextAccessor;
        }

        public string GetData()
        {
            var routedata = Context.GetRouteData().Values;
            var notice = _dataSettings.chuncaiNotice ?? "欢迎来到绅士之庭~";
            var defaultccs = "蛤蛤蛤蛤";
            var defaultface = 1;
            var action = routedata["action"].ToString();
            var controller = routedata["controller"].ToString();
            if (action.Equals("Details", StringComparison.OrdinalIgnoreCase) &&
                controller.Equals("Blog", StringComparison.OrdinalIgnoreCase)) //资源
            {
                var Rating = _ratingUtil.GetRating(routedata["id"].ToString());
                if (Rating.Count == 0)
                {
                    defaultccs = "还没有人评分，不评一个么少年";
                }
                else
                {
                    if (Rating.Average < 1)
                    {
                        defaultccs = "这啥玩意，瞎我一脸";
                        defaultface = 3;
                    }
                    else if (Rating.Average < 2)
                        defaultccs = "这。。。东西感觉一般啊";
                    else if (Rating.Average < 4)
                        defaultccs = "这个好像还不错";
                    else
                    {
                        defaultccs = "这个简直太棒了，我都石更了";
                        defaultface = 2;
                    }
                }
            }
            else if (action.Equals("List", StringComparison.OrdinalIgnoreCase))
            {
                defaultccs = "有木有找到好资源？";
            }
            else if (action.Equals("Create", StringComparison.OrdinalIgnoreCase))
            {
                defaultccs = "要投稿么？记得给图片打码哦~~~~度娘直接全部复制到“链接地址”栏即可！";
                defaultface = 2;
            }
            else if (action.Equals("Register", StringComparison.OrdinalIgnoreCase))
            {
                defaultccs = "欢迎注册~~~";
                defaultface = 2;
            }
            else if (action.Equals("Login", StringComparison.OrdinalIgnoreCase))
            {
                defaultccs = "欢迎主人登录~~~";
                defaultface = 2;
            }
            else if (action.Equals("Index", StringComparison.OrdinalIgnoreCase) && controller.Equals("Home", StringComparison.OrdinalIgnoreCase))
                defaultccs = notice.ToString();
            else if (controller.Equals("Message", StringComparison.OrdinalIgnoreCase))
            {
                var unread = _msgUtil.GetUnreadMsg(Context.User.Identity.Name);
                if (unread != null)
                    defaultccs = "你有" + unread + "条消息未读哦~";
            }
            else if (action.Equals("Suggestions", StringComparison.OrdinalIgnoreCase))
            {
                defaultccs = "欢迎主人对本站提任何建议~";
            }
            else if (action.Equals("Donate", StringComparison.OrdinalIgnoreCase))
            {
                defaultccs = "土豪我们交个朋友吧~~";
                defaultface = 4;
            }
            var lifetime = DateTime.Now - new DateTime(2013, 11, 23, 19, 0, 0);
            var showlifetime = string.Format("我已经与主人  一起生存了 <font color='red'>{0}</font> 天 <font color='red'>{1}</font> 小时 <font color='red'>{2}</font> 分钟 <font color='red'>{3}</font> 秒的快乐时光啦～*^_^", lifetime.Days, lifetime.Hours, lifetime.Minutes, lifetime.Seconds);
            return Newtonsoft.Json.JsonConvert.SerializeObject(new { notice = notice, defaultccs = defaultccs, defaultface = defaultface, showlifetime = showlifetime });
        }
    }
}