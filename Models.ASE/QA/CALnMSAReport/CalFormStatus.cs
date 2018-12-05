using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.CALnMSAReport
{
    public class CalFormStatus
    {
        public string Status { get; set; }

        private string CalibrateType { get; set; }

        private DateTime EstCalibrateDate { get; set; }

        private string LastStep { get; set; }

        private string LastStepDescription
        {
            get
            {
                if (!string.IsNullOrEmpty(LastStep))
                {
                    if (LastStep == "1")
                    {
                        return "收件";
                    }
                    else if (LastStep == "2")
                    {
                        return "送件";
                    }
                    else if (LastStep == "3")
                    {
                        return "回件";
                    }
                    else if (LastStep == "4")
                    {
                        return "發件";
                    }
                    else if (LastStep == "5")
                    {
                        return "到廠校驗";
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _Status
        {
            get
            {
                if (Status == "1")
                {
                    if (CalibrateType == "IF")
                    {
                        if (DateTime.Compare(DateTime.Today, EstCalibrateDate) > 0)
                        {
                            return "2";
                        }
                        else
                        {
                            return Status;
                        }
                    }
                    else if (CalibrateType == "IL")
                    {
                        if (!string.IsNullOrEmpty(LastStep))
                        {
                            return Status;
                        }
                        else
                        {
                            if (DateTime.Compare(DateTime.Today, EstCalibrateDate) > 0)
                            {
                                return "2";
                            }
                            else
                            {
                                return Status;
                            }
                        }
                    }
                    else if (CalibrateType == "EF")
                    {
                        if (!string.IsNullOrEmpty(LastStep))
                        {
                            return Status;
                        }
                        else
                        {
                            if (DateTime.Compare(DateTime.Today, EstCalibrateDate) > 0)
                            {
                                return "2";
                            }
                            else
                            {
                                return Status;
                            }
                        }
                    }
                    else if (CalibrateType == "EL")
                    {
                        if (!string.IsNullOrEmpty(LastStep))
                        {
                            return Status;
                        }
                        else
                        {
                            if (DateTime.Compare(DateTime.Today, EstCalibrateDate) > 0)
                            {
                                return "2";
                            }
                            else
                            {
                                return Status;
                            }
                        }
                    }
                    else
                    {
                        return Status;
                    }
                }
                else
                {
                    return Status;
                }
            }
        }

        public string Display
        {
            get
            {
                if (_Status == "0")
                {
                    return Resources.Resource.CalibrationFormStatus_0;
                }
                else if (_Status == "1")
                {
                    if (!string.IsNullOrEmpty(LastStepDescription))
                    {
                        return string.Format("{0}({1})", Resources.Resource.CalibrationFormStatus_1, LastStepDescription);
                    }
                    else
                    {
                        return Resources.Resource.CalibrationFormStatus_1;
                    }
                }
                else if (_Status == "2")
                {
                    if (!string.IsNullOrEmpty(LastStepDescription))
                    {
                        return string.Format("{0}({1})", Resources.Resource.CalibrationFormStatus_2, LastStepDescription);
                    }
                    else
                    {
                        return Resources.Resource.CalibrationFormStatus_2;
                    }
                }
                else if (_Status == "3")
                {
                    return Resources.Resource.CalibrationFormStatus_3;
                }
                else if (_Status == "4")
                {
                    return Resources.Resource.CalibrationFormStatus_4;
                }
                else if (_Status == "5")
                {
                    return Resources.Resource.CalibrationFormStatus_5;
                }
                else if (_Status == "6")
                {
                    return "文審退回";
                }
                else if (_Status == "7")
                {
                    return Resources.Resource.CalibrationFormStatus_7;
                }
                else if (_Status == "8")
                {
                    return Resources.Resource.CalibrationFormStatus_8;
                }
                else if (_Status == "9")
                {
                    return "取消立案";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string LabelClass
        {
            get
            {
                switch (_Status)
                {
                    case "0":
                        return "label-warning";
                    case "1":
                        return "label-primary";
                    case "2":
                        return "label-danger";
                    case "3":
                        return "label-purple";
                    case "4":
                        return "label-danger";
                    case "5":
                        return "label-success";
                    case "6":
                        return "label-danger";
                    case "7":
                        return "label-warning";
                    case "8":
                        return "label-warning";
                    case "9":
                        return "label-grey";
                    default:
                        return "";
                }
            }
        }

        public CalFormStatus(string Status, string CalibrateType, DateTime EstCalibrateDate, string LastStep)
        {
            this.Status = Status;
            this.CalibrateType = CalibrateType;
            this.EstCalibrateDate = EstCalibrateDate;
            this.LastStep = LastStep;
        }
    }
}
