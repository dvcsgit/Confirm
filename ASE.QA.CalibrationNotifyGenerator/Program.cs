﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace ASE.QA.CalibrationNotifyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Log("CalibrationNotifyGenerator Begin...");

            Logger.SubSeperator();

            DateTime beginTime = DateTime.Now;

            using (Generator generator = new Generator())
            {
                generator.Generate();
            }

            Logger.Log("CalibrationNotifyGenerator Finished...");

            DateTime finishedTime = DateTime.Now;

            Logger.TimeSpan(beginTime, finishedTime);

            Logger.Seperator();
        }
    }
}
