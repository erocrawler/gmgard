using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using GmGard.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public class LinodeS3 : IUpload
    {
        public string BucketName => $"{bucketName}.{serviceUrl}";

        private AmazonS3Client client { get; set; }
        private string bucketName { get; set; }
        private string serviceUrl { get; set; }

        public LinodeS3(IOptions<LinodeS3Config> config)
        {
            var c = new AmazonS3Config
            {
                ServiceURL = "https://" + config.Value.ServiceUrl,
            };
            client = new AmazonS3Client(config.Value.AccessKey, config.Value.SecretKey, c);
            serviceUrl = config.Value.ServiceUrl;
            bucketName = config.Value.BucketName;
        }

        public Task DeleteObjectsAsync(List<string> filepath)
        {
            return client.DeleteObjectsAsync(new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = filepath.Select(f => new KeyVersion { Key = f }).ToList(),
            });
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public async Task<Stream> GetObjectAsync(string filepath)
        {
            var resp = await client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = bucketName,
                Key = filepath,
            });
            return resp.ResponseStream;
        }

        public async Task<bool> PutObjectAsync(string filepath, string contentType, Stream data)
        {
            var resp = await client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucketName,
                Key = filepath,
                InputStream = data,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead,
            });
            return resp.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
