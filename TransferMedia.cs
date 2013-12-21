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
    public class TransferMedia
    {
        public static string MediaDir = "DCIM";
        public List<IMediaStorage> MediaStorage { get; set; }

        public TransferMedia()
        {
            
        }

        public void Initialize()
        {
            MediaStorage = GetMediaStorage();
        }

        private static List<IMediaStorage> GetMediaStorage()
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
                        if (dir.Contains(TransferMedia.MediaDir))
                        {
                            MediaDrive drive = new MediaDrive();
                            drive.Name = mediadrive.Name;
                            drive.MainMediaPath = dir;
                            drive.BuildMediaTree();
                            list.Add(drive);
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

            // Loop through all the connected devices
            foreach (string serial in android.ConnectedDevices)
            {
                MediaDevice device = new MediaDevice(serial);
                device.Name = serial;
                device.FindMainMediaPath();
                device.BuildMediaTree();
                //device.TransferToPC();
                list.Add(device);
                
            }

            return list;
        }

        //private static void FindMainMediaPath(List<string> paths, ref MediaDevice device)
        //{
        //    List<string> nextpaths = new List<string>();

        //    foreach (string path in paths)
        //    {
        //        using (StringReader r = new StringReader(device.ListDirectory(path)))
        //        {
        //            while (r.Peek() != -1)
        //            {
        //                MediaInfo info = new MediaInfo();
        //                device.UnixFileInfoToMediaInfo(r.ReadLine(), ref info);
        //                if (info.Name == MediaDir)
        //                {
        //                    device.MainMediaPath = path + (path.EndsWith("/") ? null : "/") + info.Name;
        //                    return;
        //                }
        //                else if (info.IsDirectory())
        //                    nextpaths.Add(path + (path.EndsWith("/") ? null : "/") + info.Name);

        //            }
        //        }
        //    }

        //    FindMainMediaPath(nextpaths, ref device);
        //}
    }
}
