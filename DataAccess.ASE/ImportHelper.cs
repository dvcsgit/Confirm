﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.EquipmentMaintenance.Import;
using DbEntity.ASE;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class ImportHelper
    {
        public static ExcelExportModel GetFile(Define.EnumExcelVersion ExcelVersion)
        {
            try
            {
                return new ExcelExportModel("Template", ExcelVersion)
                {
                    Data = System.IO.File.ReadAllBytes(Config.EquipmentMaintenanceImportTemplateFile(ExcelVersion))
                };
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);

                return null;
            }
        }

        public static RequestResult GetProgressFormModel(UploadFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Model.FormInput.Attach != null && Model.FormInput.Attach.ContentLength > 0)
                {
                    string extension = Model.FormInput.Attach.FileName.Substring(Model.FormInput.Attach.FileName.LastIndexOf('.') + 1);

                    if (extension != Define.ExcelExtension_2003 && extension != Define.ExcelExtension_2007)
                    {
                        result.ReturnFailedMessage(Resources.Resource.FileExtensionError);
                    }
                    else
                    {
                        byte[] bytes = new byte[Model.FormInput.Attach.InputStream.Length];

                        Model.FormInput.Attach.InputStream.Read(bytes, 0, bytes.Length);
                        Model.FormInput.Attach.InputStream.Seek(0, SeekOrigin.Begin);

                        var model = new ProgressFormModel();

                        if (extension == Define.ExcelExtension_2003)
                        {
                            model.Workbook = new HSSFWorkbook(new MemoryStream(bytes));
                        }
                        else if (extension == Define.ExcelExtension_2007)
                        {
                            model.Workbook = new XSSFWorkbook(new MemoryStream(bytes));
                        }

                        for (int sheetIndex = 0; sheetIndex < model.Workbook.NumberOfSheets; sheetIndex++)
                        {
                            var sheet = model.Workbook.GetSheetAt(sheetIndex);

                            model.SheetDataCount.Add(sheetIndex, sheet.LastRowNum);
                        }

                        result.ReturnData(model);
                    }
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UploadFileRequired);
                }
            }
            catch(Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetRowData(ProgressFormModel Model, int SheetIndex, int RowIndex)
        {
            RequestResult result = new RequestResult();

            try
            {
                var sheet = Model.Workbook.GetSheetAt(SheetIndex);

                var row = sheet.GetRow(RowIndex);

                switch (SheetIndex)
                { 
                    case 0:
                        Model.RowData.OrganizationList.Add(new OrganizationRowData()
                        {
                            ID = ExcelHelper.GetCellValue(row.GetCell(0)).Trim(),
                            ParentID = ExcelHelper.GetCellValue(row.GetCell(1)).Trim(),
                            Description = ExcelHelper.GetCellValue(row.GetCell(2)).Trim()
                        });
                        break;
                    case 1:
                        Model.RowData.UserList.Add(new UserRowData()
                        {
                            OrganizationID = ExcelHelper.GetCellValue(row.GetCell(0)).Trim(),
                            ID = ExcelHelper.GetCellValue(row.GetCell(1)).Trim(),
                            Name = ExcelHelper.GetCellValue(row.GetCell(2)).Trim(),
                            Title = ExcelHelper.GetCellValue(row.GetCell(3)).Trim(),
                            Email = ExcelHelper.GetCellValue(row.GetCell(4)).Trim(),
                            UID = ExcelHelper.GetCellValue(row.GetCell(5)).Trim()
                        });
                        break;
                    case 2:
                        Model.RowData.UnRFIDReasonList.Add(new UnRFIDReasonRowData()
                        {
                            ID = ExcelHelper.GetCellValue(row.GetCell(0)).Trim(),
                            Description = ExcelHelper.GetCellValue(row.GetCell(1)).Trim()
                        });
                        break;
                    case 3:
                        Model.RowData.OverTimeReasonList.Add(new OverTimeReasonRowData()
                        {
                            ID = ExcelHelper.GetCellValue(row.GetCell(0)).Trim(),
                            Description = ExcelHelper.GetCellValue(row.GetCell(1)).Trim()
                        });
                        break;
                    case 4:
                        Model.RowData.UnPatrolReasonList.Add(new UnPatrolReasonRowData()
                        {
                            ID = ExcelHelper.GetCellValue(row.GetCell(0)).Trim(),
                            Description = ExcelHelper.GetCellValue(row.GetCell(1)).Trim()
                        });
                        break;
                    case 5:
                        Model.RowData.HandlingMethodList.Add(new HandlingMethodRowData()
                        {
                            OrganizationID = ExcelHelper.GetCellValue(row.GetCell(0)).Trim(),
                            HandlingMethodType = ExcelHelper.GetCellValue(row.GetCell(1)).Trim(),
                            ID = ExcelHelper.GetCellValue(row.GetCell(2)).Trim(),
                            Description = ExcelHelper.GetCellValue(row.GetCell(3)).Trim()
                        });
                        break;
                    case 6:
                        Model.RowData.AbnormalReasonList.Add(new AbnormalReasonRowData()
                        {
                            OrganizationID = ExcelHelper.GetCellValue(row.GetCell(0)).Trim(),
                            AbnormalType = ExcelHelper.GetCellValue(row.GetCell(1)).Trim(),
                            ID = ExcelHelper.GetCellValue(row.GetCell(2)).Trim(),
                            Description = ExcelHelper.GetCellValue(row.GetCell(3)).Trim(),
                            HandlingMethodID = ExcelHelper.GetCellValue(row.GetCell(4)).Trim()
                        });
                        break;
                    case 7:
                        Model.RowData.CheckItemList.Add(new CheckItemRowData()
                        {
                            OrganizationID = ExcelHelper.GetCellValue(row.GetCell(0)).Trim(),
                            CheckType = ExcelHelper.GetCellValue(row.GetCell(1)).Trim(),
                            ID = ExcelHelper.GetCellValue(row.GetCell(2)).Trim(),
                            Description = ExcelHelper.GetCellValue(row.GetCell(3)).Trim(),
                            IsFeelItem = ExcelHelper.GetCellValue(row.GetCell(4)).Trim() == "Y",
                            LowerLimit = ExcelHelper.GetCellValue(row.GetCell(5)).Trim(),
                            LowerAlertLimit = ExcelHelper.GetCellValue(row.GetCell(6)).Trim(),
                            UpperAlertLimit = ExcelHelper.GetCellValue(row.GetCell(7)).Trim(),
                            UpperLimit = ExcelHelper.GetCellValue(row.GetCell(8)).Trim(),
                            Unit = ExcelHelper.GetCellValue(row.GetCell(9)).Trim(),
                            Remark = ExcelHelper.GetCellValue(row.GetCell(10)).Trim(),
                            FeelOption = ExcelHelper.GetCellValue(row.GetCell(11)).Trim(),
                            IsFeelOptionAbnormal = ExcelHelper.GetCellValue(row.GetCell(12)).Trim() == "Y",
                            AbnormalReasonID = ExcelHelper.GetCellValue(row.GetCell(13)).Trim()
                        });
                        break;
                    case 8:
                        Model.RowData.EquipmentList.Add(new EquipmentRowData()
                        {
                            OrganizationID = ExcelHelper.GetCellValue(row.GetCell(0)).Trim(),
                            ID = ExcelHelper.GetCellValue(row.GetCell(1)).Trim(),
                            Name = ExcelHelper.GetCellValue(row.GetCell(2)).Trim(),
                            PartDescription = ExcelHelper.GetCellValue(row.GetCell(3)).Trim(),
                            IsFeelItemDefaultNormal = ExcelHelper.GetCellValue(row.GetCell(4)).Trim() == "Y",
                            CheckItemID = ExcelHelper.GetCellValue(row.GetCell(5)).Trim(),
                            LowerLimit = ExcelHelper.GetCellValue(row.GetCell(6)).Trim(),
                            LowerAlertLimit = ExcelHelper.GetCellValue(row.GetCell(7)).Trim(),
                            UpperAlertLimit = ExcelHelper.GetCellValue(row.GetCell(8)).Trim(),
                            UpperLimit = ExcelHelper.GetCellValue(row.GetCell(9)).Trim(),
                            Unit = ExcelHelper.GetCellValue(row.GetCell(10)).Trim(),
                            Remark = ExcelHelper.GetCellValue(row.GetCell(11)).Trim()
                        });
                        break;
                    case 9:
                        Model.RowData.ControlPointList.Add(new ControlPointRowData()
                        {
                            OrganizationID = ExcelHelper.GetCellValue(row.GetCell(0)).Trim(),
                            ID = ExcelHelper.GetCellValue(row.GetCell(1)).Trim(),
                            Description = ExcelHelper.GetCellValue(row.GetCell(2)).Trim(),
                            IsFeelItemDefaultNormal = ExcelHelper.GetCellValue(row.GetCell(3)).Trim() == "Y",
                            TagID = ExcelHelper.GetCellValue(row.GetCell(4)).Trim(),
                            Remark = ExcelHelper.GetCellValue(row.GetCell(5)).Trim(),
                            CheckItemID = ExcelHelper.GetCellValue(row.GetCell(6)).Trim(),
                            LowerLimit = ExcelHelper.GetCellValue(row.GetCell(7)).Trim(),
                            LowerAlertLimit = ExcelHelper.GetCellValue(row.GetCell(8)).Trim(),
                            UpperAlertLimit = ExcelHelper.GetCellValue(row.GetCell(9)).Trim(),
                            UpperLimit = ExcelHelper.GetCellValue(row.GetCell(10)).Trim(),
                            Unit = ExcelHelper.GetCellValue(row.GetCell(11)).Trim(),
                            CheckItemRemark = ExcelHelper.GetCellValue(row.GetCell(12)).Trim()
                        });
                        break;
                    case 10:
                        Model.RowData.RouteList.Add(new RouteRowData()
                        {
                            OrganizationID = ExcelHelper.GetCellValue(row.GetCell(0)).Trim(),
                            ID = ExcelHelper.GetCellValue(row.GetCell(1)).Trim(),
                            Name = ExcelHelper.GetCellValue(row.GetCell(2)).Trim(),
                            ControlPointID = ExcelHelper.GetCellValue(row.GetCell(3)).Trim(),
                            EquipmentID = ExcelHelper.GetCellValue(row.GetCell(4)).Trim(),
                            EquipmentPartDescription = ExcelHelper.GetCellValue(row.GetCell(5)).Trim(),
                            CheckItemID = ExcelHelper.GetCellValue(row.GetCell(6)).Trim()
                        });
                        break;
                }

                result.ReturnData(Model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Validate(ProgressFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new ImportModel()
                {
                    OrganizationList = GetOrganizationModelList("*")
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    #region Organization
                    foreach (var organization in Model.RowData.OrganizationList)
                    {
                        if (organization.ParentID == "*")
                        {
                            model.OrganizationList.Add(new Models.EquipmentMaintenance.Import.OrganizationModel()
                            {
                                UniqueID = Guid.NewGuid().ToString(),
                                ParentUniqueID = "*",
                                ID = organization.ID,
                                Description = organization.Description,
                                IsNewOrganization = true,
                                IsParentError = false,
                                IsExist = db.ORGANIZATION.Any(x => x.PARENTUNIQUEID == "*" && x.ID == organization.ID)
                            });
                        }
                        else
                        {
                            var parentOrganization = GetOrganizationModelByID(model.OrganizationList, organization.ParentID);

                            if (parentOrganization != null)
                            {
                                parentOrganization.OrganizationList.Add(new Models.EquipmentMaintenance.Import.OrganizationModel()
                                {
                                    UniqueID = Guid.NewGuid().ToString(),
                                    ParentUniqueID = parentOrganization.UniqueID,
                                    ID = organization.ID,
                                    Description = organization.Description,
                                    IsNewOrganization = true,
                                    IsParentError = parentOrganization.IsError,
                                    IsExist = db.ORGANIZATION.Any(x => x.PARENTUNIQUEID == parentOrganization.UniqueID && x.ID == organization.ID)
                                });
                            }
                            else
                            {
                                model.NoOrganizationItemList.Add(new NoOrganizationItem()
                                {
                                    DataType = Resources.Resource.Organization,
                                    OrganizationID = organization.ParentID,
                                    ID = organization.ID,
                                    Description = organization.Description
                                });
                            }
                        }
                    }
                    #endregion

                    #region User
                    foreach (var user in Model.RowData.UserList)
                    {
                        var parentOrganization = GetOrganizationModelByID(model.OrganizationList, user.OrganizationID);

                        if (parentOrganization != null)
                        {
                            parentOrganization.UserList.Add(new Models.EquipmentMaintenance.Import.UserModel()
                            {
                                OrganizationUniqueID = parentOrganization.UniqueID,
                                ID = user.ID,
                                Name = user.Name,
                                Title = user.Title,
                                Email = user.Email,
                                UID = user.UID,
                                IsParentError = parentOrganization.IsError,
                                IsExist = db.ACCOUNT.Any(x => x.ID == user.ID)
                            });
                        }
                        else
                        {
                            model.NoOrganizationItemList.Add(new NoOrganizationItem()
                            {
                                DataType = Resources.Resource.User,
                                OrganizationID = user.OrganizationID,
                                ID = user.ID,
                                Description = user.Name
                            });
                        }
                    }
                    #endregion
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    #region UnRFIDReason
                    foreach (var unRFIDReason in Model.RowData.UnRFIDReasonList)
                    {
                        model.UnRFIDReasonList.Add(new UnRFIDReasonModel()
                        {
                            ID = unRFIDReason.ID,
                            Description = unRFIDReason.Description,
                            IsExist = db.UNRFIDREASON.Any(x => x.ID == unRFIDReason.ID)
                        });
                    }
                    #endregion

                    #region OverTimeReason
                    foreach (var overTimeReason in Model.RowData.OverTimeReasonList)
                    {
                        model.OverTimeReasonList.Add(new OverTimeReasonModel()
                        {
                            ID = overTimeReason.ID,
                            Description = overTimeReason.Description,
                            IsExist = db.OVERTIMEREASON.Any(x => x.ID == overTimeReason.ID)
                        });
                    }
                    #endregion

                    #region UnPatrolReason
                    foreach (var unPatrolReason in Model.RowData.UnPatrolReasonList)
                    {
                        model.UnPatrolReasonList.Add(new UnPatrolReasonModel()
                        {
                            ID = unPatrolReason.ID,
                            Description = unPatrolReason.Description,
                            IsExist = db.UNPATROLREASON.Any(x => x.ID == unPatrolReason.ID)
                        });
                    }
                    #endregion

                    #region HandlingMethod
                    foreach (var handlingMethod in Model.RowData.HandlingMethodList)
                    {
                        var parentOrganization = GetOrganizationModelByID(model.OrganizationList, handlingMethod.OrganizationID);

                        if (parentOrganization != null)
                        {
                            var exist = db.HANDLINGMETHOD.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == parentOrganization.UniqueID && x.HANDLINGMETHODTYPE == handlingMethod.HandlingMethodType && x.ID == handlingMethod.ID);

                            parentOrganization.HandlingMethodList.Add(new HandlingMethodModel()
                            {
                                OrganizationUniqueID = parentOrganization.UniqueID,
                                UniqueID = exist != null ? exist.UNIQUEID : Guid.NewGuid().ToString(),
                                HandlingMethodType = handlingMethod.HandlingMethodType,
                                ID = handlingMethod.ID,
                                Description = handlingMethod.Description,
                                IsParentError = parentOrganization.IsError,
                                IsExist = exist != null
                            });
                        }
                        else
                        {
                            model.NoOrganizationItemList.Add(new NoOrganizationItem()
                            {
                                DataType = Resources.Resource.HandlingMethod,
                                OrganizationID = handlingMethod.OrganizationID,
                                ID = handlingMethod.ID,
                                Description = handlingMethod.Description
                            });
                        }
                    }
                    #endregion

                    #region AbnormalReason
                    var abnormalReasonList = Model.RowData.AbnormalReasonList.Select(x => new
                    {
                        x.OrganizationID,
                        x.AbnormalType,
                        x.ID,
                        x.Description
                    }).Distinct().ToList();

                    foreach (var abnormalReason in abnormalReasonList)
                    {
                        var parentOrganization = GetOrganizationModelByID(model.OrganizationList, abnormalReason.OrganizationID);

                        if (parentOrganization != null)
                        {
                            var exist = db.ABNORMALREASON.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == parentOrganization.UniqueID && x.ABNORMALTYPE == abnormalReason.AbnormalType && x.ID == abnormalReason.ID);

                            var abnormalReasonModel = new AbnormalReasonModel()
                            {
                                OrganizationUniqueID = parentOrganization.UniqueID,
                                UniqueID = exist != null ? exist.UNIQUEID : Guid.NewGuid().ToString(),
                                AbnormalType = abnormalReason.AbnormalType,
                                ID = abnormalReason.ID,
                                Description = abnormalReason.Description,
                                IsParentError = parentOrganization.IsError,
                                IsExist = exist != null
                            };

                            var upStreamOrganizationList = GetUpStreamOrganizationList(model.OrganizationList, parentOrganization.UniqueID, true);

                            var abnormalReasonHandlingMethodList = Model.RowData.AbnormalReasonList.Where(x => x.OrganizationID == abnormalReason.OrganizationID && x.AbnormalType == abnormalReason.AbnormalType && x.ID == abnormalReason.ID && !string.IsNullOrEmpty(x.HandlingMethodID)).Select(x => x.HandlingMethodID).Distinct().ToList();

                            foreach (var handlingMethod in abnormalReasonHandlingMethodList)
                            {
                                var handlingMethodModel = GetAbnormalReasonHandlingMethod(upStreamOrganizationList, handlingMethod);

                                handlingMethodModel.IsParentError = abnormalReasonModel.IsError;

                                abnormalReasonModel.HandlingMethodList.Add(handlingMethodModel);
                            }

                            parentOrganization.AbnormalReasonList.Add(abnormalReasonModel);
                        }
                        else
                        {
                            model.NoOrganizationItemList.Add(new NoOrganizationItem()
                            {
                                DataType = Resources.Resource.AbnormalReason,
                                OrganizationID = abnormalReason.OrganizationID,
                                ID = abnormalReason.ID,
                                Description = abnormalReason.Description
                            });
                        }
                    }
                    #endregion

                    #region CheckItem
                    var checkItemList = Model.RowData.CheckItemList.Select(x => new
                    {
                        x.OrganizationID,
                        x.CheckType,
                        x.ID,
                        x.Description,
                        x.IsFeelItem,
                        x.LowerLimit,
                        x.LowerAlertLimit,
                        x.UpperAlertLimit,
                        x.UpperLimit,
                        x.Unit,
                        x.Remark
                    }).Distinct().ToList();

                    foreach (var checkItem in checkItemList)
                    {
                        var parentOrganization = GetOrganizationModelByID(model.OrganizationList, checkItem.OrganizationID);

                        if (parentOrganization != null)
                        {
                            var exist = db.CHECKITEM.FirstOrDefault(x=>x.ORGANIZATIONUNIQUEID==parentOrganization.UniqueID&&x.CHECKTYPE==checkItem.CheckType&&x.ID==checkItem.ID);

                            var checkItemModel = new CheckItemModel()
                            {
                                OrganizationUniqueID = parentOrganization.UniqueID,
                                UniqueID = exist != null ? exist.UNIQUEID : Guid.NewGuid().ToString(),
                                CheckType = checkItem.CheckType,
                                ID = checkItem.ID,
                                Description = checkItem.Description,
                                IsFeelItem = checkItem.IsFeelItem,
                                LowerLimitString = checkItem.LowerLimit,
                                LowerAlertLimitString = checkItem.LowerAlertLimit,
                                UpperAlertLimitString = checkItem.UpperAlertLimit,
                                UpperLimitString = checkItem.UpperLimit,
                                Unit = checkItem.Unit,
                                Remark = checkItem.Remark,
                                IsParentError = parentOrganization.IsError,
                                IsExist = exist != null
                            };

                            if (checkItem.IsFeelItem)
                            {
                                var feelOptionList = Model.RowData.CheckItemList.Where(x => x.OrganizationID == checkItem.OrganizationID && x.CheckType == checkItem.CheckType && x.ID == checkItem.ID).Select(x => new
                                {
                                    x.FeelOption,
                                    x.IsFeelOptionAbnormal
                                }).Distinct().ToList();

                                if (feelOptionList.Count > 0)
                                {
                                    int seq = 1;

                                    foreach (var feelOption in feelOptionList)
                                    {
                                        checkItemModel.FeelOptionList.Add(new FeelOptionModel()
                                        {
                                            Description = feelOption.FeelOption,
                                            IsAbnormal = feelOption.IsFeelOptionAbnormal,
                                            Seq = seq
                                        });

                                        seq++;
                                    }
                                }
                                else
                                {
                                    checkItemModel.FeelOptionList.Add(new FeelOptionModel()
                                    {
                                        Description = Resources.Resource.Normal,
                                        IsAbnormal = false,
                                        Seq = 1
                                    });

                                    checkItemModel.FeelOptionList.Add(new FeelOptionModel()
                                    {
                                        Description = Resources.Resource.Abnormal,
                                        IsAbnormal = true,
                                        Seq = 2
                                    });
                                }
                            }

                            var upStreamOrganizationList = GetUpStreamOrganizationList(model.OrganizationList, parentOrganization.UniqueID, true);

                            var checkItemAbnormalReasonList = Model.RowData.CheckItemList.Where(x => x.OrganizationID == checkItem.OrganizationID && x.CheckType == checkItem.CheckType && x.ID == checkItem.ID && !string.IsNullOrEmpty(x.AbnormalReasonID)).Select(x => x.AbnormalReasonID).Distinct().ToList();

                            foreach (var abnormalReason in checkItemAbnormalReasonList)
                            {
                                var abnormalReasonModel = GetCheckItemAbnormalReason(upStreamOrganizationList, abnormalReason);

                                abnormalReasonModel.IsParentError = checkItemModel.IsError;

                                checkItemModel.AbnormalReasonList.Add(abnormalReasonModel);
                            }

                            parentOrganization.CheckItemList.Add(checkItemModel);
                        }
                        else
                        {
                            model.NoOrganizationItemList.Add(new NoOrganizationItem()
                            {
                                DataType = Resources.Resource.CheckItem,
                                OrganizationID = checkItem.OrganizationID,
                                ID = checkItem.ID,
                                Description = checkItem.Description
                            });
                        }
                    }
                    #endregion

                    #region Equipment
                    var equipmentList = Model.RowData.EquipmentList.Select(x => new
                    {
                        x.OrganizationID,
                        x.ID,
                        x.Name,
                        x.PartDescription,
                        x.IsFeelItemDefaultNormal
                    }).Distinct().ToList();

                    foreach (var equipment in equipmentList)
                    {
                        var parentOrganization = GetOrganizationModelByID(model.OrganizationList, equipment.OrganizationID);

                        if (parentOrganization != null)
                        {
                            var equipmentExist = db.EQUIPMENT.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == parentOrganization.UniqueID && x.ID == equipment.ID);

                            var equipmentModel = new EquipmentModel()
                            {
                                OrganizationUniqueID = parentOrganization.UniqueID,
                                UniqueID = equipmentExist != null ? equipmentExist.UNIQUEID : Guid.NewGuid().ToString(),
                                ID = equipment.ID,
                                Name = equipment.Name,
                                PartUniqueID = "*",
                                PartDescription = string.IsNullOrEmpty(equipment.PartDescription) || equipment.PartDescription == "*" ? "" : equipment.PartDescription,
                                IsParentError = parentOrganization.IsError,
                                IsFeelItemDefaultNormal = equipment.IsFeelItemDefaultNormal
                            };

                            equipmentModel.IsExist = equipmentExist != null;

                            if (!string.IsNullOrEmpty(equipmentModel.PartDescription) && equipmentModel.IsExist)
                            {
                                var partExist = db.EQUIPMENTPART.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == equipmentExist.UNIQUEID && x.DESCRIPTION == equipment.PartDescription);

                                equipmentModel.UniqueID = partExist != null ? partExist.UNIQUEID : Guid.NewGuid().ToString();
                                equipmentModel.IsExist = partExist != null;
                            }

                            var upStreamOrganizationList = GetUpStreamOrganizationList(model.OrganizationList, parentOrganization.UniqueID, true);

                            var equipmentCheckItemList = Model.RowData.EquipmentList.Where(x => x.OrganizationID == equipment.OrganizationID && x.ID == equipment.ID && x.PartDescription == equipment.PartDescription && !string.IsNullOrEmpty(x.CheckItemID)).Select(x => new { x.CheckItemID, x.LowerLimit, x.LowerAlertLimit, x.UpperAlertLimit, x.UpperLimit, x.Unit, x.Remark }).Distinct().ToList();

                            foreach (var checkItem in equipmentCheckItemList)
                            {
                                var checkItemModel = GetEquipmentCheckItem(upStreamOrganizationList, checkItem.CheckItemID);

                                checkItemModel.IsParentError = equipmentModel.IsError;
                                checkItemModel.LowerLimitString = checkItem.LowerLimit;
                                checkItemModel.LowerAlertLimitString = checkItem.LowerAlertLimit;
                                checkItemModel.UpperAlertLimitString = checkItem.UpperAlertLimit;
                                checkItemModel.UpperLimitString = checkItem.UpperLimit;
                                checkItemModel.Unit = checkItem.Unit;
                                checkItemModel.Remark = checkItem.Remark;

                                equipmentModel.CheckItemList.Add(checkItemModel);
                            }

                            parentOrganization.EquipmentList.Add(equipmentModel);
                        }
                        else
                        {
                            model.NoOrganizationItemList.Add(new NoOrganizationItem()
                            {
                                DataType = Resources.Resource.Equipment,
                                OrganizationID = equipment.OrganizationID,
                                ID = equipment.ID,
                                Description = equipment.Name
                            });
                        }
                    }
                    #endregion

                    #region ControlPoint
                    var controlPointList = Model.RowData.ControlPointList.Select(x => new
                    {
                        x.OrganizationID,
                        x.ID,
                        x.Description,
                        x.IsFeelItemDefaultNormal,
                        x.TagID,
                        x.Remark
                    }).Distinct().ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var parentOrganization = GetOrganizationModelByID(model.OrganizationList, controlPoint.OrganizationID);

                        if (parentOrganization != null)
                        {
                            var exist = db.CONTROLPOINT.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == parentOrganization.UniqueID && x.ID == controlPoint.ID);

                            var controlPointModel = new ControlPointModel()
                            {
                                OrganizationUniqueID = parentOrganization.UniqueID,
                                UniqueID = exist != null ? exist.UNIQUEID : Guid.NewGuid().ToString(),
                                ID = controlPoint.ID,
                                Description = controlPoint.Description,
                                IsFeelItemDefaultNormal = controlPoint.IsFeelItemDefaultNormal,
                                TagID = controlPoint.TagID,
                                Remark = controlPoint.Remark,
                                IsParentError = parentOrganization.IsError,
                                IsExist = exist != null
                            };

                            var upStreamOrganizationList = GetUpStreamOrganizationList(model.OrganizationList, parentOrganization.UniqueID, true);

                            var controlPointCheckItemList = Model.RowData.ControlPointList.Where(x => x.OrganizationID == controlPoint.OrganizationID && x.ID == controlPoint.ID && !string.IsNullOrEmpty(x.CheckItemID)).Select(x => new { x.CheckItemID, x.LowerLimit, x.LowerAlertLimit, x.UpperAlertLimit, x.UpperLimit, x.Unit, x.Remark }).Distinct().ToList();

                            foreach (var checkItem in controlPointCheckItemList)
                            {
                                var checkItemModel = GetControlPointCheckItem(upStreamOrganizationList, checkItem.CheckItemID);

                                checkItemModel.IsParentError = controlPointModel.IsError;
                                checkItemModel.LowerLimitString = checkItem.LowerLimit;
                                checkItemModel.LowerAlertLimitString = checkItem.LowerAlertLimit;
                                checkItemModel.UpperAlertLimitString = checkItem.UpperAlertLimit;
                                checkItemModel.UpperLimitString = checkItem.UpperLimit;
                                checkItemModel.Unit = checkItem.Unit;
                                checkItemModel.Remark = checkItem.Remark;

                                controlPointModel.CheckItemList.Add(checkItemModel);
                            }

                            parentOrganization.ControlPointList.Add(controlPointModel);
                        }
                        else
                        {
                            model.NoOrganizationItemList.Add(new NoOrganizationItem()
                            {
                                DataType = Resources.Resource.ControlPoint,
                                OrganizationID = controlPoint.OrganizationID,
                                ID = controlPoint.ID,
                                Description = controlPoint.Description
                            });
                        }
                    }
                    #endregion

                    #region Route
                    var routeList = Model.RowData.RouteList.Select(x => new
                    {
                        x.OrganizationID,
                        x.ID,
                        x.Name
                    }).Distinct().ToList();

                    foreach (var route in routeList)
                    {
                        var parentOrganization = GetOrganizationModelByID(model.OrganizationList, route.OrganizationID);

                        if (parentOrganization != null)
                        {
                            var exist = db.ROUTE.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == parentOrganization.UniqueID && x.ID == route.ID);

                            var routeModel = new RouteModel()
                            {
                                OrganizationUniqueID = parentOrganization.UniqueID,
                                UniqueID = exist != null ? exist.UNIQUEID : Guid.NewGuid().ToString(),
                                ID = route.ID,
                                Name = route.Name,
                                IsParentError = parentOrganization.IsError,
                                IsExist = exist != null
                            };

                            var availableOrganizationList = new List<Models.EquipmentMaintenance.Import.OrganizationModel>();

                            availableOrganizationList.AddRange(GetDownStreamStreamOrganizationList(model.OrganizationList, parentOrganization.UniqueID, true));
                            availableOrganizationList.AddRange(GetUpStreamOrganizationList(model.OrganizationList, parentOrganization.UniqueID, false));

                            var routeControlPointList = Model.RowData.RouteList.Where(x => x.OrganizationID == route.OrganizationID && x.ID == route.ID).Select(x => x.ControlPointID).Distinct().ToList();

                            foreach (var routeControlPoint in routeControlPointList)
                            {
                                var routeControlPointModel = GetRouteControlPoint(availableOrganizationList, routeControlPoint);

                                routeControlPointModel.IsParentError = routeModel.IsError;

                                var routeControlPointCheckItemList = Model.RowData.RouteList.Where(x => x.OrganizationID == route.OrganizationID && x.ID == route.ID && x.ControlPointID == routeControlPoint && string.IsNullOrEmpty(x.EquipmentID)).Select(x => x.CheckItemID).Distinct().ToList();

                                foreach (var routeControlPointCheckItem in routeControlPointCheckItemList)
                                {
                                    var routeControlPointCheckItemModel = GetRouteControlPointCheckItem(availableOrganizationList, routeControlPoint, routeControlPointCheckItem);

                                    routeControlPointCheckItemModel.IsParentError = routeControlPointModel.IsError;

                                    routeControlPointModel.CheckItemList.Add(routeControlPointCheckItemModel);
                                }

                                var routeEquipmentList = Model.RowData.RouteList.Where(x => x.OrganizationID == route.OrganizationID && x.ID == route.ID && x.ControlPointID == routeControlPoint && !string.IsNullOrEmpty(x.EquipmentID)).Select(x => new { x.EquipmentID, x.EquipmentPartDescription }).Distinct().ToList();

                                foreach (var routeEquipment in routeEquipmentList)
                                {
                                    var partDescription = string.IsNullOrEmpty(routeEquipment.EquipmentPartDescription) || routeEquipment.EquipmentPartDescription == "*" ? "" : routeEquipment.EquipmentPartDescription;

                                    var routeEquipmentModel = GetRouteEquipment(availableOrganizationList, routeEquipment.EquipmentID, partDescription);

                                    routeEquipmentModel.IsParentError = routeControlPointModel.IsError;

                                    var routeEquipmentCheckItemList = Model.RowData.RouteList.Where(x => x.OrganizationID == route.OrganizationID && x.ID == route.ID && x.ControlPointID == routeControlPoint && x.EquipmentID == routeEquipment.EquipmentID && x.EquipmentPartDescription == routeEquipment.EquipmentPartDescription).Select(x => x.CheckItemID).Distinct().ToList();

                                    foreach (var routeEquipmentCheckItem in routeEquipmentCheckItemList)
                                    {
                                        var routeEquipmentCheckItemModel = GetRouteEquipmentCheckItem(availableOrganizationList, routeEquipment.EquipmentID, partDescription, routeEquipmentCheckItem);

                                        routeEquipmentCheckItemModel.IsParentError = routeEquipmentModel.IsError;

                                        routeEquipmentModel.CheckItemList.Add(routeEquipmentCheckItemModel);
                                    }

                                    routeControlPointModel.EquipmentList.Add(routeEquipmentModel);
                                }

                                routeModel.ControlPointList.Add(routeControlPointModel);
                            }

                            parentOrganization.RouteList.Add(routeModel);
                        }
                        else
                        {
                            model.NoOrganizationItemList.Add(new NoOrganizationItem()
                            {
                                DataType = Resources.Resource.Route,
                                OrganizationID = route.OrganizationID,
                                ID = route.ID,
                                Description = route.Name
                            });
                        }
                    }
                    #endregion
                }

                result.ReturnData(model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Import(ImportModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
#if !DEBUG
                using (TransactionScope trans = new TransactionScope())
                {
#endif
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        if (Model.HaveNewOrganization)
                        {
                            foreach (var organization in Model.OrganizationList.Where(x => x.HaveNewOrganization))
                            {
                                db.ORGANIZATION.AddRange(organization.ValidOrganizationList.Select(x => new ORGANIZATION()
                                {
                                    UNIQUEID = x.UniqueID,
                                    PARENTUNIQUEID = x.ParentUniqueID,
                                    ID = x.ID,
                                    DESCRIPTION = x.Description
                                }).ToList());
                            }
                        }

                        if (Model.HaveNewUser)
                        {
                            foreach (var organization in Model.OrganizationList.Where(x => x.HaveNewUser))
                            {
                                db.ACCOUNT.AddRange(organization.ValidUserList.Select(x => new ACCOUNT()
                                {
                                    ORGANIZATIONUNIQUEID = x.OrganizationUniqueID,
                                    ID = x.ID,
                                    NAME = x.Name,
                                    PASSWORD = x.ID,
                                    TITLE = x.Title,
                                    EMAIL = x.Email,
                                    TAGID = x.UID,
                                    LASTMODIFYTIME = DateTime.Now
                                }).ToList());
                            }
                        }

                        db.SaveChanges();
                    }

                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        if (Model.HaveNewUnRFIDReason)
                        {
                            foreach (var reason in Model.UnRFIDReasonList.Where(x => !x.IsError))
                            {
                                db.UNRFIDREASON.Add(new UNRFIDREASON()
                                {
                                    UNIQUEID = Guid.NewGuid().ToString(),
                                    ID = reason.ID,
                                    DESCRIPTION = reason.Description,
                                    LASTMODIFYTIME = DateTime.Now
                                });
                            }
                        }

                        if (Model.HaveNewOverTimeReason)
                        {
                            foreach (var reason in Model.OverTimeReasonList.Where(x=>!x.IsError))
                            {
                                db.OVERTIMEREASON.Add(new OVERTIMEREASON()
                                {
                                    UNIQUEID = Guid.NewGuid().ToString(),
                                    ID = reason.ID,
                                    DESCRIPTION = reason.Description,
                                    LASTMODIFYTIME = DateTime.Now
                                });
                            }
                        }

                        if (Model.HaveNewUnPatrolReason)
                        {
                            foreach (var reason in Model.UnPatrolReasonList.Where(x=>!x.IsError))
                            {
                                db.UNPATROLREASON.Add(new UNPATROLREASON()
                                {
                                    UNIQUEID = Guid.NewGuid().ToString(),
                                    ID = reason.ID,
                                    DESCRIPTION = reason.Description,
                                    LASTMODIFYTIME = DateTime.Now
                                });
                            }
                        }

                        if (Model.HaveNewHandlingMethod)
                        {
                            foreach (var organization in Model.OrganizationList.Where(x => x.HaveNewHandlingMethod))
                            {
                                db.HANDLINGMETHOD.AddRange(organization.ValidHandlingMethodList.Select(x => new HANDLINGMETHOD()
                                {
                                    ORGANIZATIONUNIQUEID = x.OrganizationUniqueID,
                                    UNIQUEID = x.UniqueID,
                                    HANDLINGMETHODTYPE = x.HandlingMethodType,
                                    ID = x.ID,
                                    DESCRIPTION = x.Description,
                                    LASTMODIFYTIME = DateTime.Now
                                }).ToList());
                            }
                        }

                        if (Model.HaveNewAbnormalReason)
                        {
                            foreach (var organization in Model.OrganizationList.Where(x => x.HaveNewAbnormalReason))
                            {
                                foreach (var abnormalReason in organization.ValidAbnormalReasonList)
                                {
                                    db.ABNORMALREASON.Add(new ABNORMALREASON()
                                    {
                                        UNIQUEID = abnormalReason.UniqueID,
                                        ORGANIZATIONUNIQUEID = abnormalReason.OrganizationUniqueID,
                                        ABNORMALTYPE = abnormalReason.AbnormalType,
                                        ID = abnormalReason.ID,
                                        DESCRIPTION = abnormalReason.Description,
                                        LASTMODIFYTIME = DateTime.Now
                                    });

                                    db.ABNORMALREASONHANDLINGMETHOD.AddRange(abnormalReason.HandlingMethodList.Where(x => !x.IsError).Select(x => new ABNORMALREASONHANDLINGMETHOD()
                                    {
                                        ABNORMALREASONUNIQUEID = abnormalReason.UniqueID,
                                        HANDLINGMETHODUNIQUEID = x.UniqueID
                                    }).ToList());
                                }
                            }
                        }

                        if (Model.HaveNewCheckItem)
                        {
                            foreach (var organization in Model.OrganizationList.Where(x => x.HaveNewCheckItem))
                            {
                                foreach (var checkItem in organization.ValidCheckItemList)
                                {
                                    db.CHECKITEM.Add(new CHECKITEM()
                                    {
                                        UNIQUEID = checkItem.UniqueID,
                                        ORGANIZATIONUNIQUEID = checkItem.OrganizationUniqueID,
                                        CHECKTYPE = checkItem.CheckType,
                                        ID = checkItem.ID,
                                        DESCRIPTION = checkItem.Description,
                                        ISFEELITEM = checkItem.IsFeelItem?"Y":"N",
                                        ISACCUMULATION = "N",
                                        LOWERLIMIT = checkItem.LowerLimit.HasValue ? decimal.Parse(checkItem.LowerLimit.Value.ToString()) : default(decimal?),
                                        LOWERALERTLIMIT = checkItem.LowerAlertLimit.HasValue ? decimal.Parse(checkItem.LowerAlertLimit.Value.ToString()) : default(decimal?),
                                        UPPERALERTLIMIT = checkItem.UpperAlertLimit.HasValue ? decimal.Parse(checkItem.UpperAlertLimit.Value.ToString()) : default(decimal?),
                                        UPPERLIMIT = checkItem.UpperLimit.HasValue ? decimal.Parse(checkItem.UpperLimit.Value.ToString()) : default(decimal?),
                                        UNIT = checkItem.Unit,
                                        REMARK = checkItem.Remark,
                                        LASTMODIFYTIME = DateTime.Now
                                    });

                                    if (checkItem.IsFeelItem && checkItem.FeelOptionList != null && checkItem.FeelOptionList.Count > 0)
                                    {
                                        db.CHECKITEMFEELOPTION.AddRange(checkItem.FeelOptionList.Select(x => new CHECKITEMFEELOPTION()
                                        {
                                            UNIQUEID = Guid.NewGuid().ToString(),
                                            CHECKITEMUNIQUEID = checkItem.UniqueID,
                                            DESCRIPTION = x.Description,
                                            ISABNORMAL = x.IsAbnormal ? "Y" : "N",
                                            SEQ = x.Seq
                                        }).ToList());
                                    }

                                    db.CHECKITEMABNORMALREASON.AddRange(checkItem.AbnormalReasonList.Where(x => !x.IsError).Select(x => new CHECKITEMABNORMALREASON()
                                    {
                                        CHECKITEMUNIQUEID = checkItem.UniqueID,
                                        ABNORMALREASONUNIQUEID = x.UniqueID
                                    }).ToList());
                                }
                            }
                        }

                        if (Model.HaveNewEquipment)
                        {
                            var createdEquipmentList = new List<string>();

                            foreach (var organization in Model.OrganizationList.Where(x => x.HaveNewEquipment))
                            {
                                foreach (var equipment in organization.ValidEquipmentList)
                                {
                                    if (!createdEquipmentList.Contains(equipment.UniqueID))
                                    {
                                        db.EQUIPMENT.Add(new EQUIPMENT()
                                        {
                                            UNIQUEID = equipment.UniqueID,
                                            ORGANIZATIONUNIQUEID = equipment.OrganizationUniqueID,
                                            ID = equipment.ID,
                                            NAME = equipment.Name,
                                            ISFEELITEMDEFAULTNORMAL = equipment.IsFeelItemDefaultNormal ? "Y" : "N",
                                            LASTMODIFYTIME = DateTime.Now
                                        });

                                        createdEquipmentList.Add(equipment.UniqueID);
                                    }

                                    if (equipment.PartUniqueID != "*")
                                    {
                                        db.EQUIPMENTPART.Add(new EQUIPMENTPART()
                                        {
                                            UNIQUEID = equipment.PartUniqueID,
                                            EQUIPMENTUNIQUEID = equipment.UniqueID,
                                            DESCRIPTION = equipment.PartDescription
                                        });
                                    }

                                    db.EQUIPMENTCHECKITEM.AddRange(equipment.CheckItemList.Where(x => !x.IsError).Select(x => new EQUIPMENTCHECKITEM()
                                    {
                                        EQUIPMENTUNIQUEID = equipment.UniqueID,
                                        PARTUNIQUEID = equipment.PartUniqueID,
                                        CHECKITEMUNIQUEID = x.UniqueID,
                                        ISINHERIT = x.IsInherit ? "Y" : "N",
                                        LOWERLIMIT = x.LowerLimit.HasValue ? decimal.Parse(x.LowerLimit.Value.ToString()) : default(decimal?),
                                        LOWERALERTLIMIT = x.LowerAlertLimit.HasValue ? decimal.Parse(x.LowerAlertLimit.Value.ToString()) : default(decimal?),
                                        UPPERALERTLIMIT = x.UpperAlertLimit.HasValue ? decimal.Parse(x.UpperAlertLimit.Value.ToString()) : default(decimal?),
                                        UPPERLIMIT = x.UpperLimit.HasValue ? decimal.Parse(x.UpperLimit.Value.ToString()) : default(decimal?),
                                        UNIT = x.Unit,
                                        REMARK = x.Remark
                                    }).ToList());
                                }
                            }
                        }

                        if (Model.HaveNewControlPoint)
                        {
                            foreach (var organization in Model.OrganizationList.Where(x => x.HaveNewControlPoint))
                            {
                                foreach (var controlPoint in organization.ValidControlPointList)
                                {
                                    db.CONTROLPOINT.Add(new CONTROLPOINT()
                                    {
                                        UNIQUEID = controlPoint.UniqueID,
                                        ORGANIZATIONUNIQUEID = controlPoint.OrganizationUniqueID,
                                        ID = controlPoint.ID,
                                        DESCRIPTION = controlPoint.Description,
                                        ISFEELITEMDEFAULTNORMAL = controlPoint.IsFeelItemDefaultNormal ? "Y" : "N",
                                        TAGID = controlPoint.TagID,
                                        REMARK = controlPoint.Remark,
                                        LASTMODIFYTIME = DateTime.Now
                                    });

                                    db.CONTROLPOINTCHECKITEM.AddRange(controlPoint.CheckItemList.Where(x => !x.IsError).Select(x => new CONTROLPOINTCHECKITEM()
                                    {
                                        CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                        CHECKITEMUNIQUEID = x.UniqueID,
                                        ISINHERIT = x.IsInherit ? "Y" : "N",
                                        LOWERLIMIT = x.LowerLimit.HasValue?decimal.Parse(x.LowerLimit.Value.ToString()):default(decimal?),
                                        LOWERALERTLIMIT = x.LowerAlertLimit.HasValue ? decimal.Parse(x.LowerAlertLimit.Value.ToString()) : default(decimal?),
                                        UPPERALERTLIMIT = x.UpperAlertLimit.HasValue ? decimal.Parse(x.UpperAlertLimit.Value.ToString()) : default(decimal?),
                                        UPPERLIMIT = x.UpperLimit.HasValue ? decimal.Parse(x.UpperLimit.Value.ToString()) : default(decimal?),
                                        UNIT = x.Unit,
                                        REMARK = x.Remark
                                    }).ToList());
                                }
                            }
                        }

                        if (Model.HaveNewRoute)
                        {
                            foreach (var organization in Model.OrganizationList.Where(x => x.HaveNewRoute))
                            {
                                foreach (var route in organization.ValidRouteList)
                                {
                                    db.ROUTE.Add(new ROUTE()
                                    {
                                        UNIQUEID = route.UniqueID,
                                        ORGANIZATIONUNIQUEID = route.OrganizationUniqueID,
                                        ID = route.ID,
                                        NAME = route.Name,
                                        LASTMODIFYTIME = DateTime.Now
                                    });

                                    foreach (var controlPoint in route.ControlPointList.Where(x => !x.IsError))
                                    {
                                        db.ROUTECONTROLPOINT.Add(new ROUTECONTROLPOINT()
                                        {
                                            ROUTEUNIQUEID = route.UniqueID,
                                            CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                            SEQ = route.ControlPointList.IndexOf(controlPoint)
                                        });

                                        db.ROUTECONTROLPOINTCHECKITEM.AddRange(controlPoint.CheckItemList.Where(x => !x.IsError).Select(x => new ROUTECONTROLPOINTCHECKITEM()
                                        {
                                            ROUTEUNIQUEID = route.UniqueID,
                                            CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                            CHECKITEMUNIQUEID = x.UniqueID,
                                            SEQ = controlPoint.CheckItemList.IndexOf(x)
                                        }).ToList());

                                        foreach (var equipment in controlPoint.EquipmentList.Where(x => !x.IsError))
                                        {
                                            db.ROUTEEQUIPMENT.Add(new ROUTEEQUIPMENT()
                                            {
                                                ROUTEUNIQUEID = route.UniqueID,
                                                CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                                EQUIPMENTUNIQUEID = equipment.UniqueID,
                                                PARTUNIQUEID = equipment.PartUniqueID,
                                                SEQ = controlPoint.EquipmentList.IndexOf(equipment)
                                            });

                                            db.ROUTEEQUIPMENTCHECKITEM.AddRange(equipment.CheckItemList.Where(x => !x.IsError).Select(x => new ROUTEEQUIPMENTCHECKITEM()
                                            {
                                                ROUTEUNIQUEID = route.UniqueID,
                                                CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                                EQUIPMENTUNIQUEID = equipment.UniqueID,
                                                PARTUNIQUEID = equipment.PartUniqueID,
                                                CHECKITEMUNIQUEID = x.UniqueID,
                                                SEQ = equipment.CheckItemList.IndexOf(x)
                                            }).ToList());
                                        }
                                    }
                                }
                            }
                        }

                        db.SaveChanges();
                    }
#if !DEBUG
                    trans.Complete();
                }
#endif

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Import, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private static List<Models.EquipmentMaintenance.Import.OrganizationModel> GetOrganizationModelList(string ParentUniqueID)
        {
            var organizationModelList = new List<Models.EquipmentMaintenance.Import.OrganizationModel>();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var organizationList = db.ORGANIZATION.Where(x => x.PARENTUNIQUEID == ParentUniqueID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var organizationModel = new Models.EquipmentMaintenance.Import.OrganizationModel()
                        {
                            UniqueID = organization.UNIQUEID,
                            ParentUniqueID = organization.PARENTUNIQUEID,
                            ID = organization.ID,
                            Description = organization.DESCRIPTION,
                            IsNewOrganization = false,
                            IsExist = false,
                            IsParentError = false
                        };

                        if (db.ORGANIZATION.Any(x => x.PARENTUNIQUEID == organization.UNIQUEID))
                        {
                            organizationModel.OrganizationList = GetOrganizationModelList(organization.UNIQUEID);
                        }

                        organizationModelList.Add(organizationModel);
                    }
                }
            }
            catch(Exception ex)
            {
                organizationModelList = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return organizationModelList;
        }

        private static Models.EquipmentMaintenance.Import.OrganizationModel GetOrganizationModelByUniqueID(List<Models.EquipmentMaintenance.Import.OrganizationModel> OrganizationModelList, string OrganizationUniqueID)
        {
            foreach (var organizationModel in OrganizationModelList)
            {
                if (organizationModel.UniqueID == OrganizationUniqueID)
                {
                    return organizationModel;
                }
                else
                {
                    var organization = GetOrganizationModelByUniqueID(organizationModel.OrganizationList, OrganizationUniqueID);

                    if (organization != null)
                    {
                        return organization;
                    }
                }
            }

            return null;
        }

        private static Models.EquipmentMaintenance.Import.OrganizationModel GetOrganizationModelByID(List<Models.EquipmentMaintenance.Import.OrganizationModel> OrganizationModelList, string OrganizationID)
        {
            foreach (var organizationModel in OrganizationModelList)
            {
                if (organizationModel.ID == OrganizationID)
                {
                    return organizationModel;
                }
                else
                {
                    var organization = GetOrganizationModelByID(organizationModel.OrganizationList, OrganizationID);

                    if (organization != null)
                    {
                        return organization;
                    }
                }
            }

            return null;
        }

        private static List<Models.EquipmentMaintenance.Import.OrganizationModel> GetUpStreamOrganizationList(List<Models.EquipmentMaintenance.Import.OrganizationModel> OrganizationModelList, string OrganizationUniqueID, bool Include)
        {
            var itemList = new List<Models.EquipmentMaintenance.Import.OrganizationModel>();

            try
            {
                var organization = GetOrganizationModelByUniqueID(OrganizationModelList, OrganizationUniqueID);

                if (Include)
                {
                    itemList.Add(organization);
                }

                while (organization.ParentUniqueID != "*")
                {
                    organization = GetOrganizationModelByUniqueID(OrganizationModelList, organization.ParentUniqueID);

                    itemList.Add(organization);
                }
            }
            catch (Exception ex)
            {
                itemList = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return itemList;
        }

        private static List<Models.EquipmentMaintenance.Import.OrganizationModel> GetDownStreamStreamOrganizationList(List<Models.EquipmentMaintenance.Import.OrganizationModel> OrganizationModelList, string OrganizationUniqueID, bool Include)
        {
            var itemList = new List<Models.EquipmentMaintenance.Import.OrganizationModel>();

            try
            {
                var organization = GetOrganizationModelByUniqueID(OrganizationModelList, OrganizationUniqueID);

                if (Include)
                {
                    itemList.Add(organization);
                }

                var organizationList = organization.OrganizationList;

                while (organizationList != null && organizationList.Count > 0)
                {
                    itemList.AddRange(organizationList);

                    organizationList = organizationList.SelectMany(x => x.OrganizationList).ToList();
                }
            }
            catch (Exception ex)
            {
                itemList = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return itemList;
        }

        private static AbnormalReasonHandlingMethodModel GetAbnormalReasonHandlingMethod(List<Models.EquipmentMaintenance.Import.OrganizationModel> OrganizationMdodelList, string HandlingMethodID)
        {
            foreach (var organization in OrganizationMdodelList)
            {
                var handlingMethodModel = organization.HandlingMethodList.FirstOrDefault(x => x.ID == HandlingMethodID);

                if (handlingMethodModel != null)
                {
                    return new AbnormalReasonHandlingMethodModel()
                    {
                        UniqueID = handlingMethodModel.UniqueID,
                        ID = handlingMethodModel.ID,
                        Description = handlingMethodModel.Description,
                        IsExist = true
                    };
                }
               
                if (!organization.IsNewOrganization)
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var handlingMethod = db.HANDLINGMETHOD.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID && x.ID == HandlingMethodID);

                        if (handlingMethod != null)
                        {
                            return new AbnormalReasonHandlingMethodModel()
                            {
                                UniqueID = handlingMethod.UNIQUEID,
                                ID = handlingMethod.ID,
                                Description = handlingMethod.DESCRIPTION,
                                IsExist = true
                            };
                        }
                    }
                }
               
                return GetAbnormalReasonHandlingMethod(organization.OrganizationList, HandlingMethodID);
            }

            return new AbnormalReasonHandlingMethodModel()
            {
                UniqueID = string.Empty,
                ID = HandlingMethodID,
                Description = string.Empty,
                IsExist = false
            };
        }

        private static CheckItemAbnormalReasonModel GetCheckItemAbnormalReason(List<Models.EquipmentMaintenance.Import.OrganizationModel> OrganizationMdodelList, string AbnormalReasonID)
        {
            foreach (var organization in OrganizationMdodelList)
            {
                var abnormalReasonModel = organization.AbnormalReasonList.FirstOrDefault(x => x.ID == AbnormalReasonID);

                if (abnormalReasonModel != null)
                {
                    return new CheckItemAbnormalReasonModel()
                    {
                        UniqueID = abnormalReasonModel.UniqueID,
                        ID = abnormalReasonModel.ID,
                        Description = abnormalReasonModel.Description,
                        IsExist = true
                    };
                }

                if (!organization.IsNewOrganization)
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var abnormalReason = db.ABNORMALREASON.Where(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID && x.ID == AbnormalReasonID).FirstOrDefault();

                        if (abnormalReason != null)
                        {
                            return new CheckItemAbnormalReasonModel()
                            {
                                UniqueID = abnormalReason.UNIQUEID,
                                ID = abnormalReason.ID,
                                Description = abnormalReason.DESCRIPTION,
                                IsExist = true
                            };
                        }
                    }
                }

                return GetCheckItemAbnormalReason(organization.OrganizationList, AbnormalReasonID);
            }

            return new CheckItemAbnormalReasonModel()
            {
                UniqueID = string.Empty,
                ID = AbnormalReasonID,
                Description = string.Empty,
                IsExist = false
            };
        }

        private static EquipmentCheckItemModel GetEquipmentCheckItem(List<Models.EquipmentMaintenance.Import.OrganizationModel> OrganizationMdodelList, string CheckItemID)
        {
            foreach (var organization in OrganizationMdodelList)
            {
                var checkItemModel = organization.CheckItemList.FirstOrDefault(x => x.ID == CheckItemID);

                if (checkItemModel != null)
                {
                    return new EquipmentCheckItemModel()
                    {
                        UniqueID = checkItemModel.UniqueID,
                        ID = checkItemModel.ID,
                        Description = checkItemModel.Description,
                        IsExist = true
                    };
                }

                if (!organization.IsNewOrganization)
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var checkItem = db.CHECKITEM.Where(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID && x.ID == CheckItemID).FirstOrDefault();

                        if (checkItem != null)
                        {
                            return new EquipmentCheckItemModel()
                            {
                                UniqueID = checkItem.UNIQUEID,
                                ID = checkItem.ID,
                                Description = checkItem.DESCRIPTION,
                                IsExist = true
                            };
                        }
                    }
                }

                return GetEquipmentCheckItem(organization.OrganizationList, CheckItemID);
            }

            return new EquipmentCheckItemModel()
            {
                UniqueID=string.Empty,
                ID = CheckItemID,
                Description = string.Empty,
                IsExist = false
            };
        }

        private static ControlPointCheckItemModel GetControlPointCheckItem(List<Models.EquipmentMaintenance.Import.OrganizationModel> OrganizationMdodelList, string CheckItemID)
        {
            foreach (var organization in OrganizationMdodelList)
            {
                var checkItemModel = organization.CheckItemList.FirstOrDefault(x => x.ID == CheckItemID);

                if (checkItemModel != null)
                {
                    return new ControlPointCheckItemModel()
                    {
                        UniqueID = checkItemModel.UniqueID,
                        ID = checkItemModel.ID,
                        Description = checkItemModel.Description,
                        IsExist = true
                    };
                }

                if (!organization.IsNewOrganization)
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var checkItem = db.CHECKITEM.Where(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID && x.ID == CheckItemID).FirstOrDefault();

                        if (checkItem != null)
                        {
                            return new ControlPointCheckItemModel()
                            {
                                UniqueID = checkItem.UNIQUEID,
                                ID = checkItem.ID,
                                Description = checkItem.DESCRIPTION,
                                IsExist = true
                            };
                        }
                    }
                }
                
                return GetControlPointCheckItem(organization.OrganizationList, CheckItemID);
            }

            return new ControlPointCheckItemModel()
            {
                UniqueID= string.Empty,
                ID = CheckItemID,
                Description = string.Empty,
                IsExist = false
            };
        }

        private static RouteControlPointModel GetRouteControlPoint(List<Models.EquipmentMaintenance.Import.OrganizationModel> OrganizationMdodelList, string ControlPointID)
        {
            foreach (var organization in OrganizationMdodelList)
            {
                var controlPointModel = organization.ControlPointList.FirstOrDefault(x => x.ID == ControlPointID);

                if (controlPointModel != null)
                {
                    return new RouteControlPointModel()
                    {
                        UniqueID = controlPointModel.UniqueID,
                        ID = controlPointModel.ID,
                        Description = controlPointModel.Description,
                        IsExist = true
                    };
                }

                if (!organization.IsNewOrganization)
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var controlPoint = db.CONTROLPOINT.Where(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID && x.ID == ControlPointID).FirstOrDefault();

                        if (controlPoint != null)
                        {
                            return new RouteControlPointModel()
                            {
                                UniqueID = controlPoint.UNIQUEID,
                                ID = controlPoint.ID,
                                Description = controlPoint.DESCRIPTION,
                                IsExist = true
                            };
                        }
                    }
                }

                return GetRouteControlPoint(organization.OrganizationList, ControlPointID);
            }

            return new RouteControlPointModel()
            {
                UniqueID = string.Empty,
                ID = ControlPointID,
                Description = string.Empty,
                IsExist = false
            };
        }

        private static RouteControlPointCheckItemModel GetRouteControlPointCheckItem(List<Models.EquipmentMaintenance.Import.OrganizationModel> OrganizationMdodelList, string ControlPointID, string CheckItemID)
        {
            foreach (var organization in OrganizationMdodelList)
            {
                var controlPointModel = organization.ControlPointList.FirstOrDefault(x => x.ID == ControlPointID);

                if (controlPointModel != null)
                {
                    var checkItemModel = controlPointModel.CheckItemList.FirstOrDefault(x => x.ID == CheckItemID);

                    if (checkItemModel != null)
                    {
                        return new RouteControlPointCheckItemModel()
                        {
                            UniqueID = checkItemModel.UniqueID,
                            ID = checkItemModel.ID,
                            Description = checkItemModel.Description,
                            IsExist = true
                        };
                    }
                }

                if (!organization.IsNewOrganization)
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var checkItem = (from x in db.CONTROLPOINTCHECKITEM
                                         join p in db.CONTROLPOINT
                                         on x.CONTROLPOINTUNIQUEID equals p.UNIQUEID
                                         join c in db.CHECKITEM
                                         on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                         where p.ORGANIZATIONUNIQUEID == organization.UniqueID && p.ID == ControlPointID && c.ID == CheckItemID
                                         select c).FirstOrDefault();

                        if (checkItem != null)
                        {
                            return new RouteControlPointCheckItemModel()
                            {
                                UniqueID = checkItem.UNIQUEID,
                                ID = checkItem.ID,
                                Description = checkItem.DESCRIPTION,
                                IsExist = true
                            };
                        }
                    }
                }
                
                return GetRouteControlPointCheckItem(organization.OrganizationList, ControlPointID, CheckItemID);
            }

            return new RouteControlPointCheckItemModel()
            {
                UniqueID = string.Empty,
                ID = CheckItemID,
                Description = string.Empty,
                IsExist = false
            };
        }

        private static RouteEquipmentModel GetRouteEquipment(List<Models.EquipmentMaintenance.Import.OrganizationModel> OrganizationMdodelList, string EquipmentID, string PartDescription)
        {
            foreach (var organization in OrganizationMdodelList)
            {
                var equipmentModel = organization.EquipmentList.FirstOrDefault(x => x.ID == EquipmentID && x.PartDescription == PartDescription);

                if (equipmentModel != null)
                {
                    return new RouteEquipmentModel()
                    {
                        UniqueID = equipmentModel.UniqueID,
                        ID = equipmentModel.ID,
                        Name = equipmentModel.Name,
                        PartUniqueID = equipmentModel.PartUniqueID,
                        PartDescription = equipmentModel.PartDescription,
                        IsExist = true
                    };
                }

                if (!organization.IsNewOrganization)
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var equipment = db.EQUIPMENT.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID && x.ID == EquipmentID);

                        if (equipment != null)
                        {
                            if (string.IsNullOrEmpty(PartDescription))
                            {
                                return new RouteEquipmentModel()
                                {
                                    UniqueID = equipment.UNIQUEID,
                                    ID = equipment.ID,
                                    Name = equipment.NAME,
                                    PartUniqueID = "*",
                                    PartDescription = string.Empty,
                                    IsExist = true
                                };
                            }
                            else
                            {
                                var equipmentPart = db.EQUIPMENTPART.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.DESCRIPTION == PartDescription);

                                if (equipmentPart != null)
                                {
                                    return new RouteEquipmentModel()
                                    {
                                        UniqueID = equipment.UNIQUEID,
                                        ID = equipment.ID,
                                        Name = equipment.NAME,
                                        PartUniqueID = equipmentPart.UNIQUEID,
                                        PartDescription = equipmentPart.DESCRIPTION,
                                        IsExist = true
                                    };
                                }
                            }
                        }
                    }
                }

                return GetRouteEquipment(organization.OrganizationList, EquipmentID, PartDescription);
            }

            return new RouteEquipmentModel()
            {
                UniqueID = string.Empty,
                ID = EquipmentID,
                Name = string.Empty,
                PartUniqueID = string.Empty,
                PartDescription = PartDescription,
                IsExist = false
            };
        }

        private static RouteEquipmentCheckItemModel GetRouteEquipmentCheckItem(List<Models.EquipmentMaintenance.Import.OrganizationModel> OrganizationMdodelList, string EquipmentID, string PartDescription, string CheckItemID)
        {
            foreach (var organization in OrganizationMdodelList)
            {
                var equipmentModel = organization.EquipmentList.FirstOrDefault(x => x.ID == EquipmentID && x.PartDescription == PartDescription);

                if (equipmentModel != null)
                {
                    var checkItemModel = equipmentModel.CheckItemList.FirstOrDefault(x => x.ID == CheckItemID);

                    if (checkItemModel != null)
                    {
                        return new RouteEquipmentCheckItemModel()
                        {
                            UniqueID = checkItemModel.UniqueID,
                            ID = checkItemModel.ID,
                            Description = checkItemModel.Description,
                            IsExist = true
                        };
                    }
                }

                if (!organization.IsNewOrganization)
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        if (string.IsNullOrEmpty(PartDescription))
                        {
                            var checkItem = (from x in db.EQUIPMENTCHECKITEM
                                             join e in db.EQUIPMENT
                                             on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                             join c in db.CHECKITEM
                                             on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                             where e.ORGANIZATIONUNIQUEID == organization.UniqueID && e.ID == EquipmentID && c.ID == CheckItemID
                                             select c).FirstOrDefault();

                            if (checkItem != null)
                            {
                                return new RouteEquipmentCheckItemModel()
                                {
                                    UniqueID = checkItem.UNIQUEID,
                                    ID = checkItem.ID,
                                    Description = checkItem.DESCRIPTION,
                                    IsExist = true
                                };
                            }
                        }
                        else
                        {
                            var checkItem = (from x in db.EQUIPMENTCHECKITEM
                                             join e in db.EQUIPMENT
                                             on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                             join p in db.EQUIPMENTPART
                                             on x.PARTUNIQUEID equals p.UNIQUEID
                                             join c in db.CHECKITEM
                                             on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                             where e.ORGANIZATIONUNIQUEID == organization.UniqueID && e.ID == EquipmentID && p.DESCRIPTION == PartDescription && c.ID == CheckItemID
                                             select c).FirstOrDefault();

                            if (checkItem != null)
                            {
                                return new RouteEquipmentCheckItemModel()
                                {
                                    UniqueID = checkItem.UNIQUEID,
                                    ID = checkItem.ID,
                                    Description = checkItem.DESCRIPTION,
                                    IsExist = true
                                };
                            }

                        }
                    }   
                }

                return GetRouteEquipmentCheckItem(organization.OrganizationList, EquipmentID, PartDescription, CheckItemID);
            }

            return new RouteEquipmentCheckItemModel()
            {
                UniqueID = string.Empty,
                ID = CheckItemID,
                Description = string.Empty,
                IsExist = false
            };
        }

        public static RequestResult GetOrganizationTreeItem(ImportModel Model, string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.IsNew, string.Empty },
                    { Define.EnumTreeAttribute.IsError, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty }
                };

                var organizationList = new List<Models.EquipmentMaintenance.Import.OrganizationModel>();

                if (OrganizationUniqueID == "*")
                {
                    organizationList = Model.OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && x.HaveNewOrganization).OrderBy(x => x.ID).ToList();
                }
                else
                {
                    organizationList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).OrganizationList.Where(x => x.HaveNewOrganization).OrderBy(x => x.ID).ToList();
                }

                foreach (var organization in organizationList)
                {
                    var treeItem = new TreeItem() { Title = organization.Display };

                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Define.EnumTreeAttribute.IsNew] = organization.IsNewOrganization ? "Y" : "N";
                    attributes[Define.EnumTreeAttribute.IsError] = organization.IsError ? "Y" : "N";
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    if (organization.OrganizationList.Any(x => x.HaveNewOrganization))
                    {
                        treeItem.State = "closed";
                    }

                    treeItemList.Add(treeItem);
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetUserTreeItem(ImportModel Model, string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.IsNew, string.Empty },
                    { Define.EnumTreeAttribute.IsError, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty }
                };

                if (OrganizationUniqueID != "*")
                {
                    var userList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).UserList.OrderBy(x => x.ID).ToList();

                    foreach (var user in userList)
                    {
                        var treeItem = new TreeItem() { Title = user.Display };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.User.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", user.ID, user.Name);
                        attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                        attributes[Define.EnumTreeAttribute.IsError] = user.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }
                }

                var organizationList = new List<Models.EquipmentMaintenance.Import.OrganizationModel>();

                if (OrganizationUniqueID == "*")
                {
                    organizationList = Model.OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && x.HaveNewUser).OrderBy(x => x.ID).ToList();
                }
                else
                {
                    organizationList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).OrganizationList.Where(x => x.HaveNewUser).OrderBy(x => x.ID).ToList();
                }

                foreach (var organization in organizationList)
                {
                    var treeItem = new TreeItem() { Title = organization.Description };

                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Define.EnumTreeAttribute.IsNew] = organization.IsNewOrganization ? "Y" : "N";
                    attributes[Define.EnumTreeAttribute.IsError] = organization.IsError ? "Y" : "N";
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    treeItem.State = "closed";

                    treeItemList.Add(treeItem);
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetRouteTreeItem(ImportModel Model, string OrganizationUniqueID, string RouteUniqueID, string ControlPointUniqueID, string EquipmentUniqueID, string PartUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.IsNew, string.Empty },
                    { Define.EnumTreeAttribute.IsError, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.RouteUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PartUniqueID, string.Empty }
                };

                if (!string.IsNullOrEmpty(EquipmentUniqueID))
                {
                    var checkItemList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).RouteList.First(x => x.UniqueID == RouteUniqueID).ControlPointList.First(x => x.UniqueID == ControlPointUniqueID).EquipmentList.First(x => x.UniqueID == EquipmentUniqueID && x.PartUniqueID == PartUniqueID).CheckItemList;

                    foreach (var checkItem in checkItemList)
                    {
                        var treeItem = new TreeItem() { Title = checkItem.Display };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                        attributes[Define.EnumTreeAttribute.IsError] = checkItem.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.RouteUniqueID] = RouteUniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = ControlPointUniqueID;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = EquipmentUniqueID;
                        attributes[Define.EnumTreeAttribute.PartUniqueID] = PartUniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }
                }
                else if (!string.IsNullOrEmpty(ControlPointUniqueID))
                {
                    var controlPoint = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).RouteList.First(x => x.UniqueID == RouteUniqueID).ControlPointList.First(x => x.UniqueID == ControlPointUniqueID);

                    foreach (var checkItem in controlPoint.CheckItemList)
                    {
                        var treeItem = new TreeItem() { Title = checkItem.Display };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                        attributes[Define.EnumTreeAttribute.IsError] = checkItem.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.RouteUniqueID] = RouteUniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = ControlPointUniqueID;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                        attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }

                    foreach (var equipment in controlPoint.EquipmentList)
                    {
                        var treeItem = new TreeItem() { Title = equipment.Display };

                        if (string.IsNullOrEmpty(equipment.PartDescription))
                        {
                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Equipment.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", equipment.ID, equipment.Name);
                        }
                        else
                        {
                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentPart.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}-{2}", equipment.ID, equipment.Name, equipment.PartDescription);
                        }

                        attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                        attributes[Define.EnumTreeAttribute.IsError] = equipment.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.RouteUniqueID] = RouteUniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = ControlPointUniqueID;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.UniqueID;
                        attributes[Define.EnumTreeAttribute.PartUniqueID] = equipment.PartUniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (equipment.CheckItemList != null && equipment.CheckItemList.Count > 0)
                        {
                            treeItem.State = "closed";
                        }

                        treeItemList.Add(treeItem);
                    }
                }
                else if (!string.IsNullOrEmpty(RouteUniqueID))
                {
                    var controlPointList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).RouteList.First(x => x.UniqueID == RouteUniqueID).ControlPointList;

                    foreach (var controlPoint in controlPointList)
                    {
                        var treeItem = new TreeItem() { Title = controlPoint.Display };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ControlPoint.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", controlPoint.ID, controlPoint.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                        attributes[Define.EnumTreeAttribute.IsError] = controlPoint.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.RouteUniqueID] = RouteUniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = controlPoint.UniqueID;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                        attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if ((controlPoint.CheckItemList != null && controlPoint.CheckItemList.Count > 0) || (controlPoint.EquipmentList != null && controlPoint.EquipmentList.Count > 0))
                        {
                            treeItem.State = "closed";
                        }

                        treeItemList.Add(treeItem);
                    }
                }
                else
                {
                    if (OrganizationUniqueID != "*")
                    {
                        var routeList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).RouteList.OrderBy(x => x.ID).ToList();

                        foreach (var route in routeList)
                        {
                            var treeItem = new TreeItem() { Title = route.Display };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Route.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", route.ID, route.Name);
                            attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                            attributes[Define.EnumTreeAttribute.IsError] = route.IsError ? "Y" : "N";
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = route.UniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (route.ControlPointList != null && route.ControlPointList.Count > 0)
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }

                    var organizationList = new List<Models.EquipmentMaintenance.Import.OrganizationModel>();

                    if (OrganizationUniqueID == "*")
                    {
                        organizationList = Model.OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && x.HaveNewRoute).OrderBy(x => x.ID).ToList();
                    }
                    else
                    {
                        organizationList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).OrganizationList.Where(x => x.HaveNewRoute).OrderBy(x => x.ID).ToList();
                    }

                    foreach (var organization in organizationList)
                    {
                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = organization.IsNewOrganization ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.IsError] = organization.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                        attributes[Define.EnumTreeAttribute.RouteUniqueID] = string.Empty;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = string.Empty;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                        attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItem.State = "closed";

                        treeItemList.Add(treeItem);
                    }
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetControlPointTreeItem(ImportModel Model, string OrganizationUniqueID, string ControlPointUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.IsNew, string.Empty },
                    { Define.EnumTreeAttribute.IsError, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty }
                };

                if (!string.IsNullOrEmpty(ControlPointUniqueID))
                {
                    var checkItemList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).ControlPointList.First(x => x.UniqueID == ControlPointUniqueID).CheckItemList;

                    foreach (var checkItem in checkItemList)
                    {
                        var treeItem = new TreeItem() { Title = checkItem.Display };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                        attributes[Define.EnumTreeAttribute.IsError] = checkItem.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = ControlPointUniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }
                }
                else
                {
                    if (OrganizationUniqueID != "*")
                    {
                        var controlPointList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).ControlPointList.OrderBy(x => x.ID).ToList();

                        foreach (var controlPoint in controlPointList)
                        {
                            var treeItem = new TreeItem() { Title = controlPoint.Display };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ControlPoint.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", controlPoint.ID, controlPoint.Description);
                            attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                            attributes[Define.EnumTreeAttribute.IsError] = controlPoint.IsError ? "Y" : "N";
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = controlPoint.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (controlPoint.CheckItemList != null && controlPoint.CheckItemList.Count > 0)
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }

                    var organizationList = new List<Models.EquipmentMaintenance.Import.OrganizationModel>();

                    if (OrganizationUniqueID == "*")
                    {
                        organizationList = Model.OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && x.HaveNewControlPoint).OrderBy(x => x.ID).ToList();
                    }
                    else
                    {
                        organizationList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).OrganizationList.Where(x => x.HaveNewControlPoint).OrderBy(x => x.ID).ToList();
                    }

                    foreach (var organization in organizationList)
                    {
                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = organization.IsNewOrganization ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.IsError] = organization.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItem.State = "closed";

                        treeItemList.Add(treeItem);
                    }
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetEquipmentTreeItem(ImportModel Model, string OrganizationUniqueID, string EquipmentUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.IsNew, string.Empty },
                    { Define.EnumTreeAttribute.IsError, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty }
                };

                if (!string.IsNullOrEmpty(EquipmentUniqueID))
                {
                    var checkItemList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).EquipmentList.First(x => x.UniqueID == EquipmentUniqueID).CheckItemList;

                    foreach (var checkItem in checkItemList)
                    {
                        var treeItem = new TreeItem() { Title = checkItem.Display };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                        attributes[Define.EnumTreeAttribute.IsError] = checkItem.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = EquipmentUniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }
                }
                else
                {
                    if (OrganizationUniqueID != "*")
                    {
                        var equipmentList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).EquipmentList.OrderBy(x => x.ID).ToList();

                        foreach (var equipment in equipmentList)
                        {
                            var treeItem = new TreeItem() { Title = equipment.Display };

                            if (string.IsNullOrEmpty(equipment.PartDescription))
                            {
                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Equipment.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", equipment.ID, equipment.Name);
                            }
                            else
                            {
                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentPart.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}-{2}", equipment.ID, equipment.Name, equipment.PartDescription);
                            }

                            attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                            attributes[Define.EnumTreeAttribute.IsError] = equipment.IsError ? "Y" : "N";
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (equipment.CheckItemList != null && equipment.CheckItemList.Count > 0)
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }

                    var organizationList = new List<Models.EquipmentMaintenance.Import.OrganizationModel>();

                    if (OrganizationUniqueID == "*")
                    {
                        organizationList = Model.OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && x.HaveNewEquipment).OrderBy(x => x.ID).ToList();
                    }
                    else
                    {
                        organizationList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).OrganizationList.Where(x => x.HaveNewEquipment).OrderBy(x => x.ID).ToList();
                    }

                    foreach (var organization in organizationList)
                    {
                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = organization.IsNewOrganization ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.IsError] = organization.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItem.State = "closed";

                        treeItemList.Add(treeItem);
                    }
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetCheckItemTreeItem(ImportModel Model, string OrganizationUniqueID, string CheckType, string CheckItemUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.IsNew, string.Empty },
                    { Define.EnumTreeAttribute.IsError, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.CheckType, string.Empty },
                    { Define.EnumTreeAttribute.CheckItemUniqueID, string.Empty }
                };

                if (!string.IsNullOrEmpty(CheckItemUniqueID))
                {
                    var abnormalReasonList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).CheckItemList.First(x => x.UniqueID == CheckItemUniqueID).AbnormalReasonList;

                    foreach (var abnormalReason in abnormalReasonList)
                    {
                        var treeItem = new TreeItem() { Title = abnormalReason.Display };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.AbnormalReason.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", abnormalReason.ID, abnormalReason.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                        attributes[Define.EnumTreeAttribute.IsError] = abnormalReason.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.CheckType] = CheckType;
                        attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = CheckItemUniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }
                }
                else if (!string.IsNullOrEmpty(CheckType))
                {
                    var checkItemList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).CheckItemList.Where(x => x.CheckType == CheckType).OrderBy(x => x.ID).ToList();

                    foreach (var checkItem in checkItemList)
                    {
                        var treeItem = new TreeItem() { Title = checkItem.Display };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                        attributes[Define.EnumTreeAttribute.IsError] = checkItem.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.CheckType] = checkItem.CheckType;
                        attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = checkItem.UniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (checkItem.AbnormalReasonList != null && checkItem.AbnormalReasonList.Count > 0)
                        {
                            treeItem.State = "closed";
                        }

                        treeItemList.Add(treeItem);
                    }
                }
                else
                {
                    if (OrganizationUniqueID != "*")
                    {
                        var checkTypeList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).CheckItemList.Select(x => x.CheckType).Distinct().OrderBy(x => x).ToList();

                        foreach (var checkType in checkTypeList)
                        {
                            var treeItem = new TreeItem() { Title = checkType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = checkType;
                            attributes[Define.EnumTreeAttribute.IsNew] = "N";
                            attributes[Define.EnumTreeAttribute.IsError] = "N";
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.CheckType] = checkType;
                            attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItem.State = "closed";

                            treeItemList.Add(treeItem);
                        }
                    }

                    var organizationList = new List<Models.EquipmentMaintenance.Import.OrganizationModel>();

                    if (OrganizationUniqueID == "*")
                    {
                        organizationList = Model.OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && x.HaveNewCheckItem).OrderBy(x => x.ID).ToList();
                    }
                    else
                    {
                        organizationList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).OrganizationList.Where(x => x.HaveNewCheckItem).OrderBy(x => x.ID).ToList();
                    }

                    foreach (var organization in organizationList)
                    {
                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = organization.IsNewOrganization ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.IsError] = organization.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                        attributes[Define.EnumTreeAttribute.CheckType] = string.Empty;
                        attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItem.State = "closed";

                        treeItemList.Add(treeItem);
                    }
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetAbnormalReasonTreeItem(ImportModel Model, string OrganizationUniqueID, string AbnormalType, string AbnormalReasonUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.IsNew, string.Empty },
                    { Define.EnumTreeAttribute.IsError, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.AbnormalType, string.Empty },
                    { Define.EnumTreeAttribute.AbnormalReasonUniqueID, string.Empty }
                };

                if (!string.IsNullOrEmpty(AbnormalReasonUniqueID))
                {
                    var handlingMethodList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).AbnormalReasonList.First(x => x.UniqueID == AbnormalReasonUniqueID).HandlingMethodList;

                    foreach (var handlingMethod in handlingMethodList)
                    {
                        var treeItem = new TreeItem() { Title = handlingMethod.Display };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.HandlingMethod.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", handlingMethod.ID, handlingMethod.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                        attributes[Define.EnumTreeAttribute.IsError] = handlingMethod.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.AbnormalType] = AbnormalType;
                        attributes[Define.EnumTreeAttribute.AbnormalReasonUniqueID] = AbnormalReasonUniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }
                }
                else if (!string.IsNullOrEmpty(AbnormalType))
                {
                    var abnormalReasonList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).AbnormalReasonList.Where(x => x.AbnormalType == AbnormalType).OrderBy(x => x.ID).ToList();

                    foreach (var abnormalReason in abnormalReasonList)
                    {
                        var treeItem = new TreeItem() { Title = abnormalReason.Display };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.AbnormalReason.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", abnormalReason.ID, abnormalReason.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                        attributes[Define.EnumTreeAttribute.IsError] = abnormalReason.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.AbnormalType] = abnormalReason.AbnormalType;
                        attributes[Define.EnumTreeAttribute.AbnormalReasonUniqueID] = abnormalReason.UniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (abnormalReason.HandlingMethodList != null && abnormalReason.HandlingMethodList.Count > 0)
                        {
                            treeItem.State = "closed";
                        }

                        treeItemList.Add(treeItem);
                    }
                }
                else
                {
                    if (OrganizationUniqueID != "*")
                    {
                        var abnormalReasonTypeList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).AbnormalReasonList.Select(x => x.AbnormalType).Distinct().OrderBy(x => x).ToList();

                        foreach (var abnormalReasonType in abnormalReasonTypeList)
                        {
                            var treeItem = new TreeItem() { Title = abnormalReasonType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.AbnormalType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = abnormalReasonType;
                            attributes[Define.EnumTreeAttribute.IsNew] = "N";
                            attributes[Define.EnumTreeAttribute.IsError] = "N";
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.AbnormalType] = abnormalReasonType;
                            attributes[Define.EnumTreeAttribute.AbnormalReasonUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItem.State = "closed";

                            treeItemList.Add(treeItem);
                        }
                    }

                    var organizationList = new List<Models.EquipmentMaintenance.Import.OrganizationModel>();

                    if (OrganizationUniqueID == "*")
                    {
                        organizationList = Model.OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && x.HaveNewAbnormalReason).OrderBy(x => x.ID).ToList();
                    }
                    else
                    {
                        organizationList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).OrganizationList.Where(x => x.HaveNewAbnormalReason).OrderBy(x => x.ID).ToList();
                    }

                    foreach (var organization in organizationList)
                    {
                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = organization.IsNewOrganization ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.IsError] = organization.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                        attributes[Define.EnumTreeAttribute.AbnormalType] = string.Empty;
                        attributes[Define.EnumTreeAttribute.AbnormalReasonUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItem.State = "closed";

                        treeItemList.Add(treeItem);
                    }
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetHandlingMethodTreeItem(ImportModel Model, string OrganizationUniqueID, string HandlingMethodType)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.IsNew, string.Empty },
                    { Define.EnumTreeAttribute.IsError, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.HandlingMethodType, string.Empty }
                };

                if (string.IsNullOrEmpty(HandlingMethodType))
                {
                    if (OrganizationUniqueID != "*")
                    {
                        var handlingMethodTypeList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).HandlingMethodList.Select(x => x.HandlingMethodType).Distinct().OrderBy(x => x).ToList();

                        foreach (var handlingMethodType in handlingMethodTypeList)
                        {
                            var treeItem = new TreeItem() { Title = handlingMethodType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.HandlingMethodType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = handlingMethodType;
                            attributes[Define.EnumTreeAttribute.IsNew] = "N";
                            attributes[Define.EnumTreeAttribute.IsError] = "N";
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.HandlingMethodType] = handlingMethodType;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItem.State = "closed";

                            treeItemList.Add(treeItem);
                        }
                    }

                    var organizationList = new List<Models.EquipmentMaintenance.Import.OrganizationModel>();

                    if (OrganizationUniqueID == "*")
                    {
                        organizationList = Model.OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && x.HaveNewHandlingMethod).OrderBy(x => x.ID).ToList();
                    }
                    else
                    {
                        organizationList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).OrganizationList.Where(x => x.HaveNewHandlingMethod).OrderBy(x => x.ID).ToList();
                    }

                    foreach (var organization in organizationList)
                    {
                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = organization.IsNewOrganization ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.IsError] = organization.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                        attributes[Define.EnumTreeAttribute.HandlingMethodType] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItem.State = "closed";

                        treeItemList.Add(treeItem);
                    }
                }
                else
                {
                    var handlingMethodList = GetOrganizationModelByUniqueID(Model.OrganizationList, OrganizationUniqueID).HandlingMethodList.Where(x => x.HandlingMethodType == HandlingMethodType).OrderBy(x => x.ID).ToList();

                    foreach (var handlingMethod in handlingMethodList)
                    {
                        var treeItem = new TreeItem() { Title = handlingMethod.Display };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.HandlingMethod.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", handlingMethod.ID, handlingMethod.Description);
                        attributes[Define.EnumTreeAttribute.IsNew] = "Y";
                        attributes[Define.EnumTreeAttribute.IsError] = handlingMethod.IsError ? "Y" : "N";
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.HandlingMethodType] = handlingMethod.HandlingMethodType;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }
    }
}
