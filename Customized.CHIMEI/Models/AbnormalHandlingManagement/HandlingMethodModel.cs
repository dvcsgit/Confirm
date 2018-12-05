namespace Customized.CHIMEI.Models.AbnormalHandlingManagement
{
    public class HandlingMethodModel
    {
        public string Description { get; set; }

        public string Remark { get; set; }

        public string Display
        {
            get
            {
                if (string.IsNullOrEmpty(Remark))
                {
                    return Description;
                }
                else
                {
                    return Remark;
                }
            }
        }
    }
}
