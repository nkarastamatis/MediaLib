using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MediaLib
{
    public class MediaTree : List<MediaInfo>
    {
        private int _fileCount = 0;

        public int FileCount { get { return _fileCount; } }

        static bool IsMediaFile(string path)
        {
            return -1 != Array.IndexOf(
                MediaInfo.mediaExtensions, 
                Path.GetExtension(path).ToUpperInvariant());
        }

        public new void Add(MediaInfo info)
        {
            if (info.IsDirectory() || !IsMediaFile(info.Name))
                return;

            var index = FindIndex(i => i.Name == info.DestinationFolder);
            if (index >= 0)
            {
                this[index].Files.Add(info);
                _fileCount++;
            }
            else
            {
                NewDirectory(info);
            }
        }

        private void NewDirectory(MediaInfo info)
        {
            MediaInfo newDir = new MediaInfo();
            newDir.SetAsDirectory();
            newDir.Name = info.DestinationFolder;
            newDir.Files = new List<MediaInfo>();        
            base.Add(newDir);

            // now the directory exists, so use the Add override again
            Add(info);
        }
    }
}
