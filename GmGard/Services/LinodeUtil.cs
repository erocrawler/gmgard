using GmGard.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace GmGard.Services
{
    public sealed class LinodeUtil : IUpload
    {
        private string pwd => appSettings.UploadSecret;
        public const string BucketName = "static.gmgard.us";
        public const string UploadUrl = "http://static.gmgard.us/fileUpload.php";
        public const string DeleteUrl = "http://static.gmgard.us/fileDelete.php";
        public const string GetUrl = "http://static.gmgard.us/";
        public const string UploadPath = "upload/";
        public const string ThumbsPath = "thumbs/";
        string IUpload.BucketName { get { return BucketName; } }

        private HttpClient client;
        private AppSettingsModel appSettings;

        public LinodeUtil(IOptions<AppSettingsModel> setting)
        {
            client = new HttpClient();
            appSettings = setting.Value;
        }

        public async Task<bool> PutObjectAsync(string filepath, Stream data)
        {
            var filename = System.IO.Path.GetFileName(filepath);
            filepath = System.IO.Path.GetDirectoryName(filepath);
            if (filepath.StartsWith("/"))
            {
                filepath = filepath.Substring(1);
            }
            HttpContent path = new StringContent(filepath);
            byte[] b;
            if (data.CanSeek)
                data.Position = 0;
            using (BinaryReader br = new BinaryReader(data))
            {
                b = br.ReadBytes((int)data.Length);
            }
            HttpContent fileStreamContent = new ByteArrayContent(b);
            HttpContent pass = new StringContent(pwd);
            var formData = new MultipartFormDataContent("---FenGeXian---");
            formData.Add(path, "path");
            formData.Add(fileStreamContent, "upload", filename);
            formData.Add(pass, "pwd");

            var response = await client.PostAsync(UploadUrl, formData);
            formData.Dispose();
            return response.IsSuccessStatusCode;
        }

        //public bool PutObject(string filepath, Stream data)
        //{
        //    var response = PutObjectAsync(filepath, data);
        //    response.Wait();
        //    return response.Result;
        //}

        public Task DeleteObjectsAsync(List<string> files)
        {
            if (files.Count == 0)
                return Task.FromResult(0);
            var i = 0;
            var formData = new MultipartFormDataContent("---FenGeXian---");
            HttpContent pass = new StringContent(pwd);
            foreach (var filename in files)
            {
                var name = Path.GetFileName(filename);
                var filepath = Path.GetDirectoryName(filename);
                HttpContent path = new StringContent(filepath);
                HttpContent fname = new StringContent(name);
                formData.Add(path, string.Format("path[{0}]", i));
                formData.Add(fname, string.Format("name[{0}]", i++));
            }
            formData.Add(pass, "pwd");
            return client.PostAsync(DeleteUrl, formData).ContinueWith(t => formData.Dispose());
        }

        public async Task<Stream> GetObjectAsync(string filename)
        {
            var resp = await client.GetAsync(GetUrl + filename);

            if (!resp.IsSuccessStatusCode)
                return null;
            return await resp.Content.ReadAsStreamAsync();
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}