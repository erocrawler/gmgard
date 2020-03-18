using MonoTorrent.Common;

namespace GmGard.Models
{
    public static class TorrentHelper
    {
        public static string GetMagnetURL(byte[] torrentFile)
        {
            Torrent t = null;
            if (Torrent.TryLoad(torrentFile, out t))
            {
                string[] announce = new string[2];
                for (int i = 0; i < 2 && i < t.AnnounceUrls.Count; i++)
                {
                    announce[i] = t.AnnounceUrls[i][0];
                }
                string magnet = "magnet:?xt=urn:btih:" + t.InfoHash.ToHex();
                if (!string.IsNullOrEmpty(announce[0]))
                {
                    magnet += "&tr.0=" + announce[0];
                    if (!string.IsNullOrEmpty(announce[1]))
                        magnet += "&tr.1=" + announce[1];
                }
                return magnet;
            }
            return null;
        }
    }
}