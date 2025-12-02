using Serilog;
using Serilog.Core;
using System;
using System.IO;

namespace GmGard.Models
{
    [Serializable]
    public class BlogException : Exception
    {
        public BlogException() : base("")
        {
        }

        public BlogException(string message) : base(message)
        {
        }

        public BlogException(string message, Exception inner) : base(message, inner)
        {
            Log.Error(inner, message);
        }
    }
}