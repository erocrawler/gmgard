using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace GmGard.Models
{
    public class UserReply
    {
        public int ItemID { get; set; }
        public ItemType IdType { get; set; }
        public int ReplyID { get; set; }
        public bool IsPost { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime ReplyDate { get; set; }
        public string ItemTitle { get; set; }

        public static UserReply FromPost(Post p)
        {
            UserReply reply = new UserReply
            {
                ItemID = p.ItemId,
                IdType = p.IdType,
                ReplyDate = p.PostDate,
                ReplyID = p.PostId,
                IsPost = true,
                Author = p.Author,
                Content = p.Content,
            };
            return reply;
        }

        public static UserReply FromReply(Reply r)
        {
            UserReply reply = new UserReply
            {
                ItemID = r.post.ItemId,
                IdType = r.post.IdType,
                ReplyDate = r.ReplyDate,
                ReplyID = r.ReplyId,
                IsPost = false,
                Author = r.Author,
                Content = r.Content,
            };
            return reply;
        }
    }

    public class CarouselDisplay
    {
        public string Url { get; set; }
        public string BannerPath { get; set; }
        public string Title { get; set; }

        public static CarouselDisplay FromTopic(IUrlHelper url, Topic t)
        {
            return new CarouselDisplay { BannerPath = t.BannerPath, Title = t.TopicTitle, Url = url.Action("Details", "Topic", new { id = t.TopicID }) };
        }

        public static CarouselDisplay FromBlog(IUrlHelper url, Blog b)
        {
            return new CarouselDisplay { BannerPath = BlogHelper.firstImgPath(b), Title = b.BlogTitle, Url = url.Action("Details", "Blog", new { id = b.BlogID }) };
        }
    }

    public class RankTuple //name, value, ranking
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public long Ranking { get; set; }
    }

    public class UserRanking
    {
        public List<RankTuple> Exp;
        public Tuple<int, long> MyExp;
        public List<RankTuple> Blog;
        public Tuple<int, long> MyBlog;
        public List<RankTuple> Post;
        public Tuple<int, long> MyPost;
        public List<RankTuple> Sign;
        public Tuple<int, long> MySign;
        public DateTime rankdate;
    }

    public class UserInfoModel
    {
        public UserProfile Profile { get; set; }
        public int UserBlogs { get; set; }
        public int UserPosts { get; set; }
        public int UserFans { get; set; }
        public int UserFavorites { get; set; }
        public int UserFollows { get; set; }
        public SearchBlogResult SearchResult { get; set; }
        public string UserView { get; set; }
        public PagedList.IPagedList SubModel { get; set; }
        public IList<string> UserRoles { get; set; }
        public UserQuest.UserProfession UserTitle { get; set; }
        public string UserBackground { get; set; }
    }
}