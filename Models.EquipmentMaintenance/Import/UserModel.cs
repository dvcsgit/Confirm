using System.Text;

namespace Models.EquipmentMaintenance.Import
{
    public class UserModel
    {
        public string OrganizationUniqueID { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public string Email { get; set; }

        public string UID { get; set; }

        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    return string.Format("{0}({1})", Name, ErrorMessage);
                }
                else
                {
                    return Name;
                }
            }
        }


        public bool IsExist { get; set; }

        public bool IsParentError { get; set; }

        public bool IsError
        {
            get
            {
                return 
                    IsExist ||
                    IsParentError || 
                    string.IsNullOrEmpty(ID) ||
                    (!string.IsNullOrEmpty(ID) && ID.Length > 32) || 
                    string.IsNullOrEmpty(Name) ||
                    (!string.IsNullOrEmpty(Name) && Name.Length > 32) || 
                    (!string.IsNullOrEmpty(Title) && Title.Length > 32) ||
                    (!string.IsNullOrEmpty(Email) && Email.Length > 256) ||
                    (!string.IsNullOrEmpty(UID) && UID.Length > 100);
            }
        }

        public string ErrorMessage
        {
            get
            {
                var sb = new StringBuilder();

                if (IsExist)
                {
                    sb.Append(string.Format("{0} {1}", Resources.Resource.UserID, Resources.Resource.Exists));
                    sb.Append("、");
                }

                if (string.IsNullOrEmpty(ID))
                {
                    sb.Append(Resources.Resource.UserIDRequired);
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(ID) && ID.Length > 32)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.UserID, Resources.Resource.Length, 32));
                    sb.Append("、");
                }

                if (string.IsNullOrEmpty(Name))
                {
                    sb.Append(Resources.Resource.UserNameRequired);
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(Name) && Name.Length > 32)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.UserName, Resources.Resource.Length, 32));
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(Title) && Title.Length > 32)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.Title, Resources.Resource.Length, 32));
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(Email) && Email.Length > 256)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.EMail, Resources.Resource.Length, 256));
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(UID) && UID.Length > 100)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.UID, Resources.Resource.Length, 100));
                    sb.Append("、");
                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }
    }
}
