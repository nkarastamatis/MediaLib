using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaLib
{
    public delegate bool SearchAction(ref MediaInfo info, string path);

    public interface IMediaStorage
    {
        string MainMediaPath { get; set; }
        List<MediaInfo> MediaTree { get; set; }

        void TransferToPC();
        void FindMainMediaPath();
        void SearchPaths(List<string> paths, SearchAction action);
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
    }
}
