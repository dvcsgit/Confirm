using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.AbnormalForm
{
    public class LogModel
    {
        public string Role
        {
            get
            {
                if (FlowSeq == 1)
                {
                    return Resources.Resource.EquipmentOwner;
                }
                else if (FlowSeq == 2)
                {
                    return Resources.Resource.PE;
                }
                else if (FlowSeq == 3)
                {
                    return Resources.Resource.PEManager;
                }
                else if (FlowSeq == 4)
                {
                    return Resources.Resource.QA;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public int Seq { get; set; }

        public int FlowSeq { get; set; }

        public string UserID { get; set; }

        public string UserName { get; set; }

        public string User
        {
            get
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    return string.Format("{0}/{1}", UserID, UserName);
                }
                else
                {
                    return UserID;
                }
            }
        }

        public DateTime NotifyTime { get; set; }

        public string NotifyTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(NotifyTime);
            }
        }

        public DateTime? VerifyTime { get; set; }

        public string VerifyTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(VerifyTime);
            }
        }

        public string VerifyResult { get; set; }

        public string VerifyResultDescription
        {
            get
            {
                if (!string.IsNullOrEmpty(VerifyResult))
                {
                    if (VerifyResult == "Y")
                    {
                        if (FlowSeq == 1)
                        {
                            return "呈核";
                        }
                        else
                        {
                            return "核准";
                        }
                    }
                    else
                    {
                        return "退回";
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string VerifyComment { get; set; }
    }
}
