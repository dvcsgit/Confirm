using System.Collections.Generic;
namespace Models.ASE.QS.CheckListManagement
{
    public class QueryFormModel
    {
        public QueryParameters Parameters { get; set; }

        public List<FactoryModel> FactoryList { get; set; }

        public List<ShiftModel> ShiftList { get; set; }

        public QueryFormModel()
        {
            Parameters = new QueryParameters();
            FactoryList = new List<FactoryModel>();
            ShiftList = new List<ShiftModel>();
        }
    }
}
