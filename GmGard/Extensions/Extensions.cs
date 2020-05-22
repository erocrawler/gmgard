using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace GmGard.Extensions
{
    public static class Extensions
    {
        public static bool isSameWeek(this DateTime day1, DateTime day2)
        {
            int week1 = (int)day1.DayOfWeek;
            int week2 = (int)day2.DayOfWeek;
            week1 = week1 == 0 ? 7 : week1;
            week2 = week2 == 0 ? 7 : week2;
            return day1.AddDays(7 - week1).Equals(day2.AddDays(7 - week2));
        }

        public static string KiloFormat(this int num, bool kilo = false)
        {
            return KiloFormat((long)num, kilo);
        }

        public static string KiloFormat(this long num, bool kilo = false)
        {
            if (num >= 100000000)
            {
                return (num / 1000000D).ToString("0.#M");
            }
            if (num >= 1000000)
            {
                return (num / 1000000D).ToString("0.#M");
            }
            if (num >= 100000)
            {
                return (num / 1000D).ToString("0.#K");
            }
            if (num >= 10000 || (num >= 1000 && kilo))
            {
                return (num / 1000D).ToString("0.#K");
            }
            return num.ToString();
        }

        /// <summary>
        /// Determines whether the specified HTTP request is an AJAX request.
        /// </summary>
        /// 
        /// <returns>
        /// true if the specified HTTP request is an AJAX request; otherwise, false.
        /// </returns>
        /// <param name="request">The HTTP request.</param><exception cref="T:System.ArgumentNullException">The <paramref name="request"/> parameter is null (Nothing in Visual Basic).</exception>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return false;
        }

        public static void SetInt64(this ISession session, string key, long value)
        {
            var bytes = BitConverter.GetBytes(value);
            session.Set(key, bytes);
        }

        public static long? GetInt64(this ISession session, string key)
        {
            var data = session.Get(key);
            if (data == null || data.Length < 8)
            {
                return null;
            }
            return BitConverter.ToInt64(data, 0);
        }

        public static void SetDateTime(this ISession session, string key, DateTime value)
        {
            var bytes = BitConverter.GetBytes(value.Ticks);
            session.Set(key, bytes);
        }

        public static DateTime? GetDateTime(this ISession session, string key)
        {
            var data = session.GetInt64(key);
            if (data == null)
            {
                return null;
            }
            return new DateTime(data.Value);
        }



        public static BlogOption OverrideOption(this BlogOption self, BlogUtil util)
        {
            self.LockTags = self.LockTags && util.CheckAdmin(true); // Admin and writer
            self.NoRate = self.NoRate && util.CheckAdmin(); // Only admin
            self.NoComment = self.NoComment && util.CheckAdmin(); // Only admin
            self.NoApprove = self.NoApprove && util.CheckAdmin();
            return self;
        }

        public static BlogOption MergeWith(this BlogOption self, BlogUtil util, BlogOption modified)
        {
            if (modified == null)
            {
                return self;
            }
            if (util.CheckAdmin(true, true))
            {
                self.LockTags = modified.LockTags;
            }
            if (util.CheckAdmin(includeAdManager: true))
            {
                self.NoRate = modified.NoRate;
                self.NoApprove = modified.NoApprove;
            }
            self.LockDesc = modified.LockDesc;
            return self;
        }

        public static string Encode(this IJsonHelper json, object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        private static readonly Dictionary<char, char> FullWidthMap = new Dictionary<char, char> {
            { (char)12288, (char)32 }, // Full width space
            { '【', '[' },
            { '】', ']' },
        };

        public static string ToSingleByteCharacterString(this string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (FullWidthMap.ContainsKey(c[i]))
                {
                    c[i] = FullWidthMap[c[i]];
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }
    }
}