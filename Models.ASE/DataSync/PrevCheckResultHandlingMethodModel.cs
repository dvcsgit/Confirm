namespace Models.ASE.DataSync
{
    public class PrevCheckResultHandlingMethodModel
    {
        public string Description { get; set; }

        public string Remark { get; set; }

        public string HandlingMethod
        {
            get
            {
                if (!string.IsNullOrEmpty(Remark))
                {
                    return string.Format("{0}({1})", Description, Remark);
                }
                else
                {
                    return Description;
                }
            }
        }
    }
}
