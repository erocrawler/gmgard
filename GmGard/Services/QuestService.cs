using GmGard.Controllers;
using GmGard.Extensions;
using GmGard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public class QuestService
    {
        public const int firstReplyExp = 3;
        public const int firstRateExp = 2;
        public const int firstBlogExp = 5;
        public const int weekBlogExp = 60;
        public const int dayBlogExp = 15;

        public const int dayBlogCount = 5;
        public const int weekBlogCount = 15;

        public const int EventBlogId = 98704;

        public static readonly string firstReplyNotice = string.Format("今天第一次评论，绅士度+{0}，棒棒糖+{0}", firstReplyExp);
        public static readonly string firstRateNotice = string.Format("今天第一次评分，绅士度+{0}，棒棒糖+{0}", firstRateExp);
        public static readonly string firstRateReplyNotice = string.Format("今天第一次评分与评论，绅士度+{0}，棒棒糖+{0}", firstReplyExp + firstRateExp);

        private readonly IMemoryCache _cache;
        private readonly AppSettingsModel _appSettings;

        public QuestService(IMemoryCache cache, IOptions<AppSettingsModel> appSettings)
        {
            _cache = cache;
            _appSettings = appSettings.Value;
            RegisterQuestEvents();
        }

        private void RegisterQuestEvents()
        {
            RatingUtil.OnRateBlog += OnRateBlog;
            ReplyController.OnAddPost += OnAddPost;
            Controllers.App.ReplyController.OnAddPost += OnAddPost;
            ReplyController.OnAddReply += OnAddReply;
            Controllers.App.ReplyController.OnAddPost += OnAddReply;
            ReplyController.OnRatePost += OnRatePost;
            Controllers.App.ReplyController.OnRatePost += OnRatePost;
            AuditController.OnApproveBlog += OnApprove;
            BlogController.OnNewBlog += OnApprove;
            BlogController.OnDeleteBlog += OnDelete;
            TopicController.OnNewTopic += OnApprove;
            TopicController.OnDeleteTopic += OnDelete;
        }

        private void OnRatePost(object sender, RatePostEventArgs e)
        {
            var context = sender as Controller;
            var post = e.Model;
            if (post == null || !(PostConstant.PostRatingEventActive && post.Post.ItemId == EventBlogId && post.Value == 1))
            {
                return;
            }
            var User = context.User;
            if (User.Identity.IsAuthenticated && _cache.Get<bool?>(ExpUtil.HasRatedPostCacheKey + User.Identity.Name.ToLower()) != true)
            {
                var expUtil = context.HttpContext.RequestServices.GetService<ExpUtil>();
                if (expUtil.SetRatePostDateAddExp(User.Identity.Name))
                {
                    _cache.Set(ExpUtil.HasRatedPostCacheKey + User.Identity.Name.ToLower(), true, DateTime.Today.AddDays(1));
                }
            }
        }

        private void OnAddPost(object sender, EventArgs e)
        {
            var context = sender as Controller;
            var Session = context.HttpContext.Session;
            var User = context.User;
            DateTime? lastpost = Session.GetDateTime("LastPostTime");
            if (!(lastpost.HasValue && (lastpost.Value.Date - DateTime.Today).Days == 0) || _cache.Get<bool?>(ExpUtil.HasPostedCacheKey + User.Identity.Name.ToLower()) != true) //no need to check db.
            {
                var expUtil = context.HttpContext.RequestServices.GetService<ExpUtil>();
                if (expUtil.setPostDateAddExp(User.Identity.Name))
                {
                    if (context.HttpContext.Items.ContainsKey("QuestMsg")) // should be first rate.
                    {
                        context.HttpContext.Items["QuestMsg"] = firstRateReplyNotice;
                    }
                    else
                    {
                        context.HttpContext.Items["QuestMsg"] = firstReplyNotice;
                    }
                    _cache.Set(ExpUtil.HasPostedCacheKey + User.Identity.Name.ToLower(), true, DateTime.Today.AddDays(1));
                }
            }
        }

        private void OnAddReply(object sender, EventArgs r)
        {
            var context = sender as Controller;
            var Session = context.HttpContext.Session;
            var User = context.User;
            DateTime? lastpost = Session.GetDateTime("LastPostTime");
            if (!(lastpost.HasValue && (lastpost.Value.Date - DateTime.Today).Days == 0) || _cache.Get<bool?>(ExpUtil.HasPostedCacheKey + User.Identity.Name.ToLower()) != true) //no need to check db.
            {
                var expUtil = context.HttpContext.RequestServices.GetService<ExpUtil>();
                if (expUtil.setPostDateAddExp(User.Identity.Name))
                {
                    context.HttpContext.Items["QuestMsg"] = firstReplyNotice;
                    _cache.Set(ExpUtil.HasPostedCacheKey + User.Identity.Name.ToLower(), true, DateTime.Today.AddDays(1));
                }
            }
        }

        private void OnApprove(object sender, EventArgs o)
        {
            var context = sender as Controller;
            string author = string.Empty;
            if (o is BlogEventArgs)
            {
                var b = ((BlogEventArgs)o).Model;
                if (b.isApproved != true)
                {
                    return;
                }
                author = b.Author;
            }
            else if (o is TopicEventArgs)
            {
                author = ((TopicEventArgs)o).Model.Author;
            }
            else
            {
                throw new ArgumentException("Unknown object", "o");
            }
            var db = context.HttpContext.RequestServices.GetService<UsersContext>();
            var expUtil = context.HttpContext.RequestServices.GetService<ExpUtil>();
            var user = db.Users.Include("quest").SingleOrDefault(u => u.UserName == author);
            expUtil.addExp(user, _appSettings.ExpAddOnPass);
            if (user.quest == null)
            {
                user.quest = new UserQuest();
            }

            if (user.quest.LastBlogDate.HasValue && DateTime.Today.isSameWeek(user.quest.LastBlogDate.Value))
            {
                if (user.quest.WeekBlogCount < weekBlogCount)
                {
                    user.quest.WeekBlogCount++;
                    if (user.quest.WeekBlogCount == weekBlogCount)
                    {
                        expUtil.addExp(user, weekBlogExp);
                    }
                }
            }
            else
            {
                user.quest.WeekBlogCount = 1;
            }
            _cache.Set(ExpUtil.WeekBloggedCacheKey + author.ToLower(), user.quest.WeekBlogCount, DateTime.Today.AddDays(1));
            if (user.quest.LastBlogDate.HasValue && user.quest.LastBlogDate.Value.Date == DateTime.Today)
            {
                if (user.quest.DayBlogCount < dayBlogCount)
                {
                    user.quest.DayBlogCount++;
                    if (user.quest.DayBlogCount == dayBlogCount)
                    {
                        expUtil.addExp(user, dayBlogExp);
                    }
                }
            }
            else
            {
                user.quest.DayBlogCount = 1;
                expUtil.addExp(user, firstBlogExp);
            }
            _cache.Set(ExpUtil.HasBloggedCacheKey + author.ToLower(), user.quest.DayBlogCount, DateTime.Today.AddDays(1));
            user.quest.LastBlogDate = DateTime.Today;
            db.SaveChanges();
        }

        private void OnDelete(object sender, EventArgs o)
        {
            var context = sender as Controller;
            string author = string.Empty;
            if (o is BlogEventArgs)
            {
                var b = ((BlogEventArgs)o).Model;
                if (b.isApproved != true)
                {
                    return;
                }
                author = b.Author;
            }
            else if (o is TopicEventArgs)
            {
                author = ((TopicEventArgs)o).Model.Author;
            }
            else
            {
                throw new ArgumentException("Unknown object", "o");
            }
            var db = context.HttpContext.RequestServices.GetService<UsersContext>();
            var expUtil = context.HttpContext.RequestServices.GetService<ExpUtil>();
            var user = db.Users.Include("quest").SingleOrDefault(u => u.UserName == author);
            expUtil.addExp(user, -_appSettings.ExpAddOnPass);
            if (user.quest == null || user.quest.LastBlogDate == null)
            {
                db.SaveChanges();
                return;
            }

            if (DateTime.Today.isSameWeek(user.quest.LastBlogDate.Value))
            {
                if (user.quest.WeekBlogCount < weekBlogCount && user.quest.WeekBlogCount > 0)
                {
                    user.quest.WeekBlogCount--;
                }
                if (user.quest.LastBlogDate.Value.Date == DateTime.Today)
                {
                    if (user.quest.DayBlogCount < dayBlogCount && user.quest.DayBlogCount > 0)
                    {
                        user.quest.DayBlogCount--;
                        if (user.quest.DayBlogCount == 0)
                        {
                            expUtil.addExp(user, -firstBlogExp);
                        }
                    }
                }
                db.SaveChanges();
                _cache.Set(ExpUtil.HasBloggedCacheKey + author.ToLower(), user.quest.DayBlogCount, DateTime.Today.AddDays(1));
                _cache.Set(ExpUtil.WeekBloggedCacheKey + author.ToLower(), user.quest.WeekBlogCount, DateTime.Today.AddDays(1));
            }
        }

        private void OnRateBlog(object sender, RateEventArgs r)
        {
            var User = r.Context.User;
            if (User.Identity.IsAuthenticated && _cache.Get<bool?>(ExpUtil.HasRatedCacheKey + User.Identity.Name.ToLower()) != true)
            {
                var expUtil = r.Context.RequestServices.GetService<ExpUtil>();
                if (expUtil.setRateDateAddExp(User.Identity.Name))
                {
                    r.Context.Items["QuestMsg"] = firstRateNotice;
                    _cache.Set(ExpUtil.HasRatedCacheKey + User.Identity.Name.ToLower(), true, DateTime.Today.AddDays(1));
                }
            }
        }
    }
}
