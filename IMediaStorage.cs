﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaLib
{
    interface IMediaStorage
    {
        string MainMediaPath { get; set; }
        List<MediaInfo> MediaTree { get; set; }

        void TransferToPC();
        void FindMainMediaPath();
        void BuildMediaTree();
    }
}
