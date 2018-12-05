using System.Collections.Generic;

namespace Models.ASE.DataSync
{
    public class EmgContactModel
    {
        public string UniqueID { get; set; }

        public string Title { get; set; }

        public string Name { get; set; }

        public List<EmgContactTelModel> TelList { get; set; }

        public EmgContactModel()
        {
            TelList = new List<EmgContactTelModel>();
        }

        public override bool Equals(object Object)
        {
            return Equals(Object as EmgContactModel);
        }

        public override int GetHashCode()
        {
            return UniqueID.GetHashCode();
        }

        public bool Equals(EmgContactModel Model)
        {
            return UniqueID.Equals(Model.UniqueID);
        }
    }
}
