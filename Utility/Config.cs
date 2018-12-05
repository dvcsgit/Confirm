using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Net.Mail;
using Utility.Models;

namespace Utility
{
    public class Config
    {
        #region Global
        private static XElement Root
        {
            get
            {
                return XDocument.Load(Define.ConfigFile).Root;
            }
        }

        public static string WebSite
        {
            get
            {
                return Root.Element("WebSite").Value;
            }
        }

        public static string FormsAuthentication
        {
            get
            {
                return Root.Element("WebSite").Attribute("FormsAuthentication").Value;
            }
        }

        public static string WindowsAuthentication
        {
            get
            {
                return Root.Element("WebSite").Attribute("WindowsAuthentication").Value;
            }
        }

        public static string LogFolder
        {
            get
            {
                return Root.Element("Folder").Element("Log").Value;
            }
        }

        public static string TempFolder
        {
            get
            {
                return Root.Element("Folder").Element("Temp").Value;
            }
        }

        public static string CompanyLogoImage
        {
            get
            {
                string image = string.Empty;

                var element = Root.Element("CompanyLogoImage");

                if (element != null)
                {
                    image = element.Value;
                }

                return image;
            }
        }

        public static Dictionary<string, string> SystemName
        {
            get
            {
                var element = Root.Element("SystemName");

                return new Dictionary<string, string>() 
                { 
                    { "zh-tw", element.Attribute("zh_tw").Value },
                    { "zh-cn", element.Attribute("zh_cn").Value },
                    { "en-us", element.Attribute("en_us").Value }
                }; ;
            }
        }

        public static Dictionary<string, string> SystemLogoImage
        {
            get
            {
                var images = Root.Element("SystemLogoImage");

                return  new Dictionary<string, string>() 
                { 
                    { "zh-tw", images.Attribute("zh_tw").Value },
                    { "zh-cn", images.Attribute("zh_cn").Value },
                    { "en-us", images.Attribute("en_us").Value }
                };;
            }
        }

        public static List<Define.EnumSystemModule> Modules
        {
            get
            {
                return Root.Element("Modules").Elements("Module").Select(x => Define.EnumParse<Define.EnumSystemModule>(x.Value)).ToList();
            }
        }
        #endregion

        #region Mail
        public static bool HaveMailSetting
        {
            get
            {
                return Root.Element("Mail") != null;
            }
        }

        public static List<MailAddress> BCC
        {
            get
            {
                var element = Root.Element("Mail").Element("BCC");

                if (element != null)
                {
                    var elements = element.Elements("MAIL").ToList();

                    if (elements != null && elements.Count > 0)
                    {
                        var bcc = new List<MailAddress>();

                        foreach (var e in elements)
                        {
                            bcc.Add(new MailAddress(e.Attribute("Mail").Value, e.Attribute("Name").Value));
                        }

                        return bcc;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public static MailAddress MailFrom
        {
            get
            {
                return new MailAddress(Root.Element("Mail").Element("MailFrom").Attribute("Mail").Value, Root.Element("Mail").Element("MailFrom").Attribute("Name").Value);
            }
        }

        public static string SmtpHost
        {
            get
            {
                return Root.Element("Mail").Element("Smtp").Attribute("Host").Value;
            }
        }

        public static string SmtpUserName
        {
            get
            {
                try
                {
                    return Root.Element("Mail").Element("Smtp").Attribute("UserName").Value;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public static string SmtpPassword
        {
            get
            {
                try
                {
                    return Root.Element("Mail").Element("Smtp").Attribute("Password").Value;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public static string SmtpDomain
        {
            get
            {
                try
                {
                    return Root.Element("Mail").Element("Smtp").Attribute("Domain").Value;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
        #endregion

        #region LDAP
        public static bool HaveLDAPSetting
        {
            get
            {
                return Root.Element("LDAP") != null;
            }
        }

        public static string LDAP_Domain
        {
            get
            {
                return Root.Element("LDAP").Attribute("Domain").Value;
            }
        }

        public static string LDAP_UserID
        {
            get
            {
                return Root.Element("LDAP").Attribute("UserID").Value;
            }
        }

        public static string LDAP_Password
        {
            get
            {
                return Root.Element("LDAP").Attribute("Password").Value;
            }
        }
        #endregion

        public static List<UserLimit> UserLimits
        {
            get
            {
                var userLimits = new List<UserLimit>();

                try
                {
                    var elementList = Root.Element("UserLimits").Elements("UserLimit").ToList();

                    foreach (var element in elementList)
                    {
                        userLimits.Add(new UserLimit()
                        {
                            OrganizationUniqueID = element.Attribute("OrganizationUniqueID").Value,
                            Users = int.Parse(element.Attribute("Users").Value),
                            MobileUsers = int.Parse(element.Attribute("MobileUsers").Value)
                        });
                    }
                }
                catch
                {
                    userLimits = new List<UserLimit>();
                }

                return userLimits;
            }
        }

        public static string UserPhotoFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("UserPhoto").Value;
            }
        }

        public static string UserPhotoVirtualPath
        {
            get
            {
                return Root.Element("Folder").Element("UserPhoto").Attribute("VirtualPath").Value;
            }
        }

        #region EquipmentMaintenance

        public static string EquipmentMaintenanceMobileReleaseFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("MobileRelease").Value;
            }
        }

        public static string EquipmentMaintenanceSQLiteTemplateFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("SQLite").Element("Template").Value;
            }
        }

        public static string EquipmentMaintenanceSQLiteGeneratedFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("SQLite").Element("Generated").Value;
            }
        }

        public static string EquipmentMaintenanceSQLiteUploadFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("SQLite").Element("Upload").Value;
            }
        }

        public static string EquipmentMaintenanceSQLiteProcessingFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("SQLite").Element("Processing").Value;
            }
        }

        public static string EquipmentMaintenanceSQLiteBackupFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("SQLite").Element("Backup").Value;
            }
        }

        public static string EquipmentMaintenanceSQLiteErrorFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("SQLite").Element("Error").Value;
            }
        }

        public static string EquipmentMaintenancePhotoFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("Photo").Value;
            }
        }

        public static string EquipmentMaintenanceFileFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("File").Value;
            }
        }

        public static string EquipmentMaintenanceTagSQLiteTemplateFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("TagSQLite").Element("Template").Value;
            }
        }

        public static string EquipmentMaintenanceTagSQLiteGeneratedFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("TagSQLite").Element("Generated").Value;
            }
        }

        public static string EquipmentMaintenanceTagSQLiteUploadFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("TagSQLite").Element("Upload").Value;
            }
        }

        public static string EquipmentMaintenanceTagSQLiteProcessingFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("TagSQLite").Element("Processing").Value;
            }
        }

        public static string EquipmentMaintenanceTagSQLiteBackupFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("TagSQLite").Element("Backup").Value;
            }
        }

        public static string EquipmentMaintenanceTagSQLiteErrorFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("TagSQLite").Element("Error").Value;
            }
        }

        public static string EquipmentMaintenanceImportTemplateFile(Define.EnumExcelVersion ExcelVersion)
        {
            if (ExcelVersion == Define.EnumExcelVersion._2003)
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("Import").Elements("Template").First(x => x.Attribute("ExcelVersion").Value == "2003").Value;
            }
            else if (ExcelVersion == Define.EnumExcelVersion._2007)
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("Import").Elements("Template").First(x => x.Attribute("ExcelVersion").Value == "2007").Value;
            }
            else
            {
                return string.Empty;
            }
        }
        #endregion

        public static string EquipmentMaintenanceSubstationInspectionReports
        {
            get
            {
                return Root.Element("Folder").Element("EquipmentMaintenance").Element("ReportTemplate").Element("SubstationInspectionReports").Value;
            }
        }


        #region TruckPatrol
        public static string TruckPatrolMobileReleaseFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TruckPatrol").Element("MobileRelease").Value;
            }
        }


        public static string TruckPatrolSQLiteTemplateFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TruckPatrol").Element("SQLite").Element("Template").Value;
            }
        }

        public static string TruckPatrolSQLiteGeneratedFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TruckPatrol").Element("SQLite").Element("Generated").Value;
            }
        }

        public static string TruckPatrolSQLiteUploadFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TruckPatrol").Element("SQLite").Element("Upload").Value;
            }
        }

        public static string TruckPatrolSQLiteProcessingFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TruckPatrol").Element("SQLite").Element("Processing").Value;
            }
        }

        public static string TruckPatrolSQLiteBackupFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TruckPatrol").Element("SQLite").Element("Backup").Value;
            }
        }

        public static string TruckPatrolSQLiteErrorFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TruckPatrol").Element("SQLite").Element("Error").Value;
            }
        }

        public static string TruckPatrolPhotoFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TruckPatrol").Element("Photo").Value;
            }
        }
        #endregion

        #region PipelinePatrol
        public static string PipelinePatrolMobileReleaseFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("PipelinePatrol").Element("MobileRelease").Value;
            }
        }

        public static string PipelinePatrolSQLiteTemplateFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("PipelinePatrol").Element("SQLite").Element("Template").Value;
            }
        }

        public static string PipelinePatrolSQLiteGeneratedFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("PipelinePatrol").Element("SQLite").Element("Generated").Value;
            }
        }

        public static string PipelinePatrolSQLiteUploadFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("PipelinePatrol").Element("SQLite").Element("Upload").Value;
            }
        }

        public static string PipelinePatrolSQLiteProcessingFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("PipelinePatrol").Element("SQLite").Element("Processing").Value;
            }
        }

        public static string PipelinePatrolSQLiteBackupFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("PipelinePatrol").Element("SQLite").Element("Backup").Value;
            }
        }

        public static string PipelinePatrolSQLiteErrorFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("PipelinePatrol").Element("SQLite").Element("Error").Value;
            }
        }

        public static string PipelinePatrolPhotoFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("PipelinePatrol").Element("Photo").Value;
            }
        }

        public static string PipelinePatrolFileFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("PipelinePatrol").Element("File").Value;
            }
        }
        #endregion

        #region Chat
        public static string ChatPhotoFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("Chat").Element("Photo").Value;
            }
        }
        #endregion

        #region GuardPatrol

        public static string GuardPatrolMobileReleaseFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("MobileRelease").Value;
            }
        }

        public static string GuardPatrolSQLiteTemplateFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("SQLite").Element("Template").Value;
            }
        }

        public static string GuardPatrolSQLiteGeneratedFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("SQLite").Element("Generated").Value;
            }
        }

        public static string GuardPatrolSQLiteUploadFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("SQLite").Element("Upload").Value;
            }
        }

        public static string GuardPatrolSQLiteProcessingFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("SQLite").Element("Processing").Value;
            }
        }

        public static string GuardPatrolSQLiteBackupFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("SQLite").Element("Backup").Value;
            }
        }

        public static string GuardPatrolSQLiteErrorFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("SQLite").Element("Error").Value;
            }
        }

        public static string GuardPatrolPhotoFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("Photo").Value;
            }
        }

        public static string GuardPatrolTagSQLiteTemplateFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("TagSQLite").Element("Template").Value;
            }
        }

        public static string GuardPatrolTagSQLiteGeneratedFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("TagSQLite").Element("Generated").Value;
            }
        }

        public static string GuardPatrolTagSQLiteUploadFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("TagSQLite").Element("Upload").Value;
            }
        }

        public static string GuardPatrolTagSQLiteProcessingFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("TagSQLite").Element("Processing").Value;
            }
        }

        public static string GuardPatrolTagSQLiteBackupFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("TagSQLite").Element("Backup").Value;
            }
        }

        public static string GuardPatrolTagSQLiteErrorFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("GuardPatrol").Element("TagSQLite").Element("Error").Value;
            }
        }
        #endregion

        #region TankPatrol
        public static string TankPatrolMobileReleaseFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TankPatrol").Element("MobileRelease").Value;
            }
        }

        public static string TankPatrolSQLiteTemplateFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TankPatrol").Element("SQLite").Element("Template").Value;
            }
        }

        public static string TankPatrolSQLiteGeneratedFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TankPatrol").Element("SQLite").Element("Generated").Value;
            }
        }

        public static string TankPatrolSQLiteUploadFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TankPatrol").Element("SQLite").Element("Upload").Value;
            }
        }

        public static string TankPatrolSQLiteProcessingFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TankPatrol").Element("SQLite").Element("Processing").Value;
            }
        }

        public static string TankPatrolSQLiteBackupFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TankPatrol").Element("SQLite").Element("Backup").Value;
            }
        }

        public static string TankPatrolSQLiteErrorFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TankPatrol").Element("SQLite").Element("Error").Value;
            }
        }

        public static string TankPatrolPhotoFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("TankPatrol").Element("Photo").Value;
            }
        }

        #endregion

#region QA
        public static string QASQLiteTemplateFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QA").Element("SQLite").Element("Template").Value;
            }
        }

        public static string QASQLiteGeneratedFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QA").Element("SQLite").Element("Generated").Value;
            }
        }

        public static string QASQLiteUploadFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QA").Element("SQLite").Element("Upload").Value;
            }
        }

        public static string QASQLiteProcessingFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QA").Element("SQLite").Element("Processing").Value;
            }
        }

        public static string QASQLiteBackupFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QA").Element("SQLite").Element("Backup").Value;
            }
        }

        public static string QASQLiteErrorFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QA").Element("SQLite").Element("Error").Value;
            }
        }

        public static string QAFileFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QA").Element("File").Value;
            }
        }

        public static string QAFile_v2FolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QA").Element("File_v2").Value;
            }
        }

        public static string QAMobileReleaseFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QA").Element("MobileRelease").Value;
            }
        }

        public static string ReportTemplate_MSA
        {
            get
            {
                return Root.Element("Folder").Element("QA").Element("ReportTemplate").Element("MSA").Value;
            }
        }

        public static string ReportTemplate_MSA_Anova
        {
            get
            {
                return Root.Element("Folder").Element("QA").Element("ReportTemplate").Element("MSAAnova").Value;
            }
        }

        public static string ReportTemplate_MSA_Count
        {
            get
            {
                return Root.Element("Folder").Element("QA").Element("ReportTemplate").Element("MSACount").Value;
            }
        }
#endregion

        #region QS
        public static string QSSQLiteTemplateFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QS").Element("SQLite").Element("Template").Value;
            }
        }

        public static string QSSQLiteGeneratedFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QS").Element("SQLite").Element("Generated").Value;
            }
        }

        public static string QSSQLiteUploadFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QS").Element("SQLite").Element("Upload").Value;
            }
        }

        public static string QSSQLiteProcessingFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QS").Element("SQLite").Element("Processing").Value;
            }
        }

        public static string QSSQLiteBackupFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QS").Element("SQLite").Element("Backup").Value;
            }
        }

        public static string QSSQLiteErrorFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QS").Element("SQLite").Element("Error").Value;
            }
        }

        public static string QSMobileReleaseFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QS").Element("MobileRelease").Value;
            }
        }

        public static string QSFileFolderPath
        {
            get
            {
                return Root.Element("Folder").Element("QS").Element("File").Value;
            }
        }
        #endregion
    }
}
