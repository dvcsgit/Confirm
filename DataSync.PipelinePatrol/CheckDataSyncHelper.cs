using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DbEntity.MSSQL.PipelinePatrol;
using Models.PipelinePatrol.CheckDataSync;
using Newtonsoft.Json;
using Utility;
using Utility.Models;
using DataSync.PipelinePatrol.Extensions;

namespace DataSync.PipelinePatrol
{
    public class CheckDataSyncHelper
    {
        static object lockMe = new object();

        public static RequestResult GetVHNOList()
        {
            RequestResult result = new RequestResult();
            CheckDataSyncModel viewModel = new CheckDataSyncModel();
            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    //抓沒值的單號到前端
                    Dictionary<string,string> constHashMap = new Dictionary<string,string>();
                    var const_vhnoList = db.Construction.Where(x => !x.ClosedTime.HasValue)
                        .AsEnumerable()
                        .Select(x => new ConstHash { VHNO = x.VHNO, InspectionUniqueID =  x.InspectionUniqueID  })
                        .ToList();

                    
                    Parallel.ForEach(const_vhnoList, row =>
                    {
                        //string json = JsonConvert.SerializeObject(row);
                        
                        string json = JsonConvert.SerializeObject(row,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
                        //var sha1 = row.GetMD5Hash();//.GetSHA1Hash();
                        string json_hash = json.GetMD5HashForJAVA();
                        lock (lockMe)
                        {
                            //Statements
                            Logger.Log(json);
                            constHashMap.Add(row.VHNO, json_hash);
                        }
                    });
                    viewModel.CheckDataSync.Add(new CheckDataSyncItem { MarkerFormConsts = "Construction", VHNOList = constHashMap });



                    Dictionary<string, string> inspectHashMap = new Dictionary<string, string>();
                    var inspect_vhnoList = db.Inspection.Where(x => !x.ClosedTime.HasValue)
                        .AsEnumerable()
                        .Select(x => x.VHNO)
                        .ToList();
                    inspectHashMap = inspect_vhnoList.ToDictionary(x => x, x => x);
                    viewModel.CheckDataSync.Add(new CheckDataSyncItem { MarkerFormConsts = "Inspection", VHNOList = inspectHashMap });



                    Dictionary<string, string> pipelineHashMap = new Dictionary<string, string>();
                    var pipeline_vhnoList = db.PipelineAbnormal.Where(x => !x.ClosedTime.HasValue)
                        .Select(x => x.VHNO)
                        .ToList();
                    pipelineHashMap = pipeline_vhnoList.ToDictionary(x => x, x => x);
                    viewModel.CheckDataSync.Add(new CheckDataSyncItem { MarkerFormConsts = "PipelineAbnormal", VHNOList = pipelineHashMap });

                    Dictionary<string, string> dialogHashMap = new Dictionary<string, string>();
                    var dialog_uniqueid_list = db.Dialog.Select(x => x.UniqueID).ToList();
                    dialogHashMap = dialog_uniqueid_list.ToDictionary(x => x, x => x);
                    viewModel.CheckDataSync.Add(new CheckDataSyncItem { MarkerFormConsts = "ChatDialog", VHNOList = dialogHashMap });

                    result.ReturnData(viewModel);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public class ConstHash
        {
            [JsonProperty(Order = 2)]
            public string VHNO { get; set; }

            [JsonProperty(Order = 1)]
            public string InspectionUniqueID { get; set; }
        }

   
    }
}
