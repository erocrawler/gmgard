using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models
{
    public class LinodeS3Config
    {
        public string SecretKey { get; set; }
        public string AccessKey { get; set; }
        public string ServiceUrl { get; set; }
        public string BucketName { get; set; }
    }
}
