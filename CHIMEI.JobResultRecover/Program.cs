using DataAccess.EquipmentMaintenance;
using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIMEI.JobResultRecover
{
    class Program
    {
        static void Main(string[] args)
        {
            var jobUniqueIDList = new List<string>() 
            { 
                "080e03cd-e337-4481-ba70-dce86c2de9f6",
                "09a138df-ee49-48df-accc-9eec0ea978e4",
                "2856fdfe-0698-47f7-bb13-9bf0e161fbbd",
                "3056026b-20eb-48e9-b86a-c2930fa1d129",
                "30630edc-3207-4955-bf93-3c84b910fdf7",
                "36ebc4cf-1a70-4781-b744-3a108990d508",
                "3836995a-de77-4a70-b83c-c13e4ce5817c",
                "3b85afda-8f61-4273-9c63-c3ac546af3a4",
                "459e7773-a108-4bdf-b762-5a8e8da2b594",
                "4a469e1f-6f58-4abd-9b09-b14d22cd2d28",
                "4a8cbf6b-dd53-43ac-bf00-8113b53fe05f",
                "50ac4fb1-56c8-4a7e-af6c-950005010140",
                "53afa5f4-3733-4028-8de0-d579d1ccfe40",
                "5755aac2-cc5d-4296-b3c8-cada3784f6b7",
                "5fc72c12-4a87-41b3-a113-935911826173",
                "8f9f2ae5-eaab-4487-8d52-e54e0f97634e",
                "93802bb3-132e-4ea3-a3ed-bd7c34a232bf",
                "98f67341-c69e-4d6c-8fa4-7b3b3424b22b",
                "a6c86bb4-d75b-43ee-967a-483ead72005d",
                "bf19d8a9-727c-4d2e-9025-817e918da94f",
                "d90cbd3c-7a2b-4989-90cd-8412c138f713",
                "dd8cd2fd-f18f-4007-8654-fb4e0726ecf0",
                "e30c3b88-5e15-41bc-a319-57f0b1d17351",
                "e4778e29-9802-4b83-bc6c-8043a43d130c",
                "eaa78e42-f379-4624-887a-5fead695fe6f",
                "f331ef1b-67cc-44cc-a965-c26198ef0bb0"
            };

            using (EDbEntities db = new EDbEntities())
            {
                foreach (var jobUniqueID in jobUniqueIDList)
                {
                    var jobResultList = db.JobResult.Where(x => x.JobUniqueID == jobUniqueID).ToList();

                    var description = jobResultList.First().Description;

                    var beginDate = description.Substring(description.Length - 8);

                    var correct = jobResultList.First(x => x.BeginDate == beginDate);

                    var wrong = jobResultList.Where(x => x.UniqueID != correct.UniqueID).ToList();

                    foreach (var w in wrong)
                    {
                        db.Database.ExecuteSqlCommand(string.Format("UPDATE ArriveRecord SET JobResultUniqueID = '{0}' WHERE JobResultUniqueID = '{1}'", correct.UniqueID, w.UniqueID));

                        db.JobResult.Remove(w);
                    }

                    db.SaveChanges();

                    JobResultDataAccessor.Refresh(correct.UniqueID, jobUniqueID, correct.BeginDate, correct.EndDate);
                }
            }
        }
    }
}
