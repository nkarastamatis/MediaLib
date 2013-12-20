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
        string MainMediaPath { get; set; }
        MediaTree MediaTree { get; set; }

        //void TransferToPC();
        void FindMainMediaPath();
        void SearchPaths(List<string> paths, SearchAction action);
        void CopyToPC(MediaInfo mediaToCopy, string destinationPath);
        //bool BuildTree(ref MediaInfo info, string path);
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

            info.Path = path;
            media.MediaTree.Add(info);

            return endSearch;
        }

        public static void TransferToPC(this IMediaStorage media)
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.AddMonths(-1).Month, 1);

            List<MediaInfo> mediaToCopy = media.MediaTree
                .Where(m => m.CreationTime > startOfMonth && !m.IsDirectory())
                .ToList();

            string picturesFolder =
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string destinationFolder = String.Format("{0} {1}",
                CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(startOfMonth.Month),
                startOfMonth.Year.ToString());
            string destinationPath = Path.Combine(picturesFolder, destinationFolder);

            Directory.CreateDirectory(destinationPath);

            foreach (MediaInfo info in mediaToCopy)
            {
                if (!File.Exists(Path.Combine(destinationPath, info.Name)))
                    media.CopyToPC(info, destinationPath);
            }

        }
    }
}
