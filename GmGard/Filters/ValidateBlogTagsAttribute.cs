using GmGard.Services;
using System.ComponentModel.DataAnnotations;

namespace GmGard.Filters
{
    public class ValidateBlogTagsAttribute : ValidationAttribute
    {
        public ValidateBlogTagsAttribute(int MaxCount, int MaxLength = 20)
            : base("标签不得超过{0}个。")
        {
            this.MaxCount = MaxCount;
            this.MaxLength = MaxLength;
        }

        public int MaxCount { get; private set; }
        public int MaxLength { get; private set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                int ret = TagUtil.CheckBlogTag(value.ToString());
                if (ret != 0)
                {
                    var err = ret > 0 ? string.Format(this.ErrorMessage, MaxCount) : "标签不得超过" + MaxLength + "个字符";
                    return new ValidationResult(err);
                }
            }
            return null;
        }
    }
}