﻿using System;
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
        private List<string> AndroidSystemDirs = new List<string>(new string[]
        {
            "/proc", "/dev", "/etc", "/acct", "/firmware", "/sys", "/system"
        });

        public MediaDevice(string deviceSerial)
            : base(deviceSerial)
        {
            MediaTree = new List<MediaInfo>();
        }
        
        #region IMediaStorage Members

        public string MainMediaPath { get; set; }

        public List<MediaInfo> MediaTree { get; set; }

        public void TransferToPC()
        {
            throw new NotImplementedException();
        }

        public void BuildMediaTree()
        {
            List<string> paths = new List<string>();
            paths.Add(MainMediaPath);
            SearchPaths(paths, BuildTree);
        }



        public void FindMainMediaPath()
        {
            // start with the root and Find DCIM
            List<string> paths = new List<string>();
            paths.Add("/");
            SearchPaths(paths, IsMediaDir);
        }

        #endregion

        delegate bool SearchAction(ref MediaInfo info, string path);

        private bool IsMediaDir(ref MediaInfo info, string path)
        {
            bool endSearch = false;
            if (info.Name == TransferMedia.MediaDir)
            {
                MainMediaPath = path + (path.EndsWith("/") ? null : "/") + info.Name;
                endSearch = true;
            }

            return endSearch;
        }

        private bool BuildTree(ref MediaInfo info, string path)
        {
            bool endSearch = false;
            
            info.Path = path;
            MediaTree.Add(info);
        
            return endSearch;
        }

        private void SearchPaths(List<string> paths, SearchAction action)
        {
            List<string> nextpaths = new List<string>();

            foreach (string path in paths)
            {

                using (StringReader r = new StringReader(ListDirectory(path)))
                {
                    while (r.Peek() != -1)
                    {
                        MediaInfo info = UnixFileInfoToMediaInfo(r.ReadLine());

                        if (info != null)
                        {
                            bool endsearch = action(ref info, path);
                            if (endsearch)
                                return;

                            if (info.IsDirectory() && !AndroidSystemDir(path))
                                nextpaths.Add(path + (path.EndsWith("/") ? null : "/") + info.Name);
                        }
                    }
                }
            }
            if (nextpaths.Count > 0 && nextpaths.Count < 50)
                SearchPaths(nextpaths, action);
        }

        private bool AndroidSystemDir(string path)
        {
            return AndroidSystemDirs.Any(path.Contains);
        }

        private MediaInfo UnixFileInfoToMediaInfo(string fileinfo)
        {
            MediaInfo mediainfo = null;

            Regex x = new Regex(" +");
            string[] splitInfo = x.Split(fileinfo);            

            int ix = 0;
            
            // '-' means file
            // only process if this is a file of directory
            char type = splitInfo[ix++].FirstOrDefault();
            if (type == 'd' || type == '-')
            {
                mediainfo = new MediaInfo();

                if (type == 'd')
                    mediainfo.SetAsDirectory();

                // user (skip)
                ix++;

                // dir? (skip)
                ix++;

                // Files have a size here, so skip if this is a file.
                if (!mediainfo.IsDirectory())
                    ix++;

                mediainfo.CreationTime = DateTime.Parse(splitInfo[ix++] + " " + splitInfo[ix++]);

                mediainfo.Name = splitInfo[ix];
            }

            return mediainfo;

        }
    }
}
