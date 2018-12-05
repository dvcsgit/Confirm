using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace ASE.QA.VerifyReminder
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// 1:CalibrationApply
        /// 2:CalibrationNotify
        /// 3:CalibrationForm
        /// 4:MSANotify
        /// 5:MSAForm
        /// 6:ChangeForm
        static void Main(string[] args)
        {
            Logger.Log("VerifyReminder Begin...");

            Logger.SubSeperator();

            DateTime beginTime = DateTime.Now;

            using (Reminder reminder = new Reminder())
            {
                reminder.Remind(args[0]);
            }

            Logger.Log("VerifyReminder Finished...");

            DateTime finishedTime = DateTime.Now;

            Logger.TimeSpan(beginTime, finishedTime);

            Logger.Seperator();
        }
    }
}
