using System.Text;

namespace Models.EquipmentMaintenance.Import
{
    public class UnRFIDReasonModel
    {
        public string ID { get; set; }

        public string Description { get; set; }

        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    return string.Format("{0}({1})", Description, ErrorMessage);
                }
                else
                {
                    return Description;
                }
            }
        }

        public bool IsExist { get; set; }

        public bool IsError
        {
            get
            {
                return
                    IsExist ||
                    string.IsNullOrEmpty(ID) ||
                    (!string.IsNullOrEmpty(ID) && ID.Length > 32) ||
                    string.IsNullOrEmpty(Description) ||
                    (!string.IsNullOrEmpty(Description) && Description.Length > 64);
            }
        }

        public string ErrorMessage
        {
            get
            {
                var sb = new StringBuilder();

                if (IsExist)
                {
                    sb.Append(string.Format("{0} {1}", Resources.Resource.AbnormalReasonID, Resources.Resource.Exists));
                    sb.Append("、");
                }

                if (string.IsNullOrEmpty(ID))
                {
                    sb.Append(Resources.Resource.AbnormalReasonIDRequired);
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(ID) && ID.Length > 32)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.AbnormalReasonID, Resources.Resource.Length, 32));
                    sb.Append("、");
                }

                if (string.IsNullOrEmpty(Description))
                {
                    sb.Append(Resources.Resource.AbnormalReasonDescriptionRequired);
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(Description) && Description.Length > 64)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.AbnormalReasonDescription, Resources.Resource.Length, 64));
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
