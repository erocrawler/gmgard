using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GmGard.Models
{
    public class ImageHelper
    {

        public static void MoveFile(string path, string filename)
        {
            try
            {
                File.Move(path + filename, "C:/recycle/" + filename);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory("C:/recycle/");
                File.Move(path + filename, "C:/recycle/" + filename);
            }
            catch (IOException)
            {
                FluentScheduler.JobManager.AddJob(
                    () => File.Move(path + filename, "C:/recycle/" + filename),
                    (s) => s.ToRunOnceAt(DateTime.Now.AddHours(1))
                );
            }
        }
    }
}
