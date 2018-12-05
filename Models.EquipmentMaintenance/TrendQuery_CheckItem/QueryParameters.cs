using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;
namespace Models.EquipmentMaintenance.TrendQuery_CheckItem
{
    public class QueryParameters
    {
        public string CheckItemUniqueID { get; set; }

        public string ControlPoints { get; set; }

        public List<string> ControlPointList
        {
            get
            {
                if (!string.IsNullOrEmpty(ControlPoints))
                {
                    return JsonConvert.DeserializeObject<List<string>>(ControlPoints);
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public string Equipments { get; set; }

        public List<string> EquipmentList
        {
            get
            {
                if (!string.IsNullOrEmpty(Equipments))
                {
                    return JsonConvert.DeserializeObject<List<string>>(Equipments);
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public string EquipmentParts { get; set; }

        public List<string> EquipmentPartList
        {
            get
            {
                if (!string.IsNullOrEmpty(EquipmentParts))
                {
                    return JsonConvert.DeserializeObject<List<string>>(EquipmentParts);
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        [Display(Name = "BeginDate", ResourceType = typeof(Resources.Resource))]
        public string BeginDateString { get; set; }

        public string BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(BeginDateString);
            }
        }

        [Display(Name = "EndDate", ResourceType = typeof(Resources.Resource))]
        public string EndDateString { get; set; }

        public string EndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(EndDateString);
            }
        }
    }
}
