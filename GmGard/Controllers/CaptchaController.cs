using Microsoft.AspNetCore.Mvc;
using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using System.IO;
using Microsoft.AspNetCore.Http;
using SixLabors.Shapes;

namespace GmGard.Controllers
{
    [ResponseCache(CacheProfileName = "Never")]
    public class CaptchaController : Controller
    {
        public ActionResult CaptchaImage(string prefix)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            //generate new question
            int a = rand.Next(10, 99);
            int b = rand.Next(0, 9);
            string captcha;
            int answer;
            if (rand.Next() % 2 == 0) {
                captcha = string.Format("{0} + {1} = ?", a, b);
                answer = a + b;
            } else {
                captcha = string.Format("{0} - {1} = ?", a, b);
                answer = a - b;
            }
            //store answer
            HttpContext.Session.SetInt32("Captcha" + prefix, answer);

            //image stream
            FileContentResult img = null;
            using (var mem = new MemoryStream())
            using (var image = new Image<Rgba32>(130, 30))
            {
                image.Mutate(ctx => {
                    ctx.Fill(Color.White);
                    int i, r, x, y;
                    float t;
                    for (i = 1; i < 20; i++)
                    {
                        var color = Color.FromRgba(
                        (byte)rand.Next(0, 255),
                        (byte)rand.Next(0, 255),
                        (byte)rand.Next(0, 255),
                        127);

                        r = rand.Next(1, (130 / 3));
                        x = rand.Next(0, 130);
                        y = rand.Next(0, 30);
                        t = (float)(rand.NextDouble() + 0.1) * 2;

                        var shape = new EllipsePolygon(x, y, r);
                        ctx.Draw(color, t, shape);
                    }

                    //add question
                    var font = SixLabors.Fonts.SystemFonts.CreateFont("Tahoma", 20);
                    ctx.DrawText(captcha, font, Color.Gray, new SixLabors.Primitives.PointF(rand.Next(4, 35), rand.Next(5, 8)));
                });


                //render as Jpeg
                image.SaveAsJpeg(mem);
                img = File(mem.GetBuffer(), "image/Jpeg");
            }

            return img;
        }
    }
}