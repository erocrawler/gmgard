using Microsoft.AspNetCore.Mvc;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using Microsoft.AspNetCore.Http;

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
            using (var bmp = new Bitmap(130, 30))
            using (var gfx = Graphics.FromImage(bmp))
            {
                gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));
                
                //add noise
                int i, r, x, y;
                var pen = new Pen(Color.Yellow);
                for (i = 1; i < 10; i++)
                {
                    pen.Color = Color.FromArgb(
                    (rand.Next(0, 255)),
                    (rand.Next(0, 255)),
                    (rand.Next(0, 255)));

                    r = rand.Next(0, (130 / 3));
                    x = rand.Next(0, 130);
                    y = rand.Next(0, 30);

                    gfx.DrawEllipse(pen, x - r, y - r, r, r);
                }

                //add question
                gfx.DrawString(captcha, new Font("Tahoma", 15), Brushes.Gray, 2, 3);

                //render as Jpeg
                bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
                img = File(mem.GetBuffer(), "image/Jpeg");
            }

            return img;
        }
    }
}