using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaLib
{
    class MediaDrive : IMediaStorage
    {
        public MediaDrive()
        {
            MediaTree = new MediaTree();
        }

        #region IMediaStorage Members

        public void TransferToPC()
        {
            throw new NotImplementedException();
        }

        public string Name { get; set; }
        public string MainMediaPath { get; set; }

        public MediaTree MediaTree { get; set; }

        public void FindMainMediaPath()
        {
            
        }

        public void SearchPaths(List<string> paths, SearchAction action)
        {
            List<string> nextpaths = new List<string>();

            foreach (string path in paths)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                FileSystemInfo[] files = dirInfo.GetFileSystemInfos();
                foreach (FileSystemInfo file in files)
                {
                    MediaInfo info = FileSystemInfoToMediaInfo(file);

                    bool endsearch = action(ref info, path);
                    if (endsearch)
                        return;

                    if (info.IsDirectory())
                        nextpaths.Add(Path.Combine(path, info.Name));
                }
            }
            if (nextpaths.Count > 0 && nextpaths.Count < 50)
                SearchPaths(nextpaths, action);
        }

        public void CopyToPC(MediaInfo mediaToCopy, string destinationPath)
        {
            

        }

        private MediaInfo FileSystemInfoToMediaInfo(FileSystemInfo file)
        {
            MediaInfo mediainfo = new MediaInfo();
            if ((file.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                mediainfo.SetAsDirectory();

            mediainfo.CreationTime = file.CreationTime;
            mediainfo.Name = file.Name;
            return mediainfo;
        }

        //public bool BuildTree(ref MediaInfo info, string path)
        //{
        //    return true;
        //}

        #endregion

    }
}
