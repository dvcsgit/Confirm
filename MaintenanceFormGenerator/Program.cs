using System;
using Utility;

namespace MaintenanceFormGenerator
{
    public class Program
    {
        static void Main(string[] args)
        {
            Logger.Log("MaintenanceFormGenerator Begin...");

            Logger.SubSeperator();

            DateTime beginTime = DateTime.Now;

            using (Generator generator = new Generator())
            {
                generator.Generate();
            }

            Logger.Log("MaintenanceFormGenerator Finished...");

            DateTime finishedTime = DateTime.Now;

            Logger.TimeSpan(beginTime, finishedTime);

            Logger.Seperator();
        }
    }
}
