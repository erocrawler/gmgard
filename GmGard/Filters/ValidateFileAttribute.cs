using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GmGard.Filters
{
    public class ValidateFileAttribute : RequiredAttribute
    {
        private int filesize;

        public ValidateFileAttribute(int filesize)
        {
            this.filesize = filesize;
        }

        public override bool IsValid(object value)
        {
            var file = value as IFormFile;
            if (file == null)
                return true;
            else if (file.Length > filesize || !file.ContentType.Contains("image"))
            {
                return false;
            }

            return true;
        }
    }
}