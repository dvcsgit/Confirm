using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.Message
{
    public class UserPhotoModel
    {
        public string FilePath
        {
            get
            {
                return Path.Combine(Config.UserPhotoFolderPath, string.Format("{0}.{1}", FileUniqueID, Extension));
            }
        }

        public string FileUniqueID { get; set; }

        public string Extension { get; set; }
    }
}
