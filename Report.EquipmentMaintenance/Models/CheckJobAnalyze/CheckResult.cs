using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.CheckJobAnalyze
{
   public  class CheckAllResult
    {
       /// <summary>
       /// 派工作业
       /// </summary>
       public string JobDescription { get; set; }

       /// <summary>
       /// 详细信息
       /// </summary>
       public List<string> Result { get;set; }

       public CheckAllResult()
       {
          Result=new List<string>();
        }


    }
}
