using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;
namespace Models.ASE.QS.CheckListManagement
{
    public class QueryParameters
    {
        public string BeginDateString { get; set; }

        public DateTime? BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(BeginDateString);
            }
        }

        public string EndDateString { get; set; }

        public DateTime? EndDate
        {
            get
            {
                var endDate = DateTimeHelper.DateStringWithSeperator2DateTime(EndDateString);

                if (endDate.HasValue)
                {
                    return DateTimeHelper.DateStringWithSeperator2DateTime(EndDateString).Value.AddDays(1);
                }
                else
                {
                    return null;
                }
            }
        }

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        public string Shifts { get; set; }

        public List<string> ShiftList
        {
            get
            {
                if (!string.IsNullOrEmpty(Shifts))
                {
                    return JsonConvert.DeserializeObject<List<string>>(Shifts);
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public string Factorys { get; set; }

        public List<string> FactoryList
        {
            get
            {
                if (!string.IsNullOrEmpty(Factorys))
                {
                    return JsonConvert.DeserializeObject<List<string>>(Factorys);
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public string FactoryDescription { get; set; }

        public string CarNo { get; set; }
    }
}
