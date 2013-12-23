using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace MediaLib
{
    public delegate bool SearchAction(ref MediaInfo info, string path);

    public interface IMediaStorage
    {
        string Name { get; set; }
        string MainMediaPath { get; set; }
        MediaTree MediaTree { get; set; }

        void FindMainMediaPath();
        void SearchPaths(List<string> paths, SearchAction action);
        void CopyToPC(MediaInfo mediaToCopy);
    }

    public static class MediaStorageHelper
    {
        public static void BuildMediaTree(this IMediaStorage media)
        {
            List<string> paths = new List<string>();
            paths.Add(media.MainMediaPath);
            media.SearchPaths(paths, media.BuildTree);
        }

        public static bool BuildTree(this IMediaStorage media, ref MediaInfo info, string path)
        {
            bool endSearch = false;

            info.CurrentPath = path;
            media.MediaTree.Add(info);

            return endSearch;
        }

        public static void TransferToPC(this IMediaStorage media)
        {
            foreach (MediaInfo dir in media.MediaTree)
            {
                Directory.CreateDirectory(dir.DestinationPath);
                foreach (MediaInfo file in dir.Files)
                {
                    if (!File.Exists(file.FullName))
                        media.CopyToPC(file);
                }
            }

            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.AddMonths(-1).Month, 1);

            List<MediaInfo> mediaToCopy = media.MediaTree
                .Where(m => m.CreationTime > startOfMonth && !m.IsDirectory())
                .ToList();
        }
    }
}
