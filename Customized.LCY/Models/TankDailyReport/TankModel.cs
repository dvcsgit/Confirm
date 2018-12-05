using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.LCY.Models.TankDailyReport
{
    public class TankModel
    {
        public string ID { get; set; }

        public double? _UpperLimit { get; set; }
        public string UpperLimit
        {
            get
            {
                if (_UpperLimit.HasValue)
                {
                    return _UpperLimit.Value.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public double? _Level { get; set; }
        public string Level
        {
            get
            {
                if (_Level.HasValue)
                {
                    return _Level.Value.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _Content { get; set; }
        public string Content
        {
            get
            {
                if (!string.IsNullOrEmpty(_Content))
                {
                    return _Content;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public double? _Temperature { get; set; }
        public string Temperature
        {
            get
            {
                if (_Temperature.HasValue)
                {
                    return _Temperature.Value.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _V1 { get; set; }
        public string V1
        {
            get
            {
                if (_V1 == "開啟")
                {
                    return "O";
                }
                else if (_V1 == "關閉")
                {
                    return string.Empty;
                }
                else if (_V1 == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _V2 { get; set; }
        public string V2
        {
            get
            {
                if (_V2 == "開啟")
                {
                    return "O";
                }
                else if (_V2 == "關閉")
                {
                    return string.Empty;
                }
                else if (_V2 == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _V3 { get; set; }
        public string V3
        {
            get
            {
                if (_V3 == "開啟")
                {
                    return "O";
                }
                else if (_V3 == "關閉")
                {
                    return string.Empty;
                }
                else if (_V3 == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _V4 { get; set; }
        public string V4
        {
            get
            {
                if (_V4 == "開啟")
                {
                    return "O";
                }
                else if (_V4 == "關閉")
                {
                    return string.Empty;
                }
                else if (_V4 == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _V5 { get; set; }
        public string V5
        {
            get
            {
                if (_V5 == "開啟")
                {
                    return "O";
                }
                else if (_V5 == "關閉")
                {
                    return string.Empty;
                }
                else if (_V5 == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _V6 { get; set; }
        public string V6
        {
            get
            {
                if (_V6 == "開啟")
                {
                    return "O";
                }
                else if (_V6 == "關閉")
                {
                    return string.Empty;
                }
                else if (_V6 == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _V7 { get; set; }
        public string V7
        {
            get
            {
                if (_V7 == "開啟")
                {
                    return "O";
                }
                else if (_V7 == "關閉")
                {
                    return string.Empty;
                }
                else if (_V7 == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _V8 { get; set; }
        public string V8
        {
            get
            {
                if (_V8 == "開啟")
                {
                    return "O";
                }
                else if (_V8 == "關閉")
                {
                    return string.Empty;
                }
                else if (_V8 == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _V9 { get; set; }
        public string V9
        {
            get
            {
                if (_V9 == "開啟")
                {
                    return "O";
                }
                else if (_V9 == "關閉")
                {
                    return string.Empty;
                }
                else if (_V9 == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _V10 { get; set; }
        public string V10
        {
            get
            {
                if (_V10 == "開啟")
                {
                    return "O";
                }
                else if (_V10 == "關閉")
                {
                    return string.Empty;
                }
                else if (_V10 == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _V11 { get; set; }
        public string V11
        {
            get
            {
                if (_V11 == "開啟")
                {
                    return "O";
                }
                else if (_V11 == "關閉")
                {
                    return string.Empty;
                }
                else if (_V11 == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _V12 { get; set; }
        public string V12
        {
            get
            {
                if (_V12 == "開啟")
                {
                    return "O";
                }
                else if (_V12 == "關閉")
                {
                    return string.Empty;
                }
                else if (_V12 == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _V13 { get; set; }
        public string V13
        {
            get
            {
                if (_V13 == "開啟")
                {
                    return "O";
                }
                else if (_V13 == "關閉")
                {
                    return string.Empty;
                }
                else if (_V13 == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _N2In { get; set; }
        public string N2In
        {
            get
            {
                if (_N2In == "開啟")
                {
                    return "O";
                }
                else if (_N2In == "關閉")
                {
                    return string.Empty;
                }
                else if (_N2In == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string _N2Out { get; set; }
        public string N2Out
        {
            get
            {
                if (_N2Out == "開啟")
                {
                    return "O";
                }
                else if (_N2Out == "關閉")
                {
                    return string.Empty;
                }
                else if (_N2Out == "異常")
                {
                    return "X";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public double? _Pressure { get; set; }
        public string Pressure
        {
            get
            {
                if (_Pressure.HasValue)
                {
                    return _Pressure.Value.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public double? _Recycle { get; set; }
        public string Recycle
        {
            get
            {
                if (_Recycle.HasValue)
                {
                    return _Recycle.Value.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Pipe { get; set; }

        public string _HaveExchange { get; set; }

        public bool HaveExchange
        {
            get
            {
                if (_HaveExchange == "是")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
