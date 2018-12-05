using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using DbEntity.ASE;

namespace DataAccess.ASE
{
    public class DeleteHelper
    {
        public static void AbnormalReason(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                AbnormalReason(DB, uniqueID);
            }
        }

        public static void AbnormalReason(ASEDbEntities DB, string UniqueID)
        {
            //AbnormalReason
            DB.ABNORMALREASON.Remove(DB.ABNORMALREASON.First(x => x.UNIQUEID == UniqueID));

            //AbnormalReasonHANDLINGMETHOD
            DB.ABNORMALREASONHANDLINGMETHOD.RemoveRange(DB.ABNORMALREASONHANDLINGMETHOD.Where(x => x.ABNORMALREASONUNIQUEID == UniqueID).ToList());

            //CheckItemAbnormalReason
            DB.CHECKITEMABNORMALREASON.RemoveRange(DB.CHECKITEMABNORMALREASON.Where(x => x.ABNORMALREASONUNIQUEID == UniqueID).ToList());
        }

        public static void CheckItem(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //CheckItem
                DB.CHECKITEM.Remove(DB.CHECKITEM.First(x => x.UNIQUEID == uniqueID));

                //CheckItemAbnormalReason
                DB.CHECKITEMABNORMALREASON.RemoveRange(DB.CHECKITEMABNORMALREASON.Where(x => x.CHECKITEMUNIQUEID == uniqueID).ToList());

                //CheckItemFeelOption
                DB.CHECKITEMFEELOPTION.RemoveRange(DB.CHECKITEMFEELOPTION.Where(x => x.CHECKITEMUNIQUEID == uniqueID).ToList());

                //CONTROLPOINTCHECKITEM
                DB.CONTROLPOINTCHECKITEM.RemoveRange(DB.CONTROLPOINTCHECKITEM.Where(x => x.CHECKITEMUNIQUEID == uniqueID).ToList());

                //EQUIPMENTCHECKITEM
                DB.EQUIPMENTCHECKITEM.RemoveRange(DB.EQUIPMENTCHECKITEM.Where(x => x.CHECKITEMUNIQUEID == uniqueID).ToList());

                //ROUTECONTROLPOINTCHECKITEM
                DB.ROUTECONTROLPOINTCHECKITEM.RemoveRange(DB.ROUTECONTROLPOINTCHECKITEM.Where(x => x.CHECKITEMUNIQUEID == uniqueID).ToList());

                //ROUTEEQUIPMENTCHECKITEM
                DB.ROUTEEQUIPMENTCHECKITEM.RemoveRange(DB.ROUTEEQUIPMENTCHECKITEM.Where(x => x.CHECKITEMUNIQUEID == uniqueID).ToList());

                //JOBCONTROLPOINTCHECKITEM
                DB.JOBCONTROLPOINTCHECKITEM.RemoveRange(DB.JOBCONTROLPOINTCHECKITEM.Where(x => x.CHECKITEMUNIQUEID == uniqueID).ToList());

                //JOBEQUIPMENTCHECKITEM
                DB.JOBEQUIPMENTCHECKITEM.RemoveRange(DB.JOBEQUIPMENTCHECKITEM.Where(x => x.CHECKITEMUNIQUEID == uniqueID).ToList());
            }
        }

        public static void ControlPoint(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //CONTROLPOINT
                DB.CONTROLPOINT.Remove(DB.CONTROLPOINT.First(x => x.UNIQUEID == uniqueID));

                //CONTROLPOINTCHECKITEM
                DB.CONTROLPOINTCHECKITEM.RemoveRange(DB.CONTROLPOINTCHECKITEM.Where(x => x.CONTROLPOINTUNIQUEID == uniqueID).ToList());

                //JOBCONTROLPOINT
                DB.JOBCONTROLPOINT.RemoveRange(DB.JOBCONTROLPOINT.Where(x => x.CONTROLPOINTUNIQUEID == uniqueID).ToList());

                //JOBCONTROLPOINTCHECKITEM
                DB.JOBCONTROLPOINTCHECKITEM.RemoveRange(DB.JOBCONTROLPOINTCHECKITEM.Where(x => x.CONTROLPOINTUNIQUEID == uniqueID).ToList());

                //JOBEQUIPMENT
                DB.JOBEQUIPMENT.RemoveRange(DB.JOBEQUIPMENT.Where(x => x.CONTROLPOINTUNIQUEID == uniqueID).ToList());

                //JOBEQUIPMENTCHECKITEM
                DB.JOBEQUIPMENTCHECKITEM.RemoveRange(DB.JOBEQUIPMENTCHECKITEM.Where(x => x.CONTROLPOINTUNIQUEID == uniqueID).ToList());

                //ROUTECONTROLPOINT
                DB.ROUTECONTROLPOINT.RemoveRange(DB.ROUTECONTROLPOINT.Where(x => x.CONTROLPOINTUNIQUEID == uniqueID).ToList());

                //ROUTECONTROLPOINTCHECKITEM
                DB.ROUTECONTROLPOINTCHECKITEM.RemoveRange(DB.ROUTECONTROLPOINTCHECKITEM.Where(x => x.CONTROLPOINTUNIQUEID == uniqueID).ToList());

                //ROUTEEQUIPMENT
                DB.ROUTEEQUIPMENT.RemoveRange(DB.ROUTEEQUIPMENT.Where(x => x.CONTROLPOINTUNIQUEID == uniqueID).ToList());

                //ROUTEEQUIPMENTCHECKITEM
                DB.ROUTEEQUIPMENTCHECKITEM.RemoveRange(DB.ROUTEEQUIPMENTCHECKITEM.Where(x => x.CONTROLPOINTUNIQUEID == uniqueID).ToList());
            }
        }

        public static void Equipment(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //EQUIPMENT
                DB.EQUIPMENT.Remove(DB.EQUIPMENT.First(x => x.UNIQUEID == uniqueID));

                //EQUIPMENTCHECKITEM
                DB.EQUIPMENTCHECKITEM.RemoveRange(DB.EQUIPMENTCHECKITEM.Where(x => x.EQUIPMENTUNIQUEID == uniqueID).ToList());

                //EQUIPMENTMATERIAL
                DB.EQUIPMENTMATERIAL.RemoveRange(DB.EQUIPMENTMATERIAL.Where(x => x.EQUIPMENTUNIQUEID == uniqueID).ToList());

                //EQUIPMENTPART
                DB.EQUIPMENTPART.RemoveRange(DB.EQUIPMENTPART.Where(x => x.EQUIPMENTUNIQUEID == uniqueID).ToList());

                //EQUIPMENTSPECVALUE
                DB.EQUIPMENTSPECVALUE.RemoveRange(DB.EQUIPMENTSPECVALUE.Where(x => x.EQUIPMENTUNIQUEID == uniqueID).ToList());

                //EQUIPMENTSTANDARD
                DB.EQUIPMENTSTANDARD.RemoveRange(DB.EQUIPMENTSTANDARD.Where(x => x.EQUIPMENTUNIQUEID == uniqueID).ToList());

                //JOBEQUIPMENT
                DB.JOBEQUIPMENT.RemoveRange(DB.JOBEQUIPMENT.Where(x => x.EQUIPMENTUNIQUEID == uniqueID).ToList());

                //JOBEQUIPMENTCHECKITEM
                DB.JOBEQUIPMENTCHECKITEM.RemoveRange(DB.JOBEQUIPMENTCHECKITEM.Where(x => x.EQUIPMENTUNIQUEID == uniqueID).ToList());

                //MJOBEQUIPMENT
                DB.MJOBEQUIPMENT.RemoveRange(DB.MJOBEQUIPMENT.Where(x => x.EQUIPMENTUNIQUEID == uniqueID).ToList());

                //MJOBEQUIPMENTSTANDARD
                DB.MJOBEQUIPMENTSTANDARD.RemoveRange(DB.MJOBEQUIPMENTSTANDARD.Where(x => x.EQUIPMENTUNIQUEID == uniqueID).ToList());

                //ROUTEEQUIPMENT
                DB.ROUTEEQUIPMENT.RemoveRange(DB.ROUTEEQUIPMENT.Where(x => x.EQUIPMENTUNIQUEID == uniqueID).ToList());

                //ROUTEEQUIPMENTCHECKITEM
                DB.ROUTEEQUIPMENTCHECKITEM.RemoveRange(DB.ROUTEEQUIPMENTCHECKITEM.Where(x => x.EQUIPMENTUNIQUEID == uniqueID).ToList());

                Folder(DB, DB.FOLDER.Where(x => x.EQUIPMENTUNIQUEID == uniqueID).Select(x => x.UNIQUEID).ToList());

                File(DB, DB.FFILE.Where(x => x.EQUIPMENTUNIQUEID == uniqueID).Select(x => x.UNIQUEID).ToList());
            }
        }

        public static void EquipmentSpec(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //EQUIPMENTSPEC
                DB.EQUIPMENTSPEC.Remove(DB.EQUIPMENTSPEC.First(x => x.UNIQUEID == uniqueID));

                //EQUIPMENTSPECOPTION
                DB.EQUIPMENTSPECOPTION.RemoveRange(DB.EQUIPMENTSPECOPTION.Where(x => x.SPECUNIQUEID == uniqueID).ToList());

                //EQUIPMENTSPECVALUE
                DB.EQUIPMENTSPECVALUE.RemoveRange(DB.EQUIPMENTSPECVALUE.Where(x => x.SPECUNIQUEID == uniqueID).ToList());
            }
        }

        public static void File(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                var file = DB.FFILE.First(x => x.UNIQUEID == uniqueID);

                DB.FFILE.Remove(file);

                try
                {
                    var filePath = System.IO.Path.Combine(Config.EquipmentMaintenanceFileFolderPath, file.UNIQUEID + "." + file.EXTENSION);
                    var zipFilePath = System.IO.Path.Combine(Config.EquipmentMaintenanceFileFolderPath, file.UNIQUEID + ".zip");

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

        public static void Folder(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                Folder(DB, uniqueID);
            }
        }

        public static void Folder(ASEDbEntities DB, string UniqueID)
        {
            DB.FOLDER.Remove(DB.FOLDER.First(x => x.UNIQUEID == UniqueID));

            Folder(DB, DB.FOLDER.Where(x => x.FOLDERUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());

            File(DB, DB.FFILE.Where(x => x.FOLDERUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());
        }

        public static void HandlingMethod(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //HANDLINGMETHOD
                DB.HANDLINGMETHOD.Remove(DB.HANDLINGMETHOD.First(x => x.UNIQUEID == uniqueID));

                //AbnormalReasonHANDLINGMETHOD
                DB.ABNORMALREASONHANDLINGMETHOD.RemoveRange(DB.ABNORMALREASONHANDLINGMETHOD.Where(x => x.HANDLINGMETHODUNIQUEID == uniqueID).ToList());
            }
        }

        public static void Job(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                Job(DB, uniqueID);
            }
        }

        public static void Job(ASEDbEntities DB, string UniqueID)
        {
            //JOB
            DB.JOB.Remove(DB.JOB.First(x => x.UNIQUEID == UniqueID));

            //JOBUSER
            DB.JOBUSER.RemoveRange(DB.JOBUSER.Where(x => x.JOBUNIQUEID == UniqueID).ToList());

            //JOBCONTROLPOINT
            DB.JOBCONTROLPOINT.RemoveRange(DB.JOBCONTROLPOINT.Where(x => x.JOBUNIQUEID == UniqueID).ToList());

            //JOBCONTROLPOINTCHECKITEM
            DB.JOBCONTROLPOINTCHECKITEM.RemoveRange(DB.JOBCONTROLPOINTCHECKITEM.Where(x => x.JOBUNIQUEID == UniqueID).ToList());

            //JOBEQUIPMENT
            DB.JOBEQUIPMENT.RemoveRange(DB.JOBEQUIPMENT.Where(x => x.JOBUNIQUEID == UniqueID).ToList());

            //JOBEQUIPMENTCHECKITEM
            DB.JOBEQUIPMENTCHECKITEM.RemoveRange(DB.JOBEQUIPMENTCHECKITEM.Where(x => x.JOBUNIQUEID == UniqueID).ToList());
        }

        public static void MaintenanceJob(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                MaintenanceJob(DB, uniqueID);
            }
        }

        public static void MaintenanceJob(ASEDbEntities DB, string UniqueID)
        {
            //MJOB
            DB.MJOB.Remove(DB.MJOB.First(x => x.UNIQUEID == UniqueID));

            //MJOBUSER
            DB.MJOBUSER.RemoveRange(DB.MJOBUSER.Where(x => x.MJOBUNIQUEID == UniqueID).ToList());

            //MJOBEQUIPMENT
            DB.MJOBEQUIPMENT.RemoveRange(DB.MJOBEQUIPMENT.Where(x => x.MJOBUNIQUEID == UniqueID).ToList());

            //MJOBEQUIPMENTSTANDARD
            DB.MJOBEQUIPMENTSTANDARD.RemoveRange(DB.MJOBEQUIPMENTSTANDARD.Where(x => x.MJOBUNIQUEID == UniqueID).ToList());
        }


        public static void Material(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //MATERIAL
                DB.MATERIAL.Remove(DB.MATERIAL.First(x => x.UNIQUEID == uniqueID));

                //MATERIALSPECVALUE
                DB.MATERIALSPECVALUE.RemoveRange(DB.MATERIALSPECVALUE.Where(x => x.MATERIALUNIQUEID == uniqueID).ToList());

                //EQUIPMENTMATERIAL
                DB.EQUIPMENTMATERIAL.RemoveRange(DB.EQUIPMENTMATERIAL.Where(x => x.MATERIALUNIQUEID == uniqueID).ToList());
            }
        }

        public static void MaterialSpec(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //MATERIALSpec
                DB.MATERIALSPEC.Remove(DB.MATERIALSPEC.First(x => x.UNIQUEID == uniqueID));

                //MATERIALSpecOption
                DB.MATERIALSPECOPTION.RemoveRange(DB.MATERIALSPECOPTION.Where(x => x.SPECUNIQUEID == uniqueID).ToList());

                //MATERIALSPECVALUE
                DB.MATERIALSPECVALUE.RemoveRange(DB.MATERIALSPECVALUE.Where(x => x.SPECUNIQUEID == uniqueID).ToList());
            }
        }

        public static void Route(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //Route
                DB.ROUTE.Remove(DB.ROUTE.First(x => x.UNIQUEID == uniqueID));

                //ROUTECONTROLPOINT
                DB.ROUTECONTROLPOINT.RemoveRange(DB.ROUTECONTROLPOINT.Where(x => x.ROUTEUNIQUEID == uniqueID).ToList());

                //ROUTECONTROLPOINTCHECKITEM
                DB.ROUTECONTROLPOINTCHECKITEM.RemoveRange(DB.ROUTECONTROLPOINTCHECKITEM.Where(x => x.ROUTEUNIQUEID == uniqueID).ToList());

                //ROUTEEQUIPMENT
                DB.ROUTEEQUIPMENT.RemoveRange(DB.ROUTEEQUIPMENT.Where(x => x.ROUTEUNIQUEID == uniqueID).ToList());

                //ROUTEEQUIPMENTCHECKITEM
                DB.ROUTEEQUIPMENTCHECKITEM.RemoveRange(DB.ROUTEEQUIPMENTCHECKITEM.Where(x => x.ROUTEUNIQUEID == uniqueID).ToList());

                Job(DB, DB.JOB.Where(x => x.ROUTEUNIQUEID == uniqueID).Select(x => x.UNIQUEID).ToList());
            }
        }

        public static void Standard(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                //Standard
                DB.STANDARD.Remove(DB.STANDARD.First(x => x.UNIQUEID == uniqueID));

                //EQUIPMENTSTANDARD
                DB.EQUIPMENTSTANDARD.RemoveRange(DB.EQUIPMENTSTANDARD.Where(x => x.STANDARDUNIQUEID == uniqueID).ToList());

                //MJOBEQUIPMENTSTANDARD
                DB.MJOBEQUIPMENTSTANDARD.RemoveRange(DB.MJOBEQUIPMENTSTANDARD.Where(x => x.STANDARDUNIQUEID == uniqueID).ToList());
            }
        }

        public static void User(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var userID in KeyList)
            {
                DB.ACCOUNT.Remove(DB.ACCOUNT.First(x => x.ID == userID));
                DB.USERAUTHGROUP.RemoveRange(DB.USERAUTHGROUP.Where(x => x.USERID == userID).ToList());
                DB.JOBUSER.RemoveRange(DB.JOBUSER.Where(x => x.USERID == userID).ToList());
                DB.MJOBUSER.RemoveRange(DB.MJOBUSER.Where(x => x.USERID == userID).ToList());

                EmgContact(DB, DB.EMGCONTACT.Where(x => x.USERID == userID).Select(x => x.UNIQUEID).ToList());
            }
        }

        public static void EmgContact(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                DB.EMGCONTACT.Remove(DB.EMGCONTACT.First(x => x.UNIQUEID == uniqueID));

                DB.EMGCONTACTTEL.RemoveRange(DB.EMGCONTACTTEL.Where(x => x.EMGCONTACTUNIQUEID == uniqueID).ToList());
            }
        }

        public static void Organization(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                Organization(DB, uniqueID);
            }
        }

        public static void Organization(ASEDbEntities DB, string UniqueID)
        {
            DB.ORGANIZATION.Remove(DB.ORGANIZATION.First(x => x.UNIQUEID == UniqueID));

            DB.EDITABLEORGANIZATION.RemoveRange(DB.EDITABLEORGANIZATION.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).ToList());
            DB.QUERYABLEORGANIZATION.RemoveRange(DB.QUERYABLEORGANIZATION.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).ToList());

            DeleteHelper.EmgContact(DB, DB.EMGCONTACT.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());

            DeleteHelper.User(DB, DB.ACCOUNT.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.ID).ToList());

            DeleteHelper.AbnormalReason(DB, DB.ABNORMALREASON.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());

            DeleteHelper.CheckItem(DB, DB.CHECKITEM.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());

            DeleteHelper.ControlPoint(DB, DB.CONTROLPOINT.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());

            DeleteHelper.Equipment(DB, DB.EQUIPMENT.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());

            DeleteHelper.EquipmentSpec(DB, DB.EQUIPMENTSPEC.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());

            DeleteHelper.File(DB, DB.FFILE.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());

            DeleteHelper.Folder(DB, DB.FOLDER.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());

            DeleteHelper.HandlingMethod(DB, DB.HANDLINGMETHOD.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());

            DeleteHelper.Route(DB, DB.ROUTE.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());

            DeleteHelper.Material(DB, DB.MATERIAL.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());

            DeleteHelper.MaterialSpec(DB, DB.MATERIALSPEC.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());

            DeleteHelper.Standard(DB, DB.STANDARD.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.UNIQUEID).ToList());
        }

        public static void RepairFormType(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                DB.FLOWFORM.RemoveRange(DB.FLOWFORM.Where(x => x.RFORMTYPEUNIQUEID == uniqueID).ToList());

                DB.RFORMTYPE.Remove(DB.RFORMTYPE.First(x => x.UNIQUEID == uniqueID));

                //RepairFormColumn(EDB, EDB.RFormColumn.Where(x => x.RepairFormTypeUniqueID == uniqueID).Select(x => x.UNIQUEID).ToList());
            }
        }

        public static void RepairFormColumn(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                DB.RFORMCOLUMN.Remove(DB.RFORMCOLUMN.First(x => x.UNIQUEID == uniqueID));

                DB.RFORMCOLUMNOPTION.RemoveRange(DB.RFORMCOLUMNOPTION.Where(x => x.COLUMNUNIQUEID == uniqueID).ToList());

                DB.RFORMCOLUMNVALUE.RemoveRange(DB.RFORMCOLUMNVALUE.Where(x => x.COLUMNUNIQUEID == uniqueID).ToList());
            }
        }

        public static void RepairFormSubject(ASEDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                DB.RFORMSUBJECT.Remove(DB.RFORMSUBJECT.First(x => x.UNIQUEID == uniqueID));
            }
        }
    }
}
