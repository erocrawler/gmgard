using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace GmGard.Services
{
    public class GmIdentityErrorDescriber : IdentityErrorDescriber
    {
        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password mismatch.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a password mismatch.</returns>
        public override IdentityError PasswordMismatch()
        {
            return new IdentityError
            {
                Code = nameof(PasswordMismatch),
                Description = "密码错误。"
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating an invalid token.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating an invalid token.</returns>
        public override IdentityError InvalidToken()
        {
            return new IdentityError
            {
                Code = nameof(InvalidToken),
                Description = "无效的Token。"
            };
        }

        ///// <summary>
        ///// Returns an <see cref="IdentityError"/> indicating an external login is already associated with an account.
        ///// </summary>
        ///// <returns>An <see cref="IdentityError"/> indicating an external login is already associated with an account.</returns>
        //public override IdentityError LoginAlreadyAssociated()
        //{
        //    return new IdentityError
        //    {
        //        Code = nameof(LoginAlreadyAssociated),
        //        Description = Resources.LoginAlreadyAssociated
        //    };
        //}

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating the specified user <paramref name="userName"/> is invalid.
        /// </summary>
        /// <param name="userName">The user name that is invalid.</param>
        /// <returns>An <see cref="IdentityError"/> indicating the specified user <paramref name="userName"/> is invalid.</returns>
        public override IdentityError InvalidUserName(string userName)
        {
            return new IdentityError
            {
                Code = nameof(InvalidUserName),
                Description = $"“{userName}”是无效的用户名。"
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating the specified <paramref name="email"/> is invalid.
        /// </summary>
        /// <param name="email">The email that is invalid.</param>
        /// <returns>An <see cref="IdentityError"/> indicating the specified <paramref name="email"/> is invalid.</returns>
        public override IdentityError InvalidEmail(string email)
        {
            return new IdentityError
            {
                Code = nameof(InvalidEmail),
                Description = $"“{email}”是无效的邮箱。"
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating the specified <paramref name="userName"/> already exists.
        /// </summary>
        /// <param name="userName">The user name that already exists.</param>
        /// <returns>An <see cref="IdentityError"/> indicating the specified <paramref name="userName"/> already exists.</returns>
        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateUserName),
                Description = $"用户名“{userName}”已被注册。"
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating the specified <paramref name="email"/> is already associated with an account.
        /// </summary>
        /// <param name="email">The email that is already associated with an account.</param>
        /// <returns>An <see cref="IdentityError"/> indicating the specified <paramref name="email"/> is already associated with an account.</returns>
        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateEmail),
                Description = $"邮箱“{email}”已被注册。"
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating the specified <paramref name="role"/> name is invalid.
        /// </summary>
        /// <param name="role">The invalid role.</param>
        /// <returns>An <see cref="IdentityError"/> indicating the specific role <paramref name="role"/> name is invalid.</returns>
        public override IdentityError InvalidRoleName(string role)
        {
            return new IdentityError
            {
                Code = nameof(InvalidRoleName),
                Description = $"“{role}”是无效的角色。"
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating the specified <paramref name="role"/> name already exists.
        /// </summary>
        /// <param name="role">The duplicate role.</param>
        /// <returns>An <see cref="IdentityError"/> indicating the specific role <paramref name="role"/> name already exists.</returns>
        public override IdentityError DuplicateRoleName(string role)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateRoleName),
                Description = $"角色“{role}”已存在。"
            };
        }

        ///// <summary>
        ///// Returns an <see cref="IdentityError"/> indicating a user already has a password.
        ///// </summary>
        ///// <returns>An <see cref="IdentityError"/> indicating a user already has a password.</returns>
        //public override IdentityError UserAlreadyHasPassword()
        //{
        //    return new IdentityError
        //    {
        //        Code = nameof(UserAlreadyHasPassword),
        //        Description = "User already has a password set."
        //    };
        //}

        ///// <summary>
        ///// Returns an <see cref="IdentityError"/> indicating user lockout is not enabled.
        ///// </summary>
        ///// <returns>An <see cref="IdentityError"/> indicating user lockout is not enabled..</returns>
        //public virtual IdentityError UserLockoutNotEnabled()
        //{
        //    return new IdentityError
        //    {
        //        Code = nameof(UserLockoutNotEnabled),
        //        Description = Resources.UserLockoutNotEnabled
        //    };
        //}

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a user is already in the specified <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The duplicate role.</param>
        /// <returns>An <see cref="IdentityError"/> indicating a user is already in the specified <paramref name="role"/>.</returns>
        public override IdentityError UserAlreadyInRole(string role)
        {
            return new IdentityError
            {
                Code = nameof(UserAlreadyInRole),
                Description = $"用户已是“{role}”的成员。"
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a user is not in the specified <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The duplicate role.</param>
        /// <returns>An <see cref="IdentityError"/> indicating a user is not in the specified <paramref name="role"/>.</returns>
        public override IdentityError UserNotInRole(string role)
        {
            return new IdentityError
            {
                Code = nameof(UserNotInRole),
                Description = $"用户不是“{role}”的成员。"
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password of the specified <paramref name="length"/> does not meet the minimum length requirements.
        /// </summary>
        /// <param name="length">The length that is not long enough.</param>
        /// <returns>An <see cref="IdentityError"/> indicating a password of the specified <paramref name="length"/> does not meet the minimum length requirements.</returns>
        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError
            {
                Code = nameof(PasswordTooShort),
                Description = $"密码长度至少需要{length}个字符。"
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password entered does not contain a non-alphanumeric character, which is required by the password policy.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a password entered does not contain a non-alphanumeric character.</returns>
        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = $"密码需要至少一个非字母或数字的字符。"
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password entered does not contain a numeric character, which is required by the password policy.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a password entered does not contain a numeric character.</returns>
        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresDigit),
                Description = $"密码需要至少一个数字字符。"
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password entered does not contain a lower case letter, which is required by the password policy.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a password entered does not contain a lower case letter.</returns>
        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresLower),
                Description = $"密码需要至少一个小写字符。"
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password entered does not contain an upper case letter, which is required by the password policy.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a password entered does not contain an upper case letter.</returns>
        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresUpper),
                Description = $"密码需要至少一个大写字符。"
            };
        }
    }
}
