using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.Shared
{
    public class ASEUserModel
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string OrganizationDescription { get; set; }

        public string Email { get; set; }

        public string EName
        {
            get
            {
                try
                {
                    var ename = Email.Substring(0, Email.IndexOf('@'));
                    return ename;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(EName))
                {
                    return string.Format("{0}/{1}/{2}/{3}", OrganizationDescription, ID, Name, EName);
                }
                else
                {
                    return string.Format("{0}/{1}/{2}", OrganizationDescription, ID, Name);
                }
            }
        }

        public string Term
        {
            get
            {
                return Display.ToLower();
            }
        }
    }
}
