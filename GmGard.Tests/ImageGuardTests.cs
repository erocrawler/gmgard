using System;
using System.IO;
using System.Threading.Tasks;
using GmGard.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace GmGard.Tests
{
    public class ImageGuardTests
    {
        private static byte[] Encode(Image image, IImageEncoder encoder)
        {
            using var ms = new MemoryStream();
            image.Save(ms, encoder);
            return ms.ToArray();
        }

        /// <summary>只允许顺序读取、不能 seek 的流,模拟网络上传流。</summary>
        private sealed class NonSeekableStream : Stream
        {
            private readonly MemoryStream _inner;

            public NonSeekableStream(byte[] data)
            {
                _inner = new MemoryStream(data);
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }

        private static Image<Rgba32> SolidRgba(int w, int h)
        {
            return new Image<Rgba32>(w, h, new Rgba32(10, 120, 200, 255));
        }

        // ---------- 接受 ----------

        [Fact]
        public void Load_Bytes_Accepts_Jpeg_800x600()
        {
            using var src = SolidRgba(800, 600);
            byte[] data = Encode(src, new JpegEncoder { Quality = 50 });

            using var img = ImageGuard.Load(data);

            Assert.Equal(800, img.Width);
            Assert.Equal(600, img.Height);
        }

        [Fact]
        public void Load_Bytes_Accepts_Png_3000x3000()
        {
            using var src = SolidRgba(3000, 3000);
            byte[] data = Encode(src, new PngEncoder { CompressionLevel = PngCompressionLevel.BestSpeed });

            using var img = ImageGuard.Load(data);

            Assert.Equal(3000, img.Width);
            Assert.Equal(3000, img.Height);
        }

        [Fact]
        public void Load_Bytes_Accepts_Animated_Gif_3_frames_200x200()
        {
            using var src = SolidRgba(200, 200);
            src.Frames.AddFrame(src.Frames.RootFrame);
            src.Frames.AddFrame(src.Frames.RootFrame);
            byte[] data = Encode(src, new GifEncoder());

            using var img = ImageGuard.Load(data);

            Assert.Equal(200, img.Width);
            Assert.Equal(200, img.Height);
            Assert.Equal(3, img.Frames.Count);
        }

        [Fact]
        public void Load_Bytes_Accepts_Bmp()
        {
            using var src = SolidRgba(64, 32);
            byte[] data = Encode(src, new BmpEncoder());

            using var img = ImageGuard.Load(data);

            Assert.Equal(64, img.Width);
            Assert.Equal(32, img.Height);
        }

        // ---------- 拒绝 ----------

        [Fact]
        public void Load_Bytes_Rejects_Png_over_40M_pixels()
        {
            // 9000 × 6000 = 54,000,000 像素,超过单帧 4000 万上限。L8 灰度图以控制编码体积。
            using var src = new Image<L8>(9000, 6000);
            byte[] data = Encode(src, new PngEncoder { CompressionLevel = PngCompressionLevel.BestSpeed });

            var ex = Record.Exception(() => ImageGuard.Load(data));

            Assert.IsType<ImageGuard.InvalidImageException>(ex);
        }

        [Fact]
        public void Load_Bytes_Rejects_Tiff_not_in_whitelist()
        {
            using var src = SolidRgba(16, 16);
            byte[] data = Encode(src, new TiffEncoder());

            var ex = Record.Exception(() => ImageGuard.Load(data));

            // TIFF 不在白名单里,ImageSharp 会按"未知格式"处理
            Assert.True(
                ex is UnknownImageFormatException || ex is ImageGuard.InvalidImageException,
                $"期望拒绝 TIFF,实际异常: {ex?.GetType().FullName ?? "无"}");
        }

        [Fact]
        public void Load_Bytes_Rejects_Gif_whose_total_pixels_exceed_100M()
        {
            // 注意:规格写的是 400 帧,但 500×500×400 = 100,000,000 恰好等于上限,而检查是严格大于,
            // 所以这里用 401 帧(100,250,000)确保真正超限。
            using var src = SolidRgba(500, 500);
            for (int i = 0; i < 400; i++)
            {
                src.Frames.AddFrame(src.Frames.RootFrame);
            }
            byte[] data = Encode(src, new GifEncoder());

            var ex = Record.Exception(() => ImageGuard.Load(data));

            var invalid = Assert.IsType<ImageGuard.InvalidImageException>(ex);
            Assert.Contains("动图", invalid.Message);
        }

        [Fact]
        public void Load_Bytes_Rejects_Non_image_bytes()
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes("this is definitely not an image, just some text payload");

            var ex = Record.Exception(() => ImageGuard.Load(data));

            Assert.True(
                ex is UnknownImageFormatException || ex is ImageGuard.InvalidImageException,
                $"期望拒绝非图片字节,实际异常: {ex?.GetType().FullName ?? "无"}");
        }

        // ---------- 不可 seek 的流 ----------

        [Fact]
        public void Load_NonSeekableStream_Works()
        {
            using var src = SolidRgba(200, 150);
            byte[] data = Encode(src, new PngEncoder());

            using var img = ImageGuard.Load(new NonSeekableStream(data));

            Assert.Equal(200, img.Width);
            Assert.Equal(150, img.Height);
        }

        [Fact]
        public async Task LoadAsync_NonSeekableStream_Works()
        {
            using var src = SolidRgba(120, 90);
            byte[] data = Encode(src, new BmpEncoder());

            using var img = await ImageGuard.LoadAsync(new NonSeekableStream(data));

            Assert.Equal(120, img.Width);
            Assert.Equal(90, img.Height);
        }

        [Fact]
        public async Task LoadAsync_NonSeekableStream_Rejects_Oversized()
        {
            using var src = new Image<L8>(9000, 6000);
            byte[] data = Encode(src, new PngEncoder { CompressionLevel = PngCompressionLevel.BestSpeed });

            var ex = await Record.ExceptionAsync(() => ImageGuard.LoadAsync(new NonSeekableStream(data)));

            Assert.IsType<ImageGuard.InvalidImageException>(ex);
        }

        // ---------- 常量 ----------

        [Fact]
        public void MaxFilesPerRequest_is_30()
        {
            Assert.Equal(30, ImageGuard.MaxFilesPerRequest);
        }
    }
}
