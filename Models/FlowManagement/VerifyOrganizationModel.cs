using Models.Shared;
using System.Collections.Generic;
using System.Text;
namespace Models.FlowManagement
{
    public class VerifyOrganizationModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public int Seq { get; set; }

        public bool IsManagerExist
        {
            get
            {
#if ASE
                return !string.IsNullOrEmpty(ManagerID);
#else
                return ManagerList.Count > 0;
#endif
            }
        }

        public string ManagerID { get; set; }

        public string ManagerName { get; set; }

        public string Manager
        {
            get
            {
                if (IsManagerExist)
                {
                    if (!string.IsNullOrEmpty(ManagerName))
                    {
                        return string.Format("{0}/{1}", ManagerID, ManagerName);
                    }
                    else
                    {
                        return ManagerID;
                    }
                }
                else
                {
                    return string.Format("{0} {1}", Resources.Resource.NotSet, Resources.Resource.Manager);
                }
            }
        }


        public List<UserModel> ManagerList { get; set; }

        public string Managers
        {
            get
            {
                if (ManagerList != null && ManagerList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var manager in ManagerList)
                    {
                        sb.Append(manager.User);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public VerifyOrganizationModel()
        {
            ManagerList = new List<UserModel>();
        }
    }
}
