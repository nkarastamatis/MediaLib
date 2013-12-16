using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaLib
{
    class MediaDrive : IMediaStorage
    {
        public void TransferToPC()
        {
            throw new NotImplementedException();
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

        public void BuildMediaTree()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
