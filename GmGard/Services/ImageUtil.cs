using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.PlatformAbstractions;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;

namespace GmGard.Services
{
    public class ImageUtil : UtilityService
    {
        private IHostingEnvironment _env;

        public ImageUtil(BlogContext db, UsersContext udb, IMemoryCache cache, IHostingEnvironment env) : base(db, udb, cache)
        {
            _env = env;
        }

        public bool AddPic(string name, string type, MemoryStream data)
        {
            Pictures p = new Pictures();
            p.PicName = SaveAvatar(name, type, data, Path.Combine(_env.WebRootPath, "Images/Avatar/"));
            p.PicType = type;
            p.PicUserName = name;
            p.PicDate = DateTime.Now;
            _udb.Avatars.Add(p);
            _udb.SaveChanges();
            return true;
        }

        public bool AddPic(string name, string type, byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return AddPic(name, type, ms);
            }
        }

        public bool HasPic(string name)
        {
            if (name == null)
                return false;
            return _udb.Avatars.Any(pp => pp.PicName == name);
        }

        public string SaveAvatar(string name, string type, MemoryStream data, string path)
        {
            using (ImageFactory img = new ImageFactory())
            {
                img.Load(data);
                name += "." + img.CurrentImageFormat.DefaultExtension;
                img.Save(path + name);
            }
            return name;
        }

        public bool UpdateOrAddPic(string name, string type, byte[] data)
        {
            Pictures p = _udb.Avatars.SingleOrDefault(pp => pp.PicUserName == name);
            if (p == null)
            {
                AddPic(name, type, data);
                return true;
            }
            p.PicType = type;
            using (MemoryStream ms = new MemoryStream(data))
            {
                p.PicName = SaveAvatar(name, type, ms, Path.Combine(_env.WebRootPath, "Images/Avatar/"));
            }
            p.PicDate = DateTime.Now;
            _udb.SaveChanges();
            return true;
        }

        public ImageFactory GetThumb(ImageFactory img, int max)
        {
            int w = img.Image.Width;
            int h = img.Image.Height;
            if (w <= max && h <= max)
                return img;
            double ratio = 1;
            if (h > w)
            {
                ratio = (double)max / h;
            }
            else
                ratio = (double)max / w;
            return img.Resize(new Size((int)(w * ratio), (int)(h * ratio)));
        }

        public byte[] Crop(byte[] Img, int Width, int Height, int X, int Y)
        {
            using (var stream = new MemoryStream(Img))
            {
                return Crop(stream, Width, Height, X, Y);
            }
        }

        public byte[] Crop(MemoryStream stream, int Width, int Height, int X, int Y)
        {
            if (X < 0 || X > 500)
            {
                X = 0;
            }
            if (Y < 0 || Y > 500)
            {
                Y = 0;
            }
            if (X + Width >= 500)
                Width = 500 - X;
            if (Y + Height >= 500)
                Height = 500 - Y;
            using (var img = new ImageFactory())
            {
                img.Load(stream);
                if (img.Image.Height > 500 || img.Image.Width > 500)
                {
                    GetThumb(img, 500);
                }
                img.Crop(new CropLayer(X, Y, Width, Height, CropMode.Pixels));
                using (var outStream = new MemoryStream())
                {
                    img.Save(outStream);
                    return outStream.ToArray();
                }
            }
        }

        public void SaveJpeg(Stream s, ImageFactory source)
        {
            source.Format(new JpegFormat { Quality = 70 }).Save(s);
        }
    }
}