using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace MediaLib
{
    public class MediaInfo
    {
        public MediaInfo()
        {
            Attributes = new FileAttributes();
        }

        private FileAttributes Attributes { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public string Path { get; set; }
        public List<MediaInfo> Files { get; set; }
        
        public void SetAsDirectory()
        {
            this.Attributes |= FileAttributes.Directory;
        }

        public bool IsDirectory()
        {
            return
                (this.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }

        public string DestinationFolder
        {
            get 
            {
                if (CreationTime == default(DateTime))
                    return null;

                return String.Format("{0} {1}",
                    CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(CreationTime.Month),
                    CreationTime.Year.ToString());
            }
        }

    }
}
