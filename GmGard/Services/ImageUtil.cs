using GmGard.Models;
using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp.Processing;

namespace GmGard.Services
{
    public class ImageUtil : UtilityService
    {
        private IWebHostEnvironment _env;

        public ImageUtil(BlogContext db, UsersContext udb, IMemoryCache cache, IWebHostEnvironment env) : base(db, udb, cache)
        {
            _env = env;
        }

        public bool AddPic(string name, string type, MemoryStream data)
        {
            Pictures p = new Pictures();
            p.PicName = SaveAvatar(name, data, Path.Combine(_env.WebRootPath, "Images/Avatar/"));
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

        public string SaveAvatar(string name, MemoryStream data, string path)
        {
            using (var img = Image.Load(data, out var format))
            {
                name += "." + format.FileExtensions.First();
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
                p.PicName = SaveAvatar(name, ms, Path.Combine(_env.WebRootPath, "Images/Avatar/"));
            }
            p.PicDate = DateTime.Now;
            _udb.SaveChanges();
            return true;
        }
        public static SixLabors.Primitives.Size GetMaxSize(Image img, int max)
        {
            int w = img.Width;
            int h = img.Height;
            if (w <= max && h <= max)
                return new SixLabors.Primitives.Size(w, h);
            double ratio = 1;
            if (h > w)
            {
                ratio = (double)max / h;
            }
            else
                ratio = (double)max / w;
            return new SixLabors.Primitives.Size((int)(w * ratio), (int)(h * ratio));
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
            using (var img = Image.Load(stream, out var format))
            {
                img.Mutate(ctx =>
                {
                    if (img.Height > 500 || img.Width > 500)
                    {
                        ctx.Resize(GetMaxSize(img, 500));
                    }
                    ctx.Crop(new SixLabors.Primitives.Rectangle(X, Y, Width, Height));
                });
                using (var outStream = new MemoryStream())
                {
                    img.Save(outStream, format);
                    return outStream.ToArray();
                }
            }
        }
    }
}