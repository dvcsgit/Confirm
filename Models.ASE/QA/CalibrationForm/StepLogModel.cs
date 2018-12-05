using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationForm
{
    public class StepLogModel
    {
        public int Seq { get; set; }

        public string Step { get; set; }

        public string StepDescription
        {
            get
            {
                if (Step == "1")
                {
                    return "收件";
                }
                else if (Step == "2")
                {
                    return "送件";
                }
                else if (Step == "3")
                {
                    return "回件";
                }
                else if (Step == "4")
                {
                    return "發件";
                }
                else if (Step == "5")
                {
                    return "到廠校驗";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public DateTime Time { get; set; }

        public string TimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(Time);
            }
        }

        public string OwnerID { get; set; }

        public string OwnerName { get; set; }

        public string Owner
        {
            get
            {
                if (!string.IsNullOrEmpty(OwnerName))
                {
                    return string.Format("{0}/{1}", OwnerID, OwnerName);
                }
                else
                {

                    return OwnerID;
                }
            }
        }

        public string OwnerDisplay
        {
            get
            { 
                if(Step=="1")
                {
                    return string.Format("交件人員：{0}", Owner);
                }
                else if (Step == "2")
                {
                    return string.Format("送件人員：{0}", Owner);
                }
                else if (Step == "3")
                {
                    return string.Format("回件人員：{0}", Owner);
                }
                else if (Step == "4")
                {
                    return string.Format("取件人員：{0}", Owner);
                }
                else if (Step == "5")
                {
                    return string.Format("陪同人員：{0}", Owner);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string QAID { get; set; }

        public string QAName { get; set; }

        public string QA
        {
            get
            {
                if (!string.IsNullOrEmpty(QAName))
                {
                    return string.Format("{0}/{1}", QAID, QAName);
                }
                else
                {

                    return QAID;
                }
            }
        }

        public string QADisplay
        {
            get
            {
                if (Step == "1")
                {
                    return string.Format("收件人員：{0}", QA);
                }
                else if (Step == "2")
                {
                    return string.Format("送件人員：{0}", QA);
                }
                else if (Step == "3")
                {
                    return string.Format("回件人員：{0}", QA);
                }
                else if (Step == "4")
                {
                    return string.Format("發件人員：{0}", QA);
                }
                else if (Step == "5")
                {
                    return string.Format("陪同人員：{0}", QA);
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
