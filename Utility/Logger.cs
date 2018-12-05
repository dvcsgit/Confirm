using System;
using System.IO;
using System.Text;
using System.Reflection;
using Utility.Models;

namespace Utility
{
    public class Logger
    {
        public static void Log(MethodBase Method)
        {
            try
            {
                if (Method != null)
                {
                    Log(string.Format("{0}.{1}", Method.ReflectedType.FullName, Method.Name));
                }
            }
            catch(Exception ex)
            {
                Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void Log(MethodBase Method, Exception Exception)
        {
            Log(new Error(Method, Exception));
        }

        public static void Log(MethodBase Method, string Message)
        {
            Log(new Error(Method, Message));
        }

        public static void Log(Error Error)
        {
            try
            {
                if (!string.IsNullOrEmpty(Error.MethodName))
                {
                    Log(Error.MethodName);
                }

                foreach (var msg in Error.ErrorMessageList)
                {
                    Log(msg);
                }
            }
            catch
            {

            }
        }

        public static void Log(string Message)
        {
            try
            {
                using (var sw = new StreamWriter(Define.LogFile, true, Encoding.UTF8))
                {
                    sw.WriteLine(DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now) + " " + Message);

                    sw.Close();
                }
            }
            catch
            {

            }
        }

        public static void Seperator()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(Define.LogFile, true, Encoding.UTF8))
                {
                    sw.WriteLine("====================================================================================================");

                    sw.Close();
                }
            }
            catch
            {

            }
        }

        public static void SubSeperator()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(Define.LogFile, true, Encoding.UTF8))
                {
                    sw.WriteLine("----------------------------------------------------------------------------------------------------");

                    sw.Close();
                }
            }
            catch
            {

            }
        }

        public static void TimeSpan(DateTime BeginTime, DateTime FinishedTime)
        {
            try
            {
                var ts = FinishedTime - BeginTime;

                using (StreamWriter sw = new StreamWriter(Define.LogFile, true, Encoding.UTF8))
                {
                    sw.WriteLine("TimeSpan => " + ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0'));

                    sw.Close();
                }
            }
            catch
            { }
        }
    }
}
