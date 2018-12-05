using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using DbEntity.MSSQL.TruckPatrol;
using DbEntity.MSSQL.PipelinePatrol;
using DbEntity.MSSQL.GuardPatrol;

namespace DataAccess
{
    public class DeleteHelper
    {
        public static void AbnormalReason(EDbEntities EDB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                AbnormalReason(EDB, uniqueID);
            }
        }

        public static void AbnormalReason(EDbEntities EDB, string UniqueID)
        {
            //AbnormalReason
            EDB.AbnormalReason.Remove(EDB.AbnormalReason.First(x => x.UniqueID == UniqueID));

            //AbnormalReasonHandlingMethod
            EDB.AbnormalReasonHandlingMethod.RemoveRange(EDB.AbnormalReasonHandlingMethod.Where(x => x.AbnormalReasonUniqueID == UniqueID).ToList());

            //CheckItemAbnormalReason
            EDB.CheckItemAbnormalReason.RemoveRange(EDB.CheckItemAbnormalReason.Where(x => x.AbnormalReasonUniqueID == UniqueID).ToList());
        }

        public static void AbnormalReason(TDbEntities TDB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                AbnormalReason(TDB, uniqueID);
            }
        }

        public static void AbnormalReason(TDbEntities TDB, string UniqueID)
        {
            //AbnormalReason
            TDB.AbnormalReason.Remove(TDB.AbnormalReason.First(x => x.UniqueID == UniqueID));

            //AbnormalReasonHandlingMethod
            TDB.AbnormalReasonHandlingMethod.RemoveRange(TDB.AbnormalReasonHandlingMethod.Where(x => x.AbnormalReasonUniqueID == UniqueID).ToList());

            //CheckItemAbnormalReason
            TDB.CheckItemAbnormalReason.RemoveRange(TDB.CheckItemAbnormalReason.Where(x => x.AbnormalReasonUniqueID == UniqueID).ToList());
        }

        public static void CheckItem(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //CheckItem
                DB.CheckItem.Remove(DB.CheckItem.First(x => x.UniqueID == uniqueID));

                //CheckItemAbnormalReason
                DB.CheckItemAbnormalReason.RemoveRange(DB.CheckItemAbnormalReason.Where(x => x.CheckItemUniqueID == uniqueID).ToList());

                //CheckItemFeelOption
                DB.CheckItemFeelOption.RemoveRange(DB.CheckItemFeelOption.Where(x => x.CheckItemUniqueID == uniqueID).ToList());

                //ControlPointCheckItem
                DB.ControlPointCheckItem.RemoveRange(DB.ControlPointCheckItem.Where(x => x.CheckItemUniqueID == uniqueID).ToList());

                //EquipmentCheckItem
                DB.EquipmentCheckItem.RemoveRange(DB.EquipmentCheckItem.Where(x => x.CheckItemUniqueID == uniqueID).ToList());

                //RouteControlPointCheckItem
                DB.RouteControlPointCheckItem.RemoveRange(DB.RouteControlPointCheckItem.Where(x => x.CheckItemUniqueID == uniqueID).ToList());

                //RouteEquipmentCheckItem
                DB.RouteEquipmentCheckItem.RemoveRange(DB.RouteEquipmentCheckItem.Where(x => x.CheckItemUniqueID == uniqueID).ToList());

                //JobControlPointCheckItem
                DB.JobControlPointCheckItem.RemoveRange(DB.JobControlPointCheckItem.Where(x => x.CheckItemUniqueID == uniqueID).ToList());

                //JobEquipmentCheckItem
                DB.JobEquipmentCheckItem.RemoveRange(DB.JobEquipmentCheckItem.Where(x => x.CheckItemUniqueID == uniqueID).ToList());
            }
        }

        public static void CheckItem(TDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //CheckItem
                DB.CheckItem.Remove(DB.CheckItem.First(x => x.UniqueID == uniqueID));

                //CheckItemAbnormalReason
                DB.CheckItemAbnormalReason.RemoveRange(DB.CheckItemAbnormalReason.Where(x => x.CheckItemUniqueID == uniqueID).ToList());

                //CheckItemFeelOption
                DB.CheckItemFeelOption.RemoveRange(DB.CheckItemFeelOption.Where(x => x.CheckItemUniqueID == uniqueID).ToList());

                //ControlPointCheckItem
                DB.ControlPointCheckItem.RemoveRange(DB.ControlPointCheckItem.Where(x => x.CheckItemUniqueID == uniqueID).ToList());
            }
        }

        public static void ControlPoint(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //ControlPoint
                DB.ControlPoint.Remove(DB.ControlPoint.First(x => x.UniqueID == uniqueID));

                //ControlPointCheckItem
                DB.ControlPointCheckItem.RemoveRange(DB.ControlPointCheckItem.Where(x => x.ControlPointUniqueID == uniqueID).ToList());

                //JobControlPoint
                DB.JobControlPoint.RemoveRange(DB.JobControlPoint.Where(x => x.ControlPointUniqueID == uniqueID).ToList());

                //JobControlPointCheckItem
                DB.JobControlPointCheckItem.RemoveRange(DB.JobControlPointCheckItem.Where(x => x.ControlPointUniqueID == uniqueID).ToList());

                //JobEquipment
                DB.JobEquipment.RemoveRange(DB.JobEquipment.Where(x => x.ControlPointUniqueID == uniqueID).ToList());

                //JobEquipmentCheckItem
                DB.JobEquipmentCheckItem.RemoveRange(DB.JobEquipmentCheckItem.Where(x => x.ControlPointUniqueID == uniqueID).ToList());

                //RouteControlPoint
                DB.RouteControlPoint.RemoveRange(DB.RouteControlPoint.Where(x => x.ControlPointUniqueID == uniqueID).ToList());

                //RouteControlPointCheckItem
                DB.RouteControlPointCheckItem.RemoveRange(DB.RouteControlPointCheckItem.Where(x => x.ControlPointUniqueID == uniqueID).ToList());

                //RouteEquipment
                DB.RouteEquipment.RemoveRange(DB.RouteEquipment.Where(x => x.ControlPointUniqueID == uniqueID).ToList());

                //RouteEquipmentCheckItem
                DB.RouteEquipmentCheckItem.RemoveRange(DB.RouteEquipmentCheckItem.Where(x => x.ControlPointUniqueID == uniqueID).ToList());
            }
        }

        public static void Equipment(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //Equipment
                DB.Equipment.Remove(DB.Equipment.First(x => x.UniqueID == uniqueID));

                //EquipmentCheckItem
                DB.EquipmentCheckItem.RemoveRange(DB.EquipmentCheckItem.Where(x => x.EquipmentUniqueID == uniqueID).ToList());

                //EquipmentMaterial
                DB.EquipmentMaterial.RemoveRange(DB.EquipmentMaterial.Where(x => x.EquipmentUniqueID == uniqueID).ToList());

                //EquipmentPart
                DB.EquipmentPart.RemoveRange(DB.EquipmentPart.Where(x => x.EquipmentUniqueID == uniqueID).ToList());

                //EquipmentSpecValue
                DB.EquipmentSpecValue.RemoveRange(DB.EquipmentSpecValue.Where(x => x.EquipmentUniqueID == uniqueID).ToList());

                //EquipmentStandard
                DB.EquipmentStandard.RemoveRange(DB.EquipmentStandard.Where(x => x.EquipmentUniqueID == uniqueID).ToList());

                //JobEquipment
                DB.JobEquipment.RemoveRange(DB.JobEquipment.Where(x => x.EquipmentUniqueID == uniqueID).ToList());

                //JobEquipmentCheckItem
                DB.JobEquipmentCheckItem.RemoveRange(DB.JobEquipmentCheckItem.Where(x => x.EquipmentUniqueID == uniqueID).ToList());

                

                //MJobEquipment
                DB.MJobEquipment.RemoveRange(DB.MJobEquipment.Where(x => x.EquipmentUniqueID == uniqueID).ToList());

                //MJobEquipmentStandard
                DB.MJobEquipmentStandard.RemoveRange(DB.MJobEquipmentStandard.Where(x => x.EquipmentUniqueID == uniqueID).ToList());

                //RouteEquipment
                DB.RouteEquipment.RemoveRange(DB.RouteEquipment.Where(x => x.EquipmentUniqueID == uniqueID).ToList());

                //RouteEquipmentCheckItem
                DB.RouteEquipmentCheckItem.RemoveRange(DB.RouteEquipmentCheckItem.Where(x => x.EquipmentUniqueID == uniqueID).ToList());

                Folder(DB, DB.Folder.Where(x => x.EquipmentUniqueID == uniqueID).Select(x => x.UniqueID).ToList());

                File(DB, DB.File.Where(x => x.EquipmentUniqueID == uniqueID).Select(x => x.UniqueID).ToList());
            }
        }

        public static void EquipmentSpec(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //EquipmentSpec
                DB.EquipmentSpec.Remove(DB.EquipmentSpec.First(x => x.UniqueID == uniqueID));

                //EquipmentSpecOption
                DB.EquipmentSpecOption.RemoveRange(DB.EquipmentSpecOption.Where(x => x.SpecUniqueID == uniqueID).ToList());

                //EquipmentSpecValue
                DB.EquipmentSpecValue.RemoveRange(DB.EquipmentSpecValue.Where(x => x.SpecUniqueID == uniqueID).ToList());
            }
        }

        public static void File(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                var file = DB.File.First(x => x.UniqueID == uniqueID);

                DB.File.Remove(file);

                try
                {
                    var filePath = System.IO.Path.Combine(Config.EquipmentMaintenanceFileFolderPath, file.UniqueID + "." + file.Extension);
                    var zipFilePath = System.IO.Path.Combine(Config.EquipmentMaintenanceFileFolderPath, file.UniqueID + ".zip");

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    if (System.IO.File.Exists(zipFilePath))
                    {
                        System.IO.File.Delete(zipFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(MethodBase.GetCurrentMethod(), ex);
                }
            }
        }

        public static void Folder(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                Folder(DB, uniqueID);
            }
        }

        public static void Folder(EDbEntities DB, string UniqueID)
        {
            DB.Folder.Remove(DB.Folder.First(x => x.UniqueID == UniqueID));

            Folder(DB, DB.Folder.Where(x => x.FolderUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

            File(DB, DB.File.Where(x => x.FolderUniqueID == UniqueID).Select(x => x.UniqueID).ToList());
        }

        public static void HandlingMethod(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //HandlingMethod
                DB.HandlingMethod.Remove(DB.HandlingMethod.First(x => x.UniqueID == uniqueID));

                //AbnormalReasonHandlingMethod
                DB.AbnormalReasonHandlingMethod.RemoveRange(DB.AbnormalReasonHandlingMethod.Where(x => x.HandlingMethodUniqueID == uniqueID).ToList());
            }
        }

        public static void HandlingMethod(TDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //HandlingMethod
                DB.HandlingMethod.Remove(DB.HandlingMethod.First(x => x.UniqueID == uniqueID));

                //AbnormalReasonHandlingMethod
                DB.AbnormalReasonHandlingMethod.RemoveRange(DB.AbnormalReasonHandlingMethod.Where(x => x.HandlingMethodUniqueID == uniqueID).ToList());
            }
        }

        public static void Job(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                Job(DB, uniqueID);
            }
        }

        public static void Job(EDbEntities DB, string UniqueID)
        {
            //Job
            DB.Job.Remove(DB.Job.First(x => x.UniqueID == UniqueID));

            //JobUser
            DB.JobUser.RemoveRange(DB.JobUser.Where(x => x.JobUniqueID == UniqueID).ToList());

            //JobControlPoint
            DB.JobControlPoint.RemoveRange(DB.JobControlPoint.Where(x => x.JobUniqueID == UniqueID).ToList());

            //JobControlPointCheckItem
            DB.JobControlPointCheckItem.RemoveRange(DB.JobControlPointCheckItem.Where(x => x.JobUniqueID == UniqueID).ToList());

            //JobEquipment
            DB.JobEquipment.RemoveRange(DB.JobEquipment.Where(x => x.JobUniqueID == UniqueID).ToList());

            //JobEquipmentCheckItem
            DB.JobEquipmentCheckItem.RemoveRange(DB.JobEquipmentCheckItem.Where(x => x.JobUniqueID == UniqueID).ToList());

            var today = DateTimeHelper.DateTime2DateString(DateTime.Today);
            DB.JobResult.RemoveRange(DB.JobResult.Where(x => x.JobUniqueID == UniqueID && string.Compare(x.BeginDate, today) >= 0).ToList());
        }
        

        public static void MaintenanceJob(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                MaintenanceJob(DB, uniqueID);
            }
        }

        public static void MaintenanceJob(EDbEntities DB, string UniqueID)
        {
            //MJob
            DB.MJob.Remove(DB.MJob.First(x => x.UniqueID == UniqueID));

            //MJobUser
            DB.MJobUser.RemoveRange(DB.MJobUser.Where(x => x.MJobUniqueID == UniqueID).ToList());

            //MJobEquipment
            DB.MJobEquipment.RemoveRange(DB.MJobEquipment.Where(x => x.MJobUniqueID == UniqueID).ToList());

            //MJobEquipmentStandard
            DB.MJobEquipmentStandard.RemoveRange(DB.MJobEquipmentStandard.Where(x => x.MJobUniqueID == UniqueID).ToList());
        }

        //public static void MaintenanceRoute(EDbEntities DB, List<string> KeyList)
        //{
        //    foreach (var uniqueID in KeyList)
        //    {
        //        //MRoute
        //        DB.MRoute.Remove(DB.MRoute.First(x => x.UniqueID == uniqueID));

        //        //MRouteEquipment
        //        DB.MRouteEquipment.RemoveRange(DB.MRouteEquipment.Where(x => x.MRouteUniqueID == uniqueID).ToList());

        //        //MRouteEquipmentStandard
        //        DB.MRouteEquipmentStandard.RemoveRange(DB.MRouteEquipmentStandard.Where(x => x.MRouteUniqueID == uniqueID).ToList());

        //        MaintenanceJob(DB, DB.MJob.Where(x => x.MRouteUniqueID == uniqueID).Select(x => x.UniqueID).ToList());
        //    }
        //}

        public static void Material(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //Material
                DB.Material.Remove(DB.Material.First(x => x.UniqueID == uniqueID));

                //MaterialSpecValue
                DB.MaterialSpecValue.RemoveRange(DB.MaterialSpecValue.Where(x => x.MaterialUniqueID == uniqueID).ToList());

                //EquipmentMaterial
                DB.EquipmentMaterial.RemoveRange(DB.EquipmentMaterial.Where(x => x.MaterialUniqueID == uniqueID).ToList());
            }
        }

        public static void MaterialSpec(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //MaterialSpec
                DB.MaterialSpec.Remove(DB.MaterialSpec.First(x => x.UniqueID == uniqueID));

                //MaterialSpecOption
                DB.MaterialSpecOption.RemoveRange(DB.MaterialSpecOption.Where(x => x.SpecUniqueID == uniqueID).ToList());

                //MaterialSpecValue
                DB.MaterialSpecValue.RemoveRange(DB.MaterialSpecValue.Where(x => x.SpecUniqueID == uniqueID).ToList());
            }
        }

        public static void Route(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //Route
                DB.Route.Remove(DB.Route.First(x => x.UniqueID == uniqueID));

                //RouteControlPoint
                DB.RouteControlPoint.RemoveRange(DB.RouteControlPoint.Where(x => x.RouteUniqueID == uniqueID).ToList());

                //RouteControlPointCheckItem
                DB.RouteControlPointCheckItem.RemoveRange(DB.RouteControlPointCheckItem.Where(x => x.RouteUniqueID == uniqueID).ToList());

                //RouteEquipment
                DB.RouteEquipment.RemoveRange(DB.RouteEquipment.Where(x => x.RouteUniqueID == uniqueID).ToList());

                //RouteEquipmentCheckItem
                DB.RouteEquipmentCheckItem.RemoveRange(DB.RouteEquipmentCheckItem.Where(x => x.RouteUniqueID == uniqueID).ToList());

                Job(DB, DB.Job.Where(x => x.RouteUniqueID == uniqueID).Select(x => x.UniqueID).ToList());
            }
        }

        public static void Standard(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //Standard
                DB.Standard.Remove(DB.Standard.First(x => x.UniqueID == uniqueID));

                //EquipmentStandard
                DB.EquipmentStandard.RemoveRange(DB.EquipmentStandard.Where(x => x.StandardUniqueID == uniqueID).ToList());


                //MJobEquipmentStandard
                DB.MJobEquipmentStandard.RemoveRange(DB.MJobEquipmentStandard.Where(x => x.StandardUniqueID == uniqueID).ToList());
            }
        }

        public static void Organization(DbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                Organization(DB, uniqueID);
            }
        }

        //public static void Organization(EDbEntities EDB, List<string> KeyList)
        //{
        //    foreach (var uniqueID in KeyList)
        //    {
        //        Organization(EDB, uniqueID);
        //    }
        //}

        //public static void Organization(PDbEntities PDB, List<string> KeyList)
        //{
        //    foreach (var uniqueID in KeyList)
        //    {
        //        Organization(PDB, uniqueID);
        //    }
        //}

        //public static void Organization(GDbEntities GDB, List<string> KeyList)
        //{
        //    foreach (var uniqueID in KeyList)
        //    {
        //        Organization(GDB, uniqueID);
        //    }
        //}

        //public static void Organization(TDbEntities TDB, List<string> KeyList)
        //{
        //    foreach (var uniqueID in KeyList)
        //    {
        //        Organization(TDB, uniqueID);
        //    }
        //}

        public static void Organization(DbEntities DB, string UniqueID)
        {
            DB.Organization.Remove(DB.Organization.First(x => x.UniqueID == UniqueID));

            DB.OrganizationManager.RemoveRange(DB.OrganizationManager.Where(x => x.OrganizationUniqueID == UniqueID).ToList());

            DB.EditableOrganization.RemoveRange(DB.EditableOrganization.Where(x => x.OrganizationUniqueID == UniqueID).ToList());

            DB.QueryableOrganization.RemoveRange(DB.QueryableOrganization.Where(x => x.OrganizationUniqueID == UniqueID).ToList());
        }

        //public static void Organization(DbEntities DB, EDbEntities EDB, string UniqueID)
        //{
        //    DB.Organization.Remove(DB.Organization.First(x => x.UniqueID == UniqueID));

        //    DB.EditableOrganization.RemoveRange(DB.EditableOrganization.Where(x => x.OrganizationUniqueID == UniqueID).ToList());

        //    DB.QueryableOrganization.RemoveRange(DB.QueryableOrganization.Where(x => x.OrganizationUniqueID == UniqueID).ToList());

        //    DeleteHelper.EmgContact(DB, DB.EmgContact.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

        //    DeleteHelper.User(DB, EDB, DB.User.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.ID).ToList());

        //    DeleteHelper.AbnormalReason(EDB, EDB.AbnormalReason.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

        //    DeleteHelper.CheckItem(EDB, EDB.CheckItem.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

        //    DeleteHelper.ControlPoint(EDB, EDB.ControlPoint.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

        //    DeleteHelper.Equipment(EDB, EDB.Equipment.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

        //    DeleteHelper.EquipmentSpec(EDB, EDB.EquipmentSpec.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

        //    DeleteHelper.File(EDB, EDB.File.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

        //    DeleteHelper.Folder(EDB, EDB.Folder.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

        //    DeleteHelper.HandlingMethod(EDB, EDB.HandlingMethod.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

        //    DeleteHelper.Route(EDB, EDB.Route.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

        //    DeleteHelper.Material(EDB, EDB.Material.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

        //    DeleteHelper.MaterialSpec(EDB, EDB.MaterialSpec.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

        //    //DeleteHelper.MaintenanceRoute(EDB, EDB.MRoute.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

        //    DeleteHelper.Standard(EDB, EDB.Standard.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UniqueID).ToList());
        //}

        public static void RepairFormType(DbEntities DB, EDbEntities EDB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                DB.FlowForm.RemoveRange(DB.FlowForm.Where(x => x.RFormTypeUniqueID == uniqueID).ToList());

                EDB.RFormType.Remove(EDB.RFormType.First(x => x.UniqueID == uniqueID));

                //RepairFormColumn(EDB, EDB.RFormColumn.Where(x => x.RepairFormTypeUniqueID == uniqueID).Select(x => x.UniqueID).ToList());
            }
        }

        public static void RepairFormColumn(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                DB.RFormColumn.Remove(DB.RFormColumn.First(x => x.UniqueID == uniqueID));

                DB.RFormColumnOption.RemoveRange(DB.RFormColumnOption.Where(x => x.ColumnUniqueID == uniqueID).ToList());

                DB.RFormColumnValue.RemoveRange(DB.RFormColumnValue.Where(x => x.ColumnUniqueID == uniqueID).ToList());
            }
        }

        public static void PipelineSpec(PDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                DB.PipelineSpec.Remove(DB.PipelineSpec.First(x => x.UniqueID == uniqueID));

                DB.PipelineSpecOption.RemoveRange(DB.PipelineSpecOption.Where(x => x.SpecUniqueID == uniqueID).ToList());

                DB.PipelineSpecValue.RemoveRange(DB.PipelineSpecValue.Where(x => x.SpecUniqueID == uniqueID).ToList());
            }
        }

        public static void RepairFormSubject(EDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                DB.RFormSubject.Remove(DB.RFormSubject.First(x => x.UniqueID == uniqueID));
            }
        }
    }
}
