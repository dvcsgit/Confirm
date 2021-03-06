﻿using System.Text;

namespace Models.EquipmentMaintenance.Import
{
    public class HandlingMethodModel
    {
        public string OrganizationUniqueID { get; set; }

        public string UniqueID { get; set; }

        public string HandlingMethodType { get; set; }

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

        public bool IsParentError { get; set; }

        public bool IsError
        {
            get
            {
                return
                    IsExist || 
                    IsParentError ||
                    string.IsNullOrEmpty(HandlingMethodType) ||
                    (!string.IsNullOrEmpty(HandlingMethodType) && HandlingMethodType.Length > 32) || 
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
                    sb.Append(string.Format("{0} {1}", Resources.Resource.HandlingMethodID, Resources.Resource.Exists));
                    sb.Append("、");
                }

                if (string.IsNullOrEmpty(HandlingMethodType))
                {
                    sb.Append(Resources.Resource.HandlingMethodTypeRequired);
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(HandlingMethodType) && HandlingMethodType.Length > 32)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.HandlingMethodType, Resources.Resource.Length, 32));
                    sb.Append("、");
                }

                if (string.IsNullOrEmpty(ID))
                {
                    sb.Append(Resources.Resource.HandlingMethodIDRequired);
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(ID) && ID.Length > 32)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.HandlingMethodID, Resources.Resource.Length, 32));
                    sb.Append("、");
                }

                if (string.IsNullOrEmpty(Description))
                {
                    sb.Append(Resources.Resource.HandlingMethodDescriptionRequired);
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(Description) && Description.Length > 64)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.HandlingMethodDescription, Resources.Resource.Length, 64));
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
