using System;
using System.Collections.Generic;

namespace GmGard.Models
{
    public class SettingsEventArgs : EventArgs
    {
        public AppSettingsModel AppModel { get; }
        public DataSettingsModel DataModel { get; }

        public SettingsEventArgs(DataSettingsModel m)
        {
            DataModel = m;
        }

        public SettingsEventArgs(AppSettingsModel m)
        {
            AppModel = m;
        }
    }

    public abstract class EventArgs<T> : EventArgs
    {
        public T Model { get; }

        public EventArgs(T t)
        {
            Model = t;
        }
    }

    public class BlogEventArgs : EventArgs<Blog>
    {
        public IEnumerable<Tag> Tags { get; }
        public bool Deleted { get; }

        public BlogEventArgs(Blog b) : base(b)
        {
        }

        public BlogEventArgs(Blog b, bool deleted) : base(b)
        {
            Deleted = deleted;
        }

        public BlogEventArgs(Blog b, IEnumerable<Tag> t) : base(b)
        {
            Tags = t;
        }
    }

    public class RateEventArgs : EventArgs<BlogRating>
    {
        public Microsoft.AspNetCore.Http.HttpContext Context { get; }

        public RateEventArgs(BlogRating r, Microsoft.AspNetCore.Http.HttpContext context) : base(r)
        {
            Context = context;
        }
    }

    public class PostEventArgs : EventArgs<Post>
    {
        public PostEventArgs(Post r) : base(r)
        {
        }
    }

    public class TopicEventArgs : EventArgs<Topic>
    {
        public TopicEventArgs(Topic t) : base(t)
        {
        }
    }

    public class ReplyEventArgs : EventArgs<Reply>
    {
        public ReplyEventArgs(Reply r) : base(r)
        {
        }
    }

    public class TagEventArgs : EventArgs<IEnumerable<Tag>>
    { 
        public Blog Blog { get; }

        public TagEventArgs(IEnumerable<Tag> t, Blog b) : base(t)
        {
            Blog = b;
        }
    }

    public class RatePostEventArgs : EventArgs<PostRating>
    {
        public RatePostEventArgs(PostRating r) : base(r)
        {
        }
    }
}