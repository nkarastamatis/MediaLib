using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RegawMOD.Android;
using System.Text.RegularExpressions;
using Limilabs.Client.IMAP;
using Limilabs.Mail;
using Limilabs.Mail.MIME;

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
        private Dictionary<string, Imap> mailConnections = new Dictionary<string, Imap>();

        public TransferMedia()
        {
            
        }

        ~TransferMedia()
        {
            if (android != null)
                android.Dispose();
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

        public void AddMailConnection(string newUser, Imap newConnection)
        {
            lock (_lock)
            {
                mailConnections.Add(newUser, newConnection);
            }
        }

        private List<IMediaStorage> GetMediaStorage()
        {
            List<IMediaStorage> list = new List<IMediaStorage>();

            list.AddRange(MediaDrives());
            list.AddRange(MediaDevices());
            list.AddRange(MediaMail());

            return list;
        }

        private List<IMediaStorage> MediaMail()
        {
            List<IMediaStorage> list = new List<IMediaStorage>();

            foreach (KeyValuePair<string, Imap> entry in mailConnections)
            {
                if (MediaStorage == null ||
                    !MediaStorage.Any(s => s.Name == entry.Key))
                {
                    Inform(String.Format(
                        "Searching {0} for pictures.",
                        entry.Key));
                    MediaMail mail = new MediaMail(entry.Value);
                    mail.Name = entry.Key;
                    mail.MainMediaPath = typeof(Imap).ToString();
                    mail.BuildMediaTree();
                    Inform("Search Complete");
                    list.Add(mail);
                }
            }

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
                            Inform(String.Format(
                                "Searching {0} for pictures.",
                                mediadrive.Name));
                            MediaDrive drive = new MediaDrive();
                            drive.Name = mediadrive.Name;
                            drive.MainMediaPath = dir;
                            drive.BuildMediaTree();
                            Inform("Search Complete");
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
                    Inform(String.Format(
                        "Searching Andriod device {0} for pictures.",
                        serial));
                    MediaDevice device = new MediaDevice(serial);
                    device.Name = serial;
                    device.FindMainMediaPath();
                    device.BuildMediaTree();
                    Inform("Search complete");
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
                    Inform("Storage name: " + mediastorage.Name);

                    foreach (MediaInfo dir in mediastorage.MediaTree)
                    {
                        System.Diagnostics.Debug.Assert(dir.IsDirectory());

                        Directory.CreateDirectory(dir.DestinationPath);
                        foreach (MediaInfo file in dir.Files)
                        {
                            if (!File.Exists(file.FullName))
                            {
                                Inform(String.Format("Copying {0} ...", file.FullName));

                                mediastorage.CopyToPC(file);

                                Progress.Next();
                            }
                            else
                            {
                                Progress.Iterations--;
                                Inform(String.Format("Did not copy {0} because it already exists.", file.FullName));
                            }

                        }
                    }
                }

                Inform("Transfer Complete");
            }
        }

        private void Inform(string text)
        {
            Progress.Status = text;
            logger.log(text);
        }
    }

    public class Progress
    {
        public Progress(int max)
        {
            Max = max;
        }

        private int _current; // the 'real' curret
        public int Max { get; set; }
        public int Percent 
        {
            get
            {
                if (Iterations == 0)
                    return 0;
                int val = (int)((Convert.ToDouble(_current) / Convert.ToDouble(Iterations)) * Max);
                return val;
            }            
        }

        public string Status { get; set; }
        public int Iterations { get; set; }
        public void Next()
        {
            _current++;
        }

        public void Reset(int? iterations = null)
        {
            _current = 0;
            Status = String.Empty;

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
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
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
            file.WriteLine(
                String.Format("{0}: {1}", DateTime.Now, line));
            file.Close();
        }
    }
}
