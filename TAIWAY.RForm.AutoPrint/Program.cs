using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace TAIWAY.RForm.AutoPrint
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Log("RForm.AutoPrint Begin...");

            Logger.SubSeperator();

            DateTime beginTime = DateTime.Now;

            using (PrintHelper helper = new PrintHelper())
            {
                helper.Print();
            }

            Logger.Log("RForm.AutoPrint Finished...");

            DateTime finishedTime = DateTime.Now;

            Logger.TimeSpan(beginTime, finishedTime);

            Logger.Seperator();
        }
    }
}
