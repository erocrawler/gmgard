using GmGard.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GmGard.Services
{
    /// <summary>
    /// Custom UserManager to handle UserId = 0 edge case
    /// </summary>
    public class UserManager : UserManager<UserProfile>
    {
        public UserManager(
            IUserStore<UserProfile> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<UserProfile> passwordHasher,
            IEnumerable<IUserValidator<UserProfile>> userValidators,
            IEnumerable<IPasswordValidator<UserProfile>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<UserProfile>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override Task<string> GetUserIdAsync(UserProfile user)
        {
            // Fix: Always return user.Id.ToString() to handle UserId = 0
            return Task.FromResult(user.Id.ToString());
        }
    }
}
