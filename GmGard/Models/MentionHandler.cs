using GmGard.Services;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace GmGard.Models
{
    /// <summary>
    /// Process mentions in user input contents and send notice to mentioned users.
    /// </summary>
    public class MentionHandler
    {
        private List<string> names = new List<string>();
        private UsersContext udb;

        public MentionHandler(UsersContext udb)
        {
            this.udb = udb;
        }

        /// <summary>
        /// Parse mention markup (a tag with data-mention), clear them and remember users that are mentioned here.
        /// </summary>
        /// <param name="content">Content to parse</param>
        /// <returns>Cleared content</returns>
        public string ParseMentions(string content)
        {
            var html = new HtmlDocument();
            html.LoadHtml(content);
            var title = string.Empty;
            var url = string.Empty;
            var nodes = html.DocumentNode.SelectNodes("//a/@data-mention");
            var parsedNodes = new Dictionary<string, List<HtmlNode>>(); 
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var name = HtmlEntity.DeEntitize(node.InnerText);
                    name = name.TrimStart(new char[] { '@', ' ' }).ToLower();
                    if (parsedNodes.ContainsKey(name))
                    {
                        parsedNodes[name].Add(node);
                    }
                    else
                    {
                        parsedNodes.Add(name, new List<HtmlNode> { node });
                    }
                }
            }
            var parsedNames = parsedNodes.Keys;
            var profiles = udb.Users.AsNoTracking().Where(u => parsedNames.Contains(u.NickName)).ToList();
            foreach (var profile in profiles)
            {
                foreach (var node in parsedNodes[profile.NickName.ToLower()]) {
                    node.SetAttributeValue("href", "/User/" + profile.UserName);
                    node.Attributes.Remove("data-mention");
                }
                names.Add(profile.UserName);
            }
            return html.DocumentNode.OuterHtml;
        }

        public bool HasMentions()
        {
            return names.Count > 0;
        }

        public void SendMentionMsg(MessageUtil util, string sender, string title, string url)
        {
            foreach (var name in names)
            {
                util.SendMentionNotice(name, sender, title, url);
            }
        }
    }
}