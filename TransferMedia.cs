using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RegawMOD.Android;
using System.Text.RegularExpressions;

namespace MediaLib
{
    public static class TransferMedia
    {
        public static string MediaDir = "DCIM";

        public static void transfer()
        {
            MediaStorage();
        }

        private static List<IMediaStorage> MediaStorage()
        {
            List<IMediaStorage> list = new List<IMediaStorage>();

            list.AddRange(MediaDrives());
            list.AddRange(MediaDevices());

            return list;
        }

        private static List<IMediaStorage> MediaDrives()
        {
            List<IMediaStorage> list = new List<IMediaStorage>();

            var mediadrives = DriveInfo.GetDrives()
                .Where(
                c => c.DriveType == DriveType.Removable); 

            foreach (DriveInfo mediadrive in mediadrives)
            {
                if (mediadrive.Name.Contains("A"))
                    continue;

                if (mediadrive.IsReady)
                {
                    List<string> dirs = new List<string>(Directory.EnumerateDirectories(mediadrive.Name));
                    foreach (string dir in dirs)
                    {
                        if (dir.Contains("DCIM"))
                        {
                            MediaDrive newdrive = new MediaDrive();
                            list.Add(newdrive);
                        }
                    }
                }
            }

            return list;
        }

        private static List<IMediaStorage> MediaDevices()
        {
            List<IMediaStorage> list = new List<IMediaStorage>();

            AndroidController android = AndroidController.Instance;

            foreach (string serial in android.ConnectedDevices)
            {
                MediaDevice device = new MediaDevice(serial);
                //device = android.GetConnectedDevice(serial) as MediaDevice;
                string mainmediapath = "/*/" + MediaDir;
                using (StringReader r = new StringReader(device.ListDirectory(mainmediapath)))
                {
                    string line;
                    string[] splitLine;
                    string dir;

                    while (r.Peek() != -1)
                    {
                        MediaInfo newFileOrDir = new MediaInfo();
                        device.UnixFileInfoToMediaInfo(r.ReadLine(), out newFileOrDir);
                    }
                }
            }

            return list;
        }
    }
}
