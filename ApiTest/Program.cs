using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;
using WebApi.CHIMEI.E.DataAccess;
using WebApi.CHIMEI.E.Models;

namespace ApiTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.SubSeperator();

            DateTime beginTime = DateTime.Now;

            using (DownloadHelper helper = new DownloadHelper())
            {
                var model = new DownloadFormModel() { CheckDate = "20180509" };

                model.Parameters.Add(new DownloadParameters()
                {
                    IsExceptChecked = true,
                    JobUniqueID = "888bcc90-4bb2-4d04-87a4-f9c2a8eb9d55"
                });

                RequestResult result = helper.Generate(model);
            }

            DateTime finishedTime = DateTime.Now;

            Logger.TimeSpan(beginTime, finishedTime);
        }
    }
}
