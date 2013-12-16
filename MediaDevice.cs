using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RegawMOD.Android;

namespace MediaLib
{
    class MediaDevice : Device, IMediaStorage
    {
        public MediaDevice(string deviceSerial)
            : base(deviceSerial)
        {

        }
        
        #region IMediaStorage Members

        public string MainMediaPath
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void TransferToPC()
        {
            throw new NotImplementedException();
        }

        public void BuildMediaTree()
        {
            using (StringReader r = new StringReader(this.ListDirectory("/*/DCIM/100MEDIA")))
            {
                string line;
                string[] splitLine;
                string dir;

                while (r.Peek() != -1)
                {
                    line = r.ReadLine();
                    Regex x = new Regex(" +");
                    splitLine = x.Split(line);

                    if (splitLine.Length == 1)
                        continue;

                    int ix = 0;
                    MediaInfo newFileOrDir = new MediaInfo();
                    if (splitLine[ix++].StartsWith("d"))
                        newFileOrDir.SetAsDirectory();

                    ix++;
                    ix++;

                    // Files have a size here, so skip is this is a file.
                    if (!newFileOrDir.IsDirectory())
                        ix++;

                    newFileOrDir.CreationTime = DateTime.Parse(splitLine[ix++] + " " + splitLine[ix++]);

                    newFileOrDir.Name = splitLine[ix];

                }
            }
        }

        public void UnixFileInfoToMediaInfo(string fileinfo, out MediaInfo mediainfo)
        {
            mediainfo = new MediaInfo();

            Regex x = new Regex(" +");
            string[] splitInfo = x.Split(fileinfo);

            if (splitInfo.Length == 1)
                return;

            int ix = 0;
            MediaInfo newFileOrDir = new MediaInfo();
            if (splitInfo[ix++].StartsWith("d"))
                newFileOrDir.SetAsDirectory();

            // user (skip)
            ix++;

            // 
            ix++;

            // Files have a size here, so skip is this is a file.
            if (!newFileOrDir.IsDirectory())
                ix++;

            newFileOrDir.CreationTime = DateTime.Parse(splitInfo[ix++] + " " + splitInfo[ix++]);

            newFileOrDir.Name = splitInfo[ix];

        }

        #endregion      
    }
}
