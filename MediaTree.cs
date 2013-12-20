using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaLib
{
    public class MediaTree : List<MediaInfo>
    {
        public new void Add(MediaInfo info)
        {
            if (info.IsDirectory())
                return;

            var index = FindIndex(i => i.Name == info.DestinationFolder);
            if (index >= 0)
            {
                this[index].Files.Add(info);
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
            newDir.Files.Add(info);

            base.Add(newDir);
        }
    }
}
