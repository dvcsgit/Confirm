using DbEntity.ASE;
using Models.ASE.Inventory;
using Models.Authenticated;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.ASE
{
    public class InventoryDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.MATERIAL.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.NAME.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(Parameters.OrganizationUniqueID),
                        FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(Parameters.OrganizationUniqueID),
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UNIQUEID,
                            ID = x.ID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
                            EquipmentType = x.EQUIPMENTTYPE,
                            Name = x.NAME,
                            Quantity = x.QUANTITY.Value
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.EquipmentType).ThenBy(x => x.ID).ToList()
                    });
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

        public static RequestResult Export(GridViewModel Model, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ExcelHelper helper = new ExcelHelper(string.Format("盤點確認單_{0}({1})",Model.OrganizationDescription, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), ExcelVersion))
                {
                    helper.CreateSheet<GridItem>(Model.ItemList);

                    result.ReturnData(helper.Export());
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var material = db.MATERIAL.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = material.UNIQUEID,
                        ID = material.ID,
                        Name = material.NAME,
                        Quantity = material.QUANTITY.Value
                    });
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

        public static RequestResult Edit(EditFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var material = db.MATERIAL.First(x => x.UNIQUEID == Model.UniqueID);

                    if (Model.FormInput.Quantity <= 0)
                    {
                        result.ReturnFailedMessage("異動數量不正確");
                    }
                    else if (Model.FormInput.Type == "Out" && Model.FormInput.Quantity > material.QUANTITY.Value)
                    {
                        result.ReturnFailedMessage("庫存數量不足");
                    }
                    else
                    {
                        if (Model.FormInput.Type == "In")
                        {
                            material.QUANTITY = material.QUANTITY.Value + Model.FormInput.Quantity;
                        }

                        if (Model.FormInput.Type == "Out")
                        {
                            material.QUANTITY = material.QUANTITY.Value - Model.FormInput.Quantity;
                        }

                        var alterTime = DateTime.Now;

                        db.INVENTORY.Add(new INVENTORY
                        {
                            MATERIALUNIQUEID = material.UNIQUEID,
                            TYPE = Model.FormInput.Type,
                            QUANTITY = Model.FormInput.Quantity,
                            USERID = Account.ID,
                            ALTERDATE = alterTime
                        });

                        db.SaveChanges();

                        if (Model.FormInput.Type == "In")
                        {
                            result.ReturnSuccessMessage("入庫成功");
                        }

                        if (Model.FormInput.Type == "Out")
                        {
                            result.ReturnSuccessMessage("領料成功");
                        }

                        if (Config.HaveMailSetting)
                        {
                            SendInventoryChangeMail(Model,alterTime,  Account);
                        }
                    }
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

        public static void SendInventoryChangeMail(EditFormModel Model,DateTime AlterTime, Account Account)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var material = db.MATERIAL.First(x => x.UNIQUEID == Model.UniqueID);
                    var organization = db.ORGANIZATION.First(x => x.UNIQUEID == material.ORGANIZATIONUNIQUEID);

                    var mailAddressList = new List<MailAddress>();

                    var managerList = db.INVENTORYMANAGER.Where(x => x.ORGANIZATIONUNIQUEID == material.ORGANIZATIONUNIQUEID).ToList();

                    foreach (var manager in managerList)
                    {
                        var account = db.ACCOUNT.FirstOrDefault(x => x.ID == manager.USERID);

                        if (account != null && !string.IsNullOrEmpty(account.EMAIL))
                        {
                            mailAddressList.Add(new MailAddress(account.EMAIL, account.NAME));
                        }
                    }

                    if (mailAddressList.Count > 0)
                    {
                        var type = "";

                        if (Model.FormInput.Type == "In")
                        {
                            type = "入庫";
                        }
                        else if (Model.FormInput.Type == "Out")
                        {
                            type = "領料";
                        }

                        var subject = string.Format("[物料出入庫異動通知][{0}]{1}/{2}({3}數量：{4})", organization.DESCRIPTION, material.ID, material.NAME, type, Model.FormInput.Quantity);

                        var sb = new StringBuilder();

                        sb.Append("<p>");
                        sb.Append(string.Format("部門：{0}", organization.DESCRIPTION));
                        sb.Append("</p>");

                        sb.Append("<p>");
                        sb.Append(string.Format("材料：{0}/{1}", material.ID, material.NAME));
                        sb.Append("</p>");

                        sb.Append("<p>");
                        sb.Append(string.Format("異動類別：{0}", type));
                        sb.Append("</p>");

                        sb.Append("<p>");
                        sb.Append(string.Format("異動數量：{0}", Model.FormInput.Quantity));
                        sb.Append("</p>");

                        sb.Append("<p>");
                        sb.Append(string.Format("異動時間：{0}", DateTimeHelper.DateTime2DateTimeStringWithSeperator(AlterTime)));
                        sb.Append("</p>");

                        sb.Append("<p>");
                        sb.Append(string.Format("異動人員：{0}/{1}", Account.ID, Account.Name));
                        sb.Append("</p>");

                        sb.Append("<p>");
                        sb.Append(string.Format("庫存量：{0}", material.QUANTITY));
                        sb.Append("</p>");

                        MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var material = db.MATERIAL.First(x => x.UNIQUEID == UniqueID);

                    var model = new DetailViewModel()
                    {
                         FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(material.ORGANIZATIONUNIQUEID),
                        UniqueID = material.UNIQUEID,
                        ID = material.ID,
                        Name = material.NAME,
                        Quantity = material.QUANTITY.Value,
                         SpecList = (from x in db.MATERIALSPECVALUE
                                     join s in db.MATERIALSPEC
                                     on x.SPECUNIQUEID equals s.UNIQUEID
                                     where x.MATERIALUNIQUEID == material.UNIQUEID
                                     select new SpecModel
                                     {
                                         UniqueID = s.UNIQUEID,
                                         Description = s.DESCRIPTION,
                                         OptionUniqueID = x.SPECOPTIONUNIQUEID,
                                         Value = x.VALUE,
                                         Seq = x.SEQ.Value,
                                         OptionList = db.MATERIALSPECOPTION.Where(o => o.SPECUNIQUEID == s.UNIQUEID).Select(o => new MaterialSpecOption
                                         {
                                             SpecUniqueID = o.SPECUNIQUEID,
                                             Seq = o.SEQ.Value,
                                             Description = o.DESCRIPTION,
                                             UniqueID = o.UNIQUEID
                                         }).OrderBy(o => o.Seq).ToList()
                                     }).OrderBy(x => x.Seq).ToList()
                    };

                    var inventoryList = db.INVENTORY.Where(x => x.MATERIALUNIQUEID == material.UNIQUEID).OrderByDescending(x => x.ALTERDATE).ToList();

                    foreach (var inventory in inventoryList)
                    {
                        var user = db.ACCOUNT.FirstOrDefault(x => x.ID == inventory.USERID);

                        model.InventoryList.Add(new InventoryModel()
                        {
                            AlterTime = inventory.ALTERDATE,
                            Quantity = inventory.QUANTITY,
                            Type = inventory.TYPE,
                            UserID = inventory.USERID,
                            UserName = user != null ? user.NAME : string.Empty
                        });
                    }

                    result.ReturnData(model);
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

        public static RequestResult Upload(UploadFormModel Model)
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

                        IWorkbook workBook = null;

                        if (extension == Define.ExcelExtension_2003)
                        {
                            workBook = new HSSFWorkbook(new MemoryStream(bytes));
                        }
                        else if (extension == Define.ExcelExtension_2007)
                        {
                            workBook = new XSSFWorkbook(new MemoryStream(bytes));
                        }

                        var model = new UploadResultModel();

                        var sheet = workBook.GetSheetAt(0);

                        using (ASEDbEntities db = new ASEDbEntities())
                        {
                            for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                            {
                                var item = new UploadItem()
                                {
                                    RowIndex = rowIndex,
                                    OK = false
                                };

                                try
                                {
                                    var row = sheet.GetRow(rowIndex);

                                    var uniqueID = row.GetCell(0).StringCellValue;

                                    var material = (from m in db.MATERIAL
                                                    join o in db.ORGANIZATION
                                                    on m.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                                    where m.UNIQUEID == uniqueID
                                                    select new
                                                    {
                                                        UniqueID = m.UNIQUEID,
                                                        OrganizationDescription = o.DESCRIPTION,
                                                        EquipmentType = m.EQUIPMENTTYPE,
                                                        m.ID,
                                                        Name = m.NAME,
                                                       Quantity= m.QUANTITY.Value
                                                    }).FirstOrDefault();

                                    if (material != null)
                                    {
                                        item.UniqueID = material.UniqueID;
                                        item.EquipmentType = material.EquipmentType;
                                        item.ID = material.ID;
                                        item.Name = material.Name;
                                        item.OrganizationDescription = material.OrganizationDescription;
                                        item.Quantity = material.Quantity;

                                        var cell = row.GetCell(6);

                                        try
                                        {
                                            item.Q1 = Convert.ToInt32(cell.NumericCellValue);
                                            item.OK = true;
                                        }
                                        catch
                                        {
                                            try
                                            {
                                                item.Q1 = Convert.ToInt32(cell.StringCellValue);
                                                item.OK = true;
                                            }
                                            catch {
                                                item.ErrorMessage = "無法取得盤點數量";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        item.ErrorMessage = string.Format("{0} {1}", Resources.Resource.MaterialID, Resources.Resource.NotExist);
                                    }
                                }
                                catch(Exception ex)
                                {
                                    var err = new Error(MethodBase.GetCurrentMethod(), ex);

                                    item.ErrorMessage = err.ErrorMessage;
                                }

                                model.ItemList.Add(item);
                            }
                        }

                        result.ReturnData(model);
                    }
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UploadFileRequired);
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

        public static RequestResult Import(UploadResultModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var time = DateTime.Now;

                    foreach (var item in Model.ItemList)
                    {
                        if (item.OK)
                        {
                            db.INVENTORY.Add(new INVENTORY()
                            {
                                ALTERDATE = time,
                                MATERIALUNIQUEID = item.UniqueID,
                                QUANTITY = item.Q1,
                                TYPE = "C",
                                USERID = Account.ID
                            });

                            var material = db.MATERIAL.First(x => x.UNIQUEID == item.UniqueID);

                            material.QUANTITY = item.Q1;
                        }
                    }

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage("匯入成功");
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
