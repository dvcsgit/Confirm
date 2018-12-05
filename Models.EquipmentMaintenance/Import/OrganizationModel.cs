using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.Import
{
    public class OrganizationModel
    {
        public string UniqueID { get; set; }

        public string ParentUniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public List<OrganizationModel> OrganizationList { get; set; }

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

        public bool IsNewOrganization { get; set; }

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
                    string.IsNullOrEmpty(Description) ||
                    (!string.IsNullOrEmpty(Description) && Description.Length > 64);
            }
        }

        public string ErrorMessage
        {
            get
            {
                if (IsNewOrganization)
                {
                    var sb = new StringBuilder();

                    if (IsExist)
                    {
                        sb.Append(string.Format("{0} {1}", Resources.Resource.OrganizationID, Resources.Resource.Exists));
                        sb.Append("、");
                    }

                    if (string.IsNullOrEmpty(ID))
                    {
                        sb.Append(Resources.Resource.OrganizationIDRequired);
                        sb.Append("、");
                    }

                    if (!string.IsNullOrEmpty(ID) && ID.Length > 32)
                    {
                        sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.OrganizationID, Resources.Resource.Length, 32));
                        sb.Append("、");
                    }

                    if (string.IsNullOrEmpty(Description))
                    {
                        sb.Append(Resources.Resource.OrganizationDescriptionRequired);
                        sb.Append("、");
                    }

                    if (!string.IsNullOrEmpty(Description) && Description.Length > 64)
                    {
                        sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.OrganizationDescription, Resources.Resource.Length, 64));
                        sb.Append("、");
                    }

                    if (sb.Length > 0)
                    {
                        sb.Remove(sb.Length - 1, 1);
                    }

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool HaveNewOrganization
        {
            get
            {
                return NewOrganizationCount > 0;
            }
        }

        public int NewOrganizationCount
        {
            get
            {
                return OrganizationList.Sum(x => x.NewOrganizationCount) + (IsNewOrganization ? 1 : 0);
            }
        }

        public int ValidOrganizationCount
        {
            get
            {
                return OrganizationList.Sum(x => x.ValidOrganizationCount) + (IsNewOrganization && !IsError ? 1 : 0);
            }
        }

        public List<OrganizationModel> ValidOrganizationList
        {
            get
            {
                var itemList = new List<OrganizationModel>();

                if (!IsError && IsNewOrganization)
                {
                    itemList.Add(this);
                }

                itemList.AddRange(OrganizationList.SelectMany(x => x.ValidOrganizationList).ToList());

                return itemList;
            }
        }

        public List<UserModel> UserList { get; set; }

        public bool HaveNewUser
        {
            get
            {
                return NewUserCount > 0;
            }
        }

        public int NewUserCount
        {
            get
            {
                return UserList.Count + OrganizationList.Sum(x => x.NewUserCount);
            }
        }

        public int ValidUserCount
        {
            get
            {
                return UserList.Count(x => !x.IsError) + OrganizationList.Sum(x => x.ValidUserCount);
            }
        }

        public List<UserModel> ValidUserList
        {
            get
            {
                var itemList = new List<UserModel>();

                itemList.AddRange(UserList.Where(x => !x.IsError).ToList());

                itemList.AddRange(OrganizationList.SelectMany(x => x.ValidUserList).ToList());

                return itemList;
            }
        }

        public List<HandlingMethodModel> HandlingMethodList { get; set; }

        public bool HaveNewHandlingMethod
        {
            get
            {
                return NewHandlingMethodCount > 0;
            }
        }

        public int NewHandlingMethodCount
        {
            get
            {
                return HandlingMethodList.Count + OrganizationList.Sum(x => x.NewHandlingMethodCount);
            }
        }

        public int ValidHandlingMethodCount
        {
            get
            {
                return HandlingMethodList.Count(x => !x.IsError) + OrganizationList.Sum(x => x.ValidHandlingMethodCount);
            }
        }

        public List<HandlingMethodModel> ValidHandlingMethodList
        {
            get
            {
                var itemList = new List<HandlingMethodModel>();

                itemList.AddRange(HandlingMethodList.Where(x => !x.IsError).ToList());

                itemList.AddRange(OrganizationList.SelectMany(x => x.ValidHandlingMethodList).ToList());

                return itemList;
            }
        }

        public List<AbnormalReasonModel> AbnormalReasonList { get; set; }

        public bool HaveNewAbnormalReason
        {
            get
            {
                return NewAbnormalReasonCount > 0;
            }
        }

        public int NewAbnormalReasonCount
        {
            get
            {
                return AbnormalReasonList.Count + OrganizationList.Sum(x => x.NewAbnormalReasonCount);
            }
        }

        public int ValidAbnormalReasonCount
        {
            get
            {
                return AbnormalReasonList.Count(x => !x.IsError) + OrganizationList.Sum(x => x.ValidAbnormalReasonCount);
            }
        }

        public List<AbnormalReasonModel> ValidAbnormalReasonList
        {
            get
            {
                var itemList = new List<AbnormalReasonModel>();

                itemList.AddRange(AbnormalReasonList.Where(x => !x.IsError).ToList());

                itemList.AddRange(OrganizationList.SelectMany(x => x.ValidAbnormalReasonList).ToList());

                return itemList;
            }
        }

        public List<CheckItemModel> CheckItemList { get; set; }

        public bool HaveNewCheckItem
        {
            get
            {
                return NewCheckItemCount > 0;
            }
        }

        public int NewCheckItemCount
        {
            get
            {
                return CheckItemList.Count + OrganizationList.Sum(x => x.NewCheckItemCount);
            }
        }

        public int ValidCheckItemCount
        {
            get
            {
                return CheckItemList.Count(x => !x.IsError) + OrganizationList.Sum(x => x.ValidCheckItemCount);
            }
        }

        public List<CheckItemModel> ValidCheckItemList
        {
            get
            {
                var itemList = new List<CheckItemModel>();

                itemList.AddRange(CheckItemList.Where(x => !x.IsError).ToList());

                itemList.AddRange(OrganizationList.SelectMany(x => x.ValidCheckItemList).ToList());

                return itemList;
            }
        }

        public List<EquipmentModel> EquipmentList { get; set; }

        public bool HaveNewEquipment
        {
            get
            {
                return NewEquipmentCount > 0;
            }
        }

        public int NewEquipmentCount
        {
            get
            {
                return EquipmentList.Count + OrganizationList.Sum(x => x.NewEquipmentCount);
            }
        }

        public int ValidEquipmentCount
        {
            get
            {
                return EquipmentList.Count(x => !x.IsError) + OrganizationList.Sum(x => x.ValidEquipmentCount);
            }
        }

        public List<EquipmentModel> ValidEquipmentList
        {
            get
            {
                var itemList = new List<EquipmentModel>();

                itemList.AddRange(EquipmentList.Where(x => !x.IsError).ToList());

                itemList.AddRange(OrganizationList.SelectMany(x => x.ValidEquipmentList).ToList());

                return itemList;
            }
        }

        public List<ControlPointModel> ControlPointList { get; set; }

        public bool HaveNewControlPoint
        {
            get
            {
                return NewControlPointCount > 0;
            }
        }

        public int NewControlPointCount
        {
            get
            {
                return ControlPointList.Count + OrganizationList.Sum(x => x.NewControlPointCount);
            }
        }

        public int ValidControlPointCount
        {
            get
            {
                return ControlPointList.Count(x => !x.IsError) + OrganizationList.Sum(x => x.ValidControlPointCount);
            }
        }

        public List<ControlPointModel> ValidControlPointList
        {
            get
            {
                var itemList = new List<ControlPointModel>();

                itemList.AddRange(ControlPointList.Where(x => !x.IsError).ToList());

                itemList.AddRange(OrganizationList.SelectMany(x => x.ValidControlPointList).ToList());

                return itemList;
            }
        }

        public List<RouteModel> RouteList { get; set; }

        public bool HaveNewRoute
        {
            get
            {
                return NewRouteCount > 0;
            }
        }

        public int NewRouteCount
        {
            get
            {
                return RouteList.Count + OrganizationList.Sum(x => x.NewRouteCount);
            }
        }

        public int ValidRouteCount
        {
            get
            {
                return RouteList.Count(x => !x.IsError) + OrganizationList.Sum(x => x.ValidRouteCount);
            }
        }

        public List<RouteModel> ValidRouteList
        {
            get
            {
                var itemList = new List<RouteModel>();

                itemList.AddRange(RouteList.Where(x => !x.IsError).ToList());

                itemList.AddRange(OrganizationList.SelectMany(x => x.ValidRouteList).ToList());

                return itemList;
            }
        }

        public OrganizationModel()
        {
            OrganizationList = new List<OrganizationModel>();
            UserList = new List<UserModel>();
            HandlingMethodList = new List<HandlingMethodModel>();
            AbnormalReasonList = new List<AbnormalReasonModel>();
            CheckItemList = new List<CheckItemModel>();
            EquipmentList = new List<EquipmentModel>();
            ControlPointList = new List<ControlPointModel>();
            RouteList = new List<RouteModel>();
        }
    }
}
