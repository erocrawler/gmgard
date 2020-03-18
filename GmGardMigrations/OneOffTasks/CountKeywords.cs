using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GmGardMigrations.OneOffTasks
{
    class CountKeywords
    {
        const int BATCH_SIZE = 10000;

        private static bool BalancedParanthesis(string str, char left, char right)
        {
            var items = new Stack<int>(str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c == left)
                    items.Push(i);
                else if (c == right)
                {
                    if (items.Count == 0)
                    {
                        return false;
                    }
                    items.Pop();
                }
            }
            if (items.Count > 0)
            {
                return false;
            }
            return true;
        }

        // Ignore dates
        private static Regex dateSizeRegex = new Regex("^[0-9.-]+[gGmMbB]{0,2}$");

        public static void Run()
        {
            var regex1 = new Regex(@"
                \(                    # Match (
                (
                    [^()]+            # all chars except ()
                    | (?<Level>\()    # or if ( then Level += 1
                    | (?<-Level>\))   # or if ) then Level -= 1
                )+                    # Repeat (to go from inside to outside)
                (?(Level)(?!))        # zero-width negative lookahead assertion
                \)                    # Match )",
                RegexOptions.IgnorePatternWhitespace);
            var regex2 = new Regex(@"
                \[                    # Match [
                (
                    [^\[\]]+          # all chars except []
                    | (?<Level>\[)    # or if [ then Level += 1
                    | (?<-Level>\])   # or if ] then Level -= 1
                )+                    # Repeat (to go from inside to outside)
                (?(Level)(?!))        # zero-width negative lookahead assertion
                \]                    # Match ]",
                RegexOptions.IgnorePatternWhitespace);

            var keywordDict = new Dictionary<string, int>();
            BlogContextFactory blogContextFactory = new BlogContextFactory();
            using (var db = blogContextFactory.Create())
            {
                var blogCount = db.Blogs.Count();
                for (int i = 0; i < blogCount; i += BATCH_SIZE)
                {
                    var titles = db.Blogs.OrderBy(b => b.BlogID).Skip(i).Take(BATCH_SIZE).Select(t => t.BlogTitle).ToList();
                    foreach (var title in titles)
                    {
                        if (BalancedParanthesis(title, '(', ')'))
                        {
                            var matches = regex1.Matches(title);
                            foreach (Match m in matches)
                            {
                                var key = m.Value.Substring(1, m.Value.Length - 2);
                                if (dateSizeRegex.IsMatch(key))
                                {
                                    continue;
                                }
                                if (keywordDict.ContainsKey(key))
                                {
                                    keywordDict[key] = keywordDict[key] + 1;
                                }
                                else
                                {
                                    keywordDict[key] = 1;
                                }
                            }
                        }
                        if (BalancedParanthesis(title, '[', ']'))
                        {
                            var matches = regex2.Matches(title);
                            foreach (Match m in matches)
                            {
                                var key = m.Value.Substring(1, m.Value.Length - 2);
                                if (dateSizeRegex.IsMatch(key))
                                {
                                    continue;
                                }
                                if (keywordDict.ContainsKey(key))
                                {
                                    keywordDict[key] = keywordDict[key] + 1;
                                }
                                else
                                {
                                    keywordDict[key] = 1;
                                }
                            }
                        }
                    }

                }
            }
            File.WriteAllLines("output.txt", keywordDict.OrderByDescending(kvp => kvp.Value).Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        }
    }
}
