namespace GmGard.Client.Components
{
    public class BufferedImage
    {
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "image/jpeg";
        public byte[] Data { get; set; } = System.Array.Empty<byte>();
        public long Size => Data.LongLength;
    }
}
