using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace GmGard.Services
{
    /// <summary>
    /// 解码用户上传图片前的统一防护:
    /// 1. 只允许 JPEG/PNG/GIF/WebP/BMP(不再解码 TIFF、TGA、QOI、PBM 等用不到、历史上漏洞最多的格式;
    ///    ImageSharp 3.1.12 的 TIFF 相关 advisory 因此不可达);
    ///    SkipMetadata 不解析 EXIF/ICC 等元数据(站里只用 DecodedImageFormat),绕开 ICC CLUT 解析那条 advisory;
    /// 2. 先只读头部(Identify)拿到宽高和帧数,超过像素上限直接拒绝,避免"几 KB 的压缩文件声明几万×几万像素"的解压炸弹;
    /// 3. 全局限制同时解码的图片数量,避免一个请求里放很多图并行吃光内存。
    /// </summary>
    public static class ImageGuard
    {
        /// <summary>单帧最大像素(约 8000×5000)。</summary>
        public const long MaxPixels = 40_000_000;

        /// <summary>所有帧像素总和上限(动图按每像素 4 字节估算,100M 约 400MB)。</summary>
        public const long MaxTotalPixels = 100_000_000;

        public const int MaxDimension = 12000;

        /// <summary>单次请求允许上传的图片文件数。线上最大的投稿有 26 张,最近一年没有超过 20 张的。</summary>
        public const int MaxFilesPerRequest = 30;

        /// <summary>全局同时处理(解码+缩放+编码)的图片数。</summary>
        public static readonly SemaphoreSlim Gate = new SemaphoreSlim(4, 4);

        private static readonly DecoderOptions Options = new DecoderOptions
        {
            SkipMetadata = true,
            Configuration = new Configuration(
                new JpegConfigurationModule(),
                new PngConfigurationModule(),
                new GifConfigurationModule(),
                new WebpConfigurationModule(),
                new BmpConfigurationModule()),
        };

        public class InvalidImageException : Exception
        {
            public InvalidImageException(string message) : base(message) { }
        }

        private static void Check(ImageInfo info)
        {
            if (info == null)
            {
                throw new InvalidImageException("无法识别的图片格式");
            }
            long pixels = (long)info.Width * info.Height;
            if (info.Width <= 0 || info.Height <= 0 || info.Width > MaxDimension || info.Height > MaxDimension || pixels > MaxPixels)
            {
                throw new InvalidImageException("图片尺寸过大");
            }
            int frames = Math.Max(1, info.FrameMetadataCollection.Count);
            if (pixels * frames > MaxTotalPixels)
            {
                throw new InvalidImageException("动图帧数或尺寸过大");
            }
        }

        public static Image Load(byte[] data)
        {
            Check(Image.Identify(Options, data));
            return Image.Load(Options, data);
        }

        public static Image Load(Stream stream)
        {
            stream = EnsureSeekable(stream);
            Check(Image.Identify(Options, stream));
            stream.Position = 0;
            return Image.Load(Options, stream);
        }

        public static async Task<Image> LoadAsync(Stream stream)
        {
            stream = EnsureSeekable(stream);
            Check(await Image.IdentifyAsync(Options, stream));
            stream.Position = 0;
            return await Image.LoadAsync(Options, stream);
        }

        private static Stream EnsureSeekable(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
                return stream;
            }
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
