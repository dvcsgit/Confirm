using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.Message
{
    public class UserModel
    {
        public string OrganizationDescription { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public string User
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    return string.Format("{0}/{1}", ID, Name);
                }
                else
                {
                    return ID;
                }
            }
        }

        public UserPhotoModel Photo { get; set; }

        public bool HasPhoto
        {
            get
            {
                return Photo != null && !string.IsNullOrEmpty(Photo.FileUniqueID);
            }
        }
    }
}
