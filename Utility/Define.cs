using System;
using System.IO;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Web.Mvc.Ajax;
using PagedList.Mvc;

namespace Utility
{
    public class Define
    {
        public const string Version = "v1808.29.1";

        public const string UtilityPassword = "70549797";

        public const string SQLite_EquipmentMaintenance = "EquipmentMaintenance.db";

        public const string SQLiteZip_EquipmentMaintenance = "EquipmentMaintenance.zip";

        public const string SQLite_EquipmentMaintenanceTag = "EquipmentMaintenance.Tag.db";

        public const string SQLiteZip_EquipmentMaintenanceTag = "EquipmentMaintenance.Tag.zip";

        public const string SQLite_QS = "QS.db";

        public const string SQLiteZip_QS = "QS.zip";

        public const string SQLite_TankPatrol = "TankPatrol.db";

        public const string SQLiteZip_TankPatrol = "TankPatrol.zip";

        public const string SQLite_TruckPatrol = "TruckPatrol.db";

        public const string SQLiteZip_TruckPatrol = "TruckPatrol.zip";

        public const string SQLite_PipelinePatrol = "PipelinePatrol.db";

        public const string SQLiteZip_PipelinePatrol = "PipelinePatrol.zip";

        public const string SQLite_GuardPatrol = "EquipmentMaintenance.db";
        //public const string SQLite_GuardPatrol = "GuardPatrol.db";

        public const string SQLiteZip_GuardPatrol = "EquipmentMaintenance.zip";
        //public const string SQLiteZip_GuardPatrol = "GuardPatrol.zip";

        //public const string SQLite_GuardPatrolTag = "GuardPatrol.Tag.db";
        public const string SQLite_GuardPatrolTag = "EquipmentMaintenance.Tag.db";

        //public const string SQLiteZip_GuardPatrolTag = "GuardPatrol.Tag.zip";
        public const string SQLiteZip_GuardPatrolTag = "EquipmentMaintenance.Tag.zip";

        public const string PhotoZip = "Photo.zip";

        public const string FileZip = "File.zip";

        public const string Color_Red = "#d15b47";

        public const string Color_Green = "#82af6f";

        public const string Color_Blue = "#3a87ad";

        public const string Color_Orange = "#f0ad4e";

        public const string Color_Gray = "#999";

        public const string Color_Purple = "#9585bf";

        public const string Label_Color_Red_Class = "label-danger";

        public const string Label_Color_Green_Class = "label-success";

        public const string Label_Color_Blue_Class = "label-info";

        public const string NEW = "NEW";

        public const string OTHER = "OTHER";

        public static Dictionary<int, string> FileSizeDescription = new Dictionary<int, string>() 
        { 
            { 0, "Bytes" },
            { 1, "KB" },
            { 2, "MB" },
            { 3, "GB" },
            { 4, "TB" },
            { 5, "PB" },
            { 6, "EB" },
            { 7, "ZB" },
            { 8, "YB" }
        };

        #region Excel
        public const string ExcelContentType_2003 = "application/vnd.ms-excel";

        public const string ExcelContentType_2007 = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public const string ExcelExtension_2003 = "xls";

        public const string ExcelExtension_2007 = "xlsx";
        #endregion

        #region Config
        private const string ConfigFileName = "Utility.Config.xml";

        public static string ConfigFile
        {
            get
            {
                string exePath = System.AppDomain.CurrentDomain.BaseDirectory;

                string filePath = Path.Combine(exePath, ConfigFileName);

                if (File.Exists(filePath))
                {
                    return filePath;
                }
                else
                {
                    filePath = Path.Combine(exePath, "bin", ConfigFileName);

                    if (File.Exists(filePath))
                    {
                        return filePath;
                    }
                    else
                    {
                        return ConfigFileName;
                    }
                }
            }
        }
        #endregion

        #region DateTime Format
        public const char DateTimeFormat_DateSeperator = '-';

        public const char DateTimeFormat_TimeSeperator = ':';

        /// <summary>
        /// yyyyMMdd
        /// </summary>
        public const string DateTimeFormat_DateString = "yyyyMMdd";

        /// <summary>
        /// HHmmss
        /// </summary>
        public const string DateTimeFormat_TimeString = "HHmmss";

        /// <summary>
        /// yyyy{0}MM{0}dd
        /// </summary>
        public static string DateTimeFormat_DateStringWithSeperator
        {
            get
            {
                return string.Format("yyyy{0}MM{0}dd", DateTimeFormat_DateSeperator);
            }
        }

        /// <summary>
        /// HH{0}mm{0}ss
        /// </summary>
        public static string DateTimeFormat_TimeStringWithSeperator
        {
            get
            {
                return string.Format("HH{0}mm{0}ss", DateTimeFormat_TimeSeperator);
            }
        }
        #endregion

        #region Log
        private static string LogFileName
        {
            get
            {
                return DateTimeHelper.DateTime2DateString(DateTime.Today) + ".log";
            }
        }

        public static string LogFile
        {
            get
            {
                return Path.Combine(Config.LogFolder, LogFileName);
            }
        }
        #endregion

        #region Seperator
        public const string Seperator = "[|]";

        public static string[] Seperators
        {
            get
            {
                return new string[] { Seperator };
            }
        }
        #endregion

        #region Enum
        public enum EnumOrganizationPermission
        {
            None,
            Visible,
            Queryable,
            Editable
        }

        public enum EnumDevice
        {
            Android,
            WPF,
            WindowsMobile,
            WindowsCE
        }

        public enum EnumForm
        {
            RepairForm,
            MaintenanceForm,
            EquipmentPatrolResult
        }

        public enum EnumExcelVersion
        {
            _2003,
            _2007
        }

        public enum EnumSystemModule
        {
            RepairForm,
            EquipmentPatrol,
            EquipmentMaintenance,
            PipelinePatrol,
            TruckPatrol,
            GuardPatrol,
            TankPatrol,
            QForm
        }

        public enum EnumFormAction
        {
            Create,
            Edit
        }

        public enum EnumMoveDirection
        {
            Up,
            Down
        }

        public enum EnumTreeAttribute
        {
            NodeType,
            ToolTip,
            OrganizationPermission,
            OrganizationUniqueID,
            UserID,
            AbnormalType,
            AbnormalReasonUniqueID,
            CheckType,
            CheckItemUniqueID,
            EquipmentUniqueID,
            ControlPointUniqueID,
            RouteUniqueID,
            JobUniqueID,
            EquipmentSpecUniqueID,
            EquipmentType,
            MaterialSpecUniqueID,
            MaterialType,
            MaterialUniqueID,
            HandlingMethodType,
            HandlingMethodUniqueID,
            PartUniqueID,
            MaintenanceType,
            StandardUniqueID,
            FolderUniqueID,
            FileUniqueID,
            EmgContactUniqueID,
            FlowUniqueID,
            IsNew,
            IsError,
            TruckUniqueID,
            RepairFormTypeUniqueID,
            RepairFormColumnUniqueID,
            PipelineUniqueID,
            PipelineSpecUniqueID,
            SpecType,
            PipePointType,
            PipePointUniqueID,
            Class,
            Shift,
            ConstructionUniqueID,
            InspectionUniqueID,
            PipelineAbnormalUniqueID,
            RepairFormSubjectUniqueID,
            ManagerID,
            Manager,
            StationUniqueID,
            IslandUniqueID,
            PortUniqueID
        }

        public enum EnumTreeNodeType
        {
            Organization,
            User,
            AbnormalType,
            AbnormalReason,
            CheckType,
            CheckItem,
            Equipment,
            ControlPoint,
            Route,
            Job,
            EquipmentType,
            EquipmentSpec,
            MaterialType,
            MaterialSpec,
            Material,
            HandlingMethodType,
            HandlingMethod,
            EquipmentPart,
            MaintenanceType,
            Standard,
            Folder,
            File,
            EmgContact,
            Flow,
            Truck,
            RepairFormType,
            RepairFormColumn,
            Pipeline,
            PipelineSpec,
            SpecType,
            PipePoint,
            PipePointType,
            Shift,
            ConstructionRoot,
            InspectionRoot,
            PipelineAbnormalRoot,
            PipePointRoot,
            Construction,
            Inspection,
            PipelineAbnormal,
            RepairFormSubject,
            Station,
            Island,
            Port
        }
        #endregion

        public static SelectListItem DefaultSelectListItem(string DefaultOption)
        {
            return new SelectListItem()
            {
                Selected = true,
                Text = "= " + DefaultOption + " =",
                Value = ""
            };
        }

        public static SelectListItem SelectListItem(string DefaultOption)
        {
            return new SelectListItem()
            {
                Selected = true,
                Text = DefaultOption ,
                Value = DefaultOption
            };
        }

        public static T EnumParse<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        //public static PagerOptions DefaultPagerOptions
        //{
        //    get
        //    {
        //        var _strFormat = "<li>{0}</li>";

        //        return new PagerOptions()
        //        {
        //            AutoHide = true,
        //            PageIndexParameterName = "PageIndex",
        //            FirstPageText = "<i class=\"ace-icon fa fa-angle-double-left\"></i>",
        //            PrevPageText = "<i class=\"ace-icon fa fa-angle-left\"></i>",
        //            NextPageText = "<i class=\"ace-icon fa fa-angle-right\"></i>",
        //            LastPageText = "<i class=\"ace-icon fa fa-angle-double-right\"></i>",
        //            ContainerTagName = "ul",
        //            CssClass = "pagination pull-right no-margin",
        //            CurrentPagerItemTemplate = "<li class=\"active\"><a>{0}</a></li>",
        //            NumericPagerItemTemplate = _strFormat,
        //            MorePagerItemTemplate = _strFormat,
        //            PagerItemTemplate = _strFormat,
        //            //sep = "",
        //            ShowMorePagerItems = true,
        //            NumericPagerItemCount = 5
        //        };
        //    }
        //}

        public static AjaxOptions GetDefaultAjaxOptions()
        {
            var updateTargetId = "divQueryResult";

            return new AjaxOptions
            {
                UpdateTargetId = updateTargetId,
                InsertionMode = InsertionMode.Replace,
                OnBegin = "$('#" + updateTargetId + "').Overlay('show');",
                OnComplete = "$('#" + updateTargetId + "').Overlay('hide');"
            };
        }
        public static PagedListRenderOptions GetDefaultPagerOptions(bool isAjax)
        {

            PagedListRenderOptions result = null;

            var options = new PagedListRenderOptions()
            {
                DisplayLinkToFirstPage = PagedListDisplayMode.Always,
                DisplayLinkToLastPage = PagedListDisplayMode.Always,
                DisplayLinkToPreviousPage = PagedListDisplayMode.Always,
                DisplayLinkToNextPage = PagedListDisplayMode.Always,
                DisplayLinkToIndividualPages = true,
                ContainerDivClasses = null,
                UlElementClasses = new[] { "pagination" },
                ClassToApplyToFirstListItemInPager = null,
                ClassToApplyToLastListItemInPager = null,
                LinkToPreviousPageFormat = "<span class=\"ui-icon ace-icon fa fa-angle-left bigger-140\"></span>",
                LinkToNextPageFormat = "<span class=\"ui-icon ace-icon fa fa-angle-right bigger-140\"></span>",
                LinkToFirstPageFormat = "<span class=\"ui-icon ace-icon fa fa-angle-double-left bigger-140\"></span>",
                LinkToLastPageFormat = "<span class=\"ui-icon ace-icon fa fa-angle-double-right bigger-140\"></span>",
            };

            if (isAjax)
            {
                result = PagedListRenderOptions.EnableUnobtrusiveAjaxReplacing(options, GetDefaultAjaxOptions());
            }
            else
            {
                result = options;
            }

            return options;
        }

    }
}
