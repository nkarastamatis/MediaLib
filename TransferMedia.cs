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
    public class UnixFileSystemInfo
    {
        public FileAttributes Attributes { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public string Path { get; set; }
    }

    public static class TransferMedia
    {
        public static void transfer()
        {

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

            string serial;
            AndroidController android = AndroidController.Instance;;
            Device device;

            android.UpdateDeviceList();


            if (android.HasConnectedDevices)
            {
                serial = android.ConnectedDevices[0];
                device = android.GetConnectedDevice(serial);
                //this.TextBox1.Text = device.SerialNumber;

                List<FileSystemInfo> andriodMediaFileSystem = new List<FileSystemInfo>();

                //var r = device.PullDirectory("/sdcard/DCIM", "C:\\temp\\DCIM");
                var s = device.PullFile("/sdcard/DCIM/100BURST/001", "C:\\temp\\BURST");

                AdbCommand adbCmd = Adb.FormAdbShellCommand(device, false, "ls", "-l", "/*/DCIM/100MEDIA");
                using (StringReader r = new StringReader(Adb.ExecuteAdbCommand(adbCmd)))
                {
                    string line;
                    string[] splitLine;
                    string dir;

                    while (r.Peek() != -1)
                    {
                        line = r.ReadLine();
                        Regex x = new Regex(" +");
                        splitLine = x.Split(line);
                        //splitLine = line.Split(' ');

                        if (splitLine.Length == 1)
                            continue;

                        int ix = 0;
                        UnixFileSystemInfo newFileOrDir = new UnixFileSystemInfo();
                        newFileOrDir.Attributes = new FileAttributes();
                        if (splitLine[ix++].StartsWith("d"))
                            newFileOrDir.Attributes &= FileAttributes.Directory;

                        ix++;
                        ix++;

                        if ((newFileOrDir.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
                            ix++;

                        newFileOrDir.CreationTime = DateTime.Parse(splitLine[ix++] + " " + splitLine[ix++]);

                        newFileOrDir.Name = splitLine[ix];

                        try
                        {
                            if (line.Contains(" on /system "))
                            {

                            }
                        }
                        catch
                        {

                        }
                    }
                }

                var success = device.FileSystem.FileOrDirectory("DCIM");
                device.Phone.DialPhoneNumber("4433980031");

            }
            else
            {
                //this.TextBox1.Text = "Error - No Devices Connected";
            }

            return list;
        }
    }
}
