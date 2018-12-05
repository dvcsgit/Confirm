using System;

namespace Utility
{
    /// <summary>
    /// DateString => yyyyMMdd
    /// DateStringWithSeparator => yyyy{0}MM{1}dd
    /// DateTimeStringWithSeperator => yyyy{0}MM{0}dd HH{1}mm{1}ss
    /// DateTimeString => yyyyMMddHHmmss
    /// TimeString => HHmmss
    /// TimeStringWithSeperator => HH{0}mm{0}ss
    /// </summary>
    public class DateTimeHelper
    {
        /// <summary>
        /// yyyyMMdd => yyyy{0}MM{0}dd
        /// </summary>
        public static string DateString2DateStringWithSeparator(string Input)
        {
            string output = string.Empty;

            try
            {
                output = DateString2DateTime(Input).Value.ToString(Define.DateTimeFormat_DateStringWithSeperator);
            }
            catch
            {
                output = string.Empty;
            }

            return output;
        }

        /// <summary>
        /// yyyyMMdd => DateTime
        /// </summary>
        public static DateTime? DateString2DateTime(string Input)
        {
            DateTime? output = null;

            try
            {
                output = new DateTime(int.Parse(Input.Substring(0, 4)),
                                      int.Parse(Input.Substring(4, 2)),
                                      int.Parse(Input.Substring(6, 2)));
            }
            catch
            {
                output = null;
            }

            return output;
        }

        /// <summary>
        /// yyyy{0}MM{0}dd => yyyyMMdd
        /// </summary>
        public static string DateStringWithSeperator2DateString(string Input)
        {
            string output = string.Empty;

            try
            {
                output = DateStringWithSeperator2DateTime(Input).Value.ToString(Define.DateTimeFormat_DateString);
            }
            catch
            {
                output = string.Empty;
            }

            return output;
        }

        /// <summary>
        /// yyyy{0}MM{0}dd => DateTime
        /// </summary>
        public static DateTime? DateStringWithSeperator2DateTime(string Input)
        {
            DateTime? output = null;

            try
            {
                string[] temp = Input.Split(Define.DateTimeFormat_DateSeperator);

                output = new DateTime(int.Parse(temp[0]),
                                      int.Parse(temp[1]),
                                      int.Parse(temp[2]));
            }
            catch
            {
                output = null;
            }

            return output;
        }

        /// <summary>
        /// DateTime => yyyy{0}MM{0}dd
        /// </summary>
        public static string DateTime2DateStringWithSeperator(DateTime? Input)
        {
            string output = string.Empty;

            try
            {
                output = Input.Value.ToString(Define.DateTimeFormat_DateStringWithSeperator);
            }
            catch
            {
                output = string.Empty;
            }

            return output;
        }

        /// <summary>
        /// DateTime => yyyyMMdd
        /// </summary>
        public static string DateTime2DateString(DateTime? Input)
        {
            string output = string.Empty;

            try
            {
                output = Input.Value.ToString(Define.DateTimeFormat_DateString);
            }
            catch
            {
                output = string.Empty;
            }

            return output;
        }

        /// <summary>
        /// DateTime => yyyy{0}MM{0}dd HH{1}mm{1}ss
        /// </summary>
        public static string DateTime2DateTimeStringWithSeperator(DateTime? Input)
        {
            string output = string.Empty;

            try
            {
                output = string.Format("{0} {1}", DateTime2DateStringWithSeperator(Input), DateTime2TimeStringWithSeperator(Input));
            }
            catch
            {
                output = string.Empty;
            }

            return output;
        }

        /// <summary>
        /// DateTime => yyyyMMddHHmmss
        /// </summary>
        public static string DateTime2DateTimeString(DateTime? Input)
        {
            string output = string.Empty;

            try
            {
                output = string.Format("{0}{1}", DateTime2DateString(Input), DateTime2TimeString(Input));
            }
            catch
            {
                output = string.Empty;
            }

            return output;
        }

        /// <summary>
        /// yyyyMMdd, HHmmss => DateTime
        /// </summary>
        /// <returns></returns>
        public static DateTime? DateTimeString2DateTime(string Date, string Time)
        {
            DateTime? output = null;

            try
            {
                if (string.IsNullOrEmpty(Time))
                {
                    Time = "000000";
                }
                else
                {
                    Time = Time.PadRight(6, '0');
                }

                output = new DateTime(int.Parse(Date.Substring(0, 4)),
                                      int.Parse(Date.Substring(4, 2)),
                                      int.Parse(Date.Substring(6, 2)),
                                      int.Parse(Time.Substring(0, 2)),
                                      int.Parse(Time.Substring(2, 2)),
                                      int.Parse(Time.Substring(4, 2)));
            }
            catch
            {
                output = null;
            }

            return output;
        }

        /// <summary>
        /// DateTime => HHmmss
        /// </summary>
        public static string DateTime2TimeString(DateTime? Input)
        {
            string output = string.Empty;

            try
            {
                output = Input.Value.ToString(Define.DateTimeFormat_TimeString);
            }
            catch
            {
                output = string.Empty;
            }

            return output;
        }

        /// <summary>
        /// DateTime => HH{0}mm{0}ss
        /// </summary>
        public static string DateTime2TimeStringWithSeperator(DateTime? Input)
        {
            string output = string.Empty;

            try
            {
                output = Input.Value.ToString(Define.DateTimeFormat_TimeStringWithSeperator);
            }
            catch
            {
                output = string.Empty;
            }

            return output;
        }

        /// <summary>
        /// HHmmss => HH{1}mm{1}ss
        /// </summary>
        /// <param name="Input"></param>
        public static string TimeString2TimeStringWithSeperator(string Input)
        {
            string output = string.Empty;

            try
            {
                output = DateTime2TimeStringWithSeperator(DateTimeString2DateTime(DateTime2DateString(DateTime.Today), Input));
            }
            catch
            {
                output = string.Empty;
            }

            return output;
        }
    }
}
