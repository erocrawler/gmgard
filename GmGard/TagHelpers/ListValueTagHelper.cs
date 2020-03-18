using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.TagHelpers
{
    [HtmlTargetElement(Attributes = "list-value-*")]
    public class ListValueTagHelper : TagHelper
    {
        private const string prefix = "list-value-";

        [HtmlAttributeName(DictionaryAttributePrefix = prefix)]
        public Dictionary<string, IEnumerable> list { get; set; } = new Dictionary<string, IEnumerable>();

        public override int Order => 1;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            foreach (var item in list)
            {
                output.Attributes.RemoveAll(item.Key);
                if (item.Value != null)
                {
                    output.Attributes.Add(item.Key, string.Join(",", item.Value.Cast<object>().ToArray()));
                }
                else
                {
                    output.Attributes.Add(item.Key, "");
                }
            }
        }
    }
}
