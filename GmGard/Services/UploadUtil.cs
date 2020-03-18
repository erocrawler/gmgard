using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public interface IUpload : IDisposable
    {
        Task<bool> PutObjectAsync(string filepath, Stream data);

//        bool PutObject(string filepath, Stream data);

        Task DeleteObjectsAsync(List<string> filepath);

        Task<Stream> GetObjectAsync(string filepath);

        string BucketName { get; }
    }

    public class UploadUtil : IDisposable
    {
        private IUpload _client;
        private ImageUtil _imageUtil;
        public const string BucketName = LinodeUtil.BucketName;

        public UploadUtil(IUpload client, ImageUtil imageUtil)
        {
            _client = client;
            _imageUtil = imageUtil;
        }

        // Save Images and Thumbs!
        public async Task<List<string>> SaveImagesAsync(IEnumerable<IFormFile> BlogImage, bool savethumb = true)
        {
            string imgname = string.Empty;
            var imglist = new List<string>();
            var tasks = new List<Task<bool>>();
            var tmplist = new List<string>();
            foreach (var file in BlogImage)
            {
                if (file != null)
                {
                    var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString();
                    filename = filename.Trim('"');// FileName returns "fileName.ext"(with double quotes)

                    imgname = GenerateImageName(filename);
                    using (var stream = file.OpenReadStream())
                    {
                        using (var img = new ImageFactory())
                        {
                            img.Load(stream);
                            img.Format(new JpegFormat { Quality = 70 });
                            _imageUtil.GetThumb(img, 1200);
                            var ms = new MemoryStream();
                            img.Save(ms);
                            tasks.Add(_client.PutObjectAsync(imgname, ms).ContinueWith(t => { ms.Dispose(); return t.Result; }));
                            imglist.Add("//" + _client.BucketName + "/" + imgname);
                            if (tmplist.Count == 0 && savethumb)
                            {
                                _imageUtil.GetThumb(img, 100);
                                var nms = new MemoryStream();
                                img.Save(nms);
                                tasks.Add(_client.PutObjectAsync(imgname.Replace("/upload/", "/thumbs/"), nms)
                                    .ContinueWith(t => { nms.Dispose(); return t.Result; }));
                            }
                        }
                    }
                    tmplist.Add(imgname);
                }
            }
            await Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                if (!task.IsCompleted || !task.Result)
                {
                    await _client.DeleteObjectsAsync(tmplist);
                    imglist.RemoveRange(0, imglist.Count);
                    break;
                }
            }
            return imglist;
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

        public async Task<bool> saveImageAsync(byte[] image, string imgname)
        {
            using (var img = new ImageFactory())
            {
                img.Load(image);
                _imageUtil.GetThumb(img, 1200);
                using (var ms = new MemoryStream())
                {
                    _imageUtil.SaveJpeg(ms, img);
                    return await _client.PutObjectAsync(imgname, ms);
                }
            }
        }

        public Task DeleteFilesAsync(IEnumerable<string> files)
        {
            List<string> filenames = new List<string>(files.Count());
            foreach (var file in files)
            {
                var filename = file;
                var pos = file.IndexOf(_client.BucketName);
                if (pos > 0)
                {
                    // remove //static.gmgard.us/
                    filename = file.Substring(pos + _client.BucketName.Length + 1);
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
                    using (var img = new ImageFactory())
                    {
                        img.Load(response);
                        _imageUtil.GetThumb(img, 100);
                        using (var nms = new MemoryStream())
                        {
                            _imageUtil.SaveJpeg(nms, img);
                            await _client.PutObjectAsync(imgname.Replace("/upload/", "/thumbs/"), nms);
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