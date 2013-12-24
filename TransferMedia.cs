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
        private Object _lock = new Object();

        public Progress Progress = new Progress(100);

        private Logger logger = new Logger("TransferMedia.log");
        private AndroidController android = AndroidController.Instance;

        public TransferMedia()
        {
            
        }

        #region Populate MediaStorage

        public void Initialize()
        {
            MediaStorage = GetMediaStorage();
        }

        public bool Refresh()
        {
            lock (_lock)
            {
                if (MediaStorage == null)
                {
                    Initialize();
                    return true;
                }
                else
                {
                    int beforeCount = MediaStorage.Count;
                    MediaStorage.AddRange(GetMediaStorage());
                    return beforeCount != MediaStorage.Count;
                }
            }
        }

        private List<IMediaStorage> GetMediaStorage()
        {
            List<IMediaStorage> list = new List<IMediaStorage>();

            list.AddRange(MediaDrives());
            list.AddRange(MediaDevices());

            return list;
        }

        private List<IMediaStorage> MediaDrives()
        {
            List<IMediaStorage> list = new List<IMediaStorage>();

            var mediadrives = DriveInfo.GetDrives()
                .Where(
                c => c.DriveType == DriveType.Removable);

            if (MediaStorage != null)
            {
                MediaStorage.RemoveAll(
                    s => s is MediaDrive &&
                        !mediadrives
                        .Select(d => d.Name)
                        .Any(s.Name.Contains)
                        );
            }

            foreach (DriveInfo mediadrive in mediadrives)
            {
                if (mediadrive.Name.Contains("A"))
                    continue;

                if (mediadrive.IsReady)
                {
                    List<string> dirs = new List<string>(Directory.EnumerateDirectories(mediadrive.Name));
                    foreach (string dir in dirs)
                    {
                        if (dir.Contains(TransferMedia.MediaDir) &&
                            (MediaStorage == null ||
                            !MediaStorage.Any(s => s.Name == mediadrive.Name)))
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

        private List<IMediaStorage> MediaDevices()
        {
            List<IMediaStorage> list = new List<IMediaStorage>();

            List<string> connectedDevices = android.ConnectedDevices;

            if (MediaStorage != null)
            {
                MediaStorage.RemoveAll(
                    s => s is MediaDevice &&
                        !connectedDevices
                        .Any(s.Name.Contains)
                        );
            }

            // Loop through all the connected devices
            foreach (string serial in connectedDevices)
            {
                if (MediaStorage == null || 
                    !MediaStorage.Any(s => s.Name == serial))
                {
                    MediaDevice device = new MediaDevice(serial);
                    device.Name = serial;
                    device.FindMainMediaPath();
                    device.BuildMediaTree();
                    list.Add(device);
                }
                
            }

            return list;
        }

        #endregion

        public void transfer()
        {
            lock (_lock)
            {
                int totalFiles = 0;
                MediaStorage.ForEach(s => totalFiles += s.MediaTree.FileCount);
                Progress.Reset(totalFiles);
                logger.log(
                    String.Format(
                    "{0}: Starting to transfer {1} files",
                    DateTime.Now,
                    totalFiles));

                foreach (IMediaStorage mediastorage in MediaStorage)
                {
                    foreach (MediaInfo dir in mediastorage.MediaTree)
                    {
                        System.Diagnostics.Debug.Assert(dir.IsDirectory());

                        Directory.CreateDirectory(dir.DestinationPath);
                        foreach (MediaInfo file in dir.Files)
                        {
                            if (!File.Exists(file.FullName))
                                mediastorage.CopyToPC(file);
                            else
                                logger.log(String.Format("Did not copy {0} because it already exists.", file.FullName));

                            Progress.Next();
                        }
                    }
                }

                logger.log(
                    String.Format(
                    "{0}: Transfer complete",
                    DateTime.Now));
            }
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

    public class Progress
    {
        public Progress(int max)
        {
            Max = max;
        }

        private int _current; // the 'real' curret
        public int Max { get; set; }
        public int Current 
        {
            get
            {
                if (Iterations == 0)
                    return 0;
                int val = (int)((Convert.ToDouble(_current) / Convert.ToDouble(Iterations)) * Max);
                return val;
            }            
        }

        public int Iterations { get; set; }
        public void Next()
        {
            _current++;
        }

        public void Reset(int? iterations = null)
        {
            _current = 0;

            if (iterations != null)
                Iterations = iterations.Value;
        }
    }

    public class Logger
    {
        private System.IO.StreamWriter file;
        private string fullname;

        public Logger(String filename)
        {

            // Write the string to a file.append mode is enabled so that the log
            // lines get appended to  test.txt than wiping content and writing the log

            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TransferMedia");

            Directory.CreateDirectory(path);
            fullname = Path.Combine(path, filename);

            //file = new System.IO.StreamWriter(fullname, true);
            
        }

        ~Logger()
        {
            //file.Close();
        }

        public void log(String line)
        {
            file = new System.IO.StreamWriter(fullname, true);
            file.WriteLine(line);
            file.Close();
        }
    }
}
