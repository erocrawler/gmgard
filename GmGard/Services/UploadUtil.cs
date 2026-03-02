using SixLabors.ImageSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;

namespace GmGard.Services
{
    public interface IUpload : IDisposable
    {
        Task<bool> PutObjectAsync(string filepath, string contentType, Stream data);

        Task DeleteObjectsAsync(List<string> filepath);

        Task<Stream> GetObjectAsync(string filepath);

        string BucketName { get; }
    }

    public class UploadUtil : IDisposable
    {
        private IUpload _client;

        public UploadUtil(IUpload client)
        {
            _client = client;
        }

        // Save Images and Thumbs!
        public async Task<List<string>> SaveImagesAsync(IEnumerable<IFormFile> BlogImage, bool savethumb = true)
        {
            var files = BlogImage.Where(f => f != null).ToList();

            // Process all images in parallel
            var tasks = files.Select((file, index) => ProcessImageAsync(file, index == 0 && savethumb));
            var results = await Task.WhenAll(tasks);

            var imgnames = results.Select(r => r.imgname).ToList();
            var imglist = imgnames.Select(n => "//" + _client.BucketName + "/" + n).ToList();

            if (results.Any(r => !r.success))
            {
                await _client.DeleteObjectsAsync(imgnames);
                return new List<string>();
            }

            return imglist;
        }

        private async Task<(string imgname, bool success)> ProcessImageAsync(IFormFile file, bool saveThumb)
        {
            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
            var imgname = GenerateImageName(filename);

            using var stream = file.OpenReadStream();
            using var img = await Image.LoadAsync(stream);

            img.Mutate(ctx => ctx.Resize(ImageUtil.GetMaxSize(img, 2400)));

            using var ms = new MemoryStream();
            await img.SaveAsJpegAsync(ms);
            ms.Position = 0;
            var mainUpload = _client.PutObjectAsync(imgname, "image/jpeg", ms);

            if (saveThumb)
            {
                img.Mutate(ctx => ctx.Resize(ImageUtil.GetMaxSize(img, 150)));
                using var nms = new MemoryStream();
                await img.SaveAsJpegAsync(nms);
                nms.Position = 0;
                var results = await Task.WhenAll(mainUpload, _client.PutObjectAsync(imgname.Replace("/upload/", "/thumbs/"), "image/jpeg", nms));
                return (imgname, results.All(r => r));
            }

            return (imgname, await mainUpload);
        }

        public string GenerateImageName(string filename = null)
        {
            int code = 0;
            if (string.IsNullOrEmpty(filename))
            {
                code = new Random().Next();
            }
            else
            {
                code = filename.GetHashCode();
            }
            return "Images/upload/" + Math.Abs(code).ToString().Substring(0, 3) + (new Random().Next(100)) + DateTime.Now.ToString("ddHHmmssffff") + ".jpg";
        }

        public async Task<bool> SaveImageAsync(byte[] image, string imgname)
        {
            using (var img = Image.Load(image))
            {
                img.Mutate(ctx => ctx.Resize(ImageUtil.GetMaxSize(img, 1200)));
                using (var ms = new MemoryStream())
                {
                    await img.SaveAsJpegAsync(ms);
                    ms.Position = 0; // Reset position for upload
                    return await _client.PutObjectAsync(imgname, "image/jpeg", ms);
                }
            }
        }

        public Task DeleteFilesAsync(IEnumerable<string> files)
        {
            List<string> filenames = new List<string>(files.Count());
            foreach (var file in files)
            {
                var pos = file.IndexOf(_client.BucketName);
                if (pos > 0)
                {
                    // remove //static.gmgard.us/
                    string filename = file.Substring(pos + _client.BucketName.Length + 1);
                    filenames.Add(filename);
                }
            }
            return _client.DeleteObjectsAsync(filenames);
        }

        public Task DeleteFileAsync(string file)
        {
            return DeleteFilesAsync(new[] { file });
        }

        public async Task SaveThumbAsync(string imgname)
        {
            int s = imgname.IndexOf(';');
            if (s >= 0)
            {
                imgname = imgname.Substring(0, s);
            }
            var pos = imgname.IndexOf(_client.BucketName);
            if (pos > 0)
            {
                // remove //static.gmgard.us/
                imgname = imgname.Substring(pos + _client.BucketName.Length + 1);
                var response = await _client.GetObjectAsync(imgname);
                if (response != null)
                {
                    using (var img = Image.Load(response))
                    {
                        img.Mutate(ctx => ctx.Resize(ImageUtil.GetMaxSize(img, 150)));
                        using (var nms = new MemoryStream())
                        {
                            await img.SaveAsJpegAsync(nms);
                            nms.Position = 0; // Reset position for upload
                            await _client.PutObjectAsync(imgname.Replace("/upload/", "/thumbs/"), "image/jpeg", nms);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client.Dispose();
            }
        }
    }
}