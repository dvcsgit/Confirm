using System.Collections.Generic;

namespace Models.EquipmentMaintenance.DataSync
{
    public class AbnormalReasonModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public List<HandlingMethodModel> HandlingMethodList { get; set; }

        public AbnormalReasonModel()
        {
            HandlingMethodList = new List<HandlingMethodModel>();
        }

        public override bool Equals(object Object)
        {
            return Equals(Object as AbnormalReasonModel);
        }

        public override int GetHashCode()
        {
            return UniqueID.GetHashCode();
        }

        public bool Equals(AbnormalReasonModel Model)
        {
            return UniqueID.Equals(Model.UniqueID);
        }
    }
}
