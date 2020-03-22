using System;

namespace AspNetCore.Identity.EntityFramework6
{
    public class IdentityUserToken : IdentityUserToken<int>
    {
    }

    public class IdentityUserToken<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Gets or sets the primary key of the user that the token belongs to.
        /// </summary>
        public virtual TKey UserId { get; set; }

        /// <summary>
        /// Gets or sets the LoginProvider this token is from.
        /// </summary>
        public virtual string LoginProvider { get; set; }

        /// <summary>
        /// Gets or sets the name of the token.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the token value.
        /// </summary>
        public virtual string Value { get; set; }
    }
}