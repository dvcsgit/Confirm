using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace CPDC.RENDA2CPDC
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Log("RENDA2CPDC Begin...");

            Logger.SubSeperator();

            DateTime beginTime = DateTime.Now;

            using (TransHelper helper = new TransHelper())
            {
                helper.Trans();
            }

            Logger.Log("RENDA2CPDC Finished...");

            DateTime finishedTime = DateTime.Now;

            Logger.TimeSpan(beginTime, finishedTime);

            Logger.Seperator();
        }
    }
}
