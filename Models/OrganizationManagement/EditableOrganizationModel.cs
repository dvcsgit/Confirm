using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.OrganizationManagement
{
    public class EditableOrganizationModel
    {
        public bool CanDelete { get; set; }

        public string UniqueID { get; set; }

        public string FullDescription { get; set; }
    }
}
