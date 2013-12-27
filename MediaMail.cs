using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Limilabs.Client.IMAP;
using System.Drawing;

namespace MediaLib
{
    class MediaMail : IMediaStorage
    {
        private Imap imap;

        public MediaMail(Imap imap)
        {
            this.imap = imap;
            MediaTree = new MediaTree();
        }

        public string Name { get; set; }

        public string MainMediaPath { get; set; }

        public MediaTree MediaTree { get; set; }

        public void FindMainMediaPath()
        {
            
        }

        public void SearchPaths(List<string> paths, SearchAction action)
        {
            // Not the best name for this in the context of mail but I'm 
            // just going with it because the action that builds the tree
            // should end up being the same.
            
            imap.SelectInbox();

            // All search methods return list of unique ids of found messages.
            List<long> unseen = imap.Search(Flag.Unseen);           // Simple 'by flag' search.
            List<MessageInfo> infos = imap.GetMessageInfoByUID(unseen);

            foreach (MessageInfo messageinfo in infos)
            {
                //BodyStructure structure = imap.GetBodyStructureByUID(uid);
                foreach (MimeStructure attachment in messageinfo.BodyStructure.NonVisuals)
                {
                    MediaInfo info = MessageInfoToMediaInfo(messageinfo, attachment);
                    info.attachment = attachment;
                    action(ref info, messageinfo.Envelope.Subject);                                       
                }
            }

        }

        public void CopyToPC(MediaInfo mediaToCopy)
        {
            MimeStructure attachment = mediaToCopy.attachment as MimeStructure;

            Image image;

            if (System.Diagnostics.Debugger.IsAttached)
                image = Image.FromStream(
                    new System.IO.MemoryStream(imap.PeekDataByUID(attachment)));
            else
                image = Image.FromStream(
                    new System.IO.MemoryStream(imap.GetDataByUID(attachment)));

            image.Save(mediaToCopy.FullName);
            
        }

        private MediaInfo MessageInfoToMediaInfo(MessageInfo messageinfo, MimeStructure attachment)
        {
            MediaInfo mediainfo = new MediaInfo();

            mediainfo.CreationTime = messageinfo.Envelope.Date.GetValueOrDefault(DateTime.Now);
            mediainfo.Name = attachment.SafeFileName;
            return mediainfo;
        }
    }
}
