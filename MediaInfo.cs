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
        public static string[] mediaExtensions = {
            ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF", //etc
            ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA", //etc
            ".AVI", ".MP4", ".DIVX", ".WMV", //etc
        };

        public MediaInfo()
        {
            Attributes = new FileAttributes();
        }

        private FileAttributes Attributes { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public string CurrentPath { get; set; }
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
                if (IsDirectory())
                    return Name;

                return String.Format("{0} {1}",
                    CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(CreationTime.Month),
                    CreationTime.Year.ToString());
            }
        }

        public string DestinationPath
        {
            get
            {
                string picturesFolder =
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                return 
                    Path.Combine(picturesFolder, DestinationFolder);
            }
        }

        public string FullName
        {
            get
            {
                return Path.Combine(DestinationPath, IsDirectory() ? String.Empty : Name);
            }
        }

        public DateTime? Date
        {
            get 
            {
                if (CreationTime == default(DateTime))
                    return null;

                return CreationTime;
            }
        }

    }
}
