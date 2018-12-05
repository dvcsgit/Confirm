using DbEntity.MSSQL.PipelinePatrol;
using Ionic.Zip;
using Models.PipelinePatrol.DataSync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;
using Models.PipelinePatrol.Shared;

namespace DataSync.PipelinePatrol
{
    public class ChatHelper
    {
        public static RequestResult NewDialog(string UniqueID, DialogFormInput Model, string TempPhotoFolder)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var extension = string.Empty;

                    if (!string.IsNullOrEmpty(TempPhotoFolder))
                    {
                        var zipFile = Path.Combine(TempPhotoFolder, string.Format("{0}.zip", UniqueID));

                        var photoFile = string.Empty;

                        using (var zip = ZipFile.Read(zipFile))
                        {
                            if (zip.Count > 0)
                            {
                                photoFile = zip[0].FileName;

                                zip[0].Extract(TempPhotoFolder);
                            }
                        }

                        if (!string.IsNullOrEmpty(photoFile))
                        {
                            var fileInfo = new FileInfo(Path.Combine(TempPhotoFolder, photoFile));

                            extension = fileInfo.Extension.Replace(".", "");

                            System.IO.File.Copy(fileInfo.FullName, Path.Combine(Config.ChatPhotoFolderPath, string.Format("{0}.{1}", UniqueID, extension)));
                        }
                    }

                    var subject = string.Empty;

                    var description = string.Empty;

                    if (!string.IsNullOrEmpty(Model.PipelineAbnormalUniqueID))
                    {
                        var query = db.Dialog.FirstOrDefault(x => x.PipelineAbnormalUniqueID == Model.PipelineAbnormalUniqueID);

                        if (query != null)
                        {
                            subject = string.Empty;
                            description = string.Empty;

                            result.Message = "該討論主題已存在";
                            result.IsSuccess = false;
                        }
                        else
                        {
                            var pipelineAbnormal = db.PipelineAbnormal.First(x => x.UniqueID == Model.PipelineAbnormalUniqueID);

                            subject = string.Format("【{0}】#{1}", Resources.Resource.PipelineAbnormal, pipelineAbnormal.VHNO);

                            if (!string.IsNullOrEmpty(pipelineAbnormal.AbnormalReasonUniqueID))
                            {
                                var abnormalReason = db.AbnormalReason.FirstOrDefault(x => x.UniqueID == pipelineAbnormal.AbnormalReasonUniqueID);

                                if (abnormalReason != null)
                                {
                                    description = abnormalReason.Description;
                                }
                                else
                                {
                                    description = pipelineAbnormal.AbnormalReasonRemark;
                                }
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(Model.InspectionUniqueID))
                    {
                        var query = db.Dialog.FirstOrDefault(x => x.InspectionUniqueID == Model.InspectionUniqueID);

                        if (query != null)
                        {
                            subject = string.Empty;
                            description = string.Empty;

                            result.Message = "該討論主題已存在";
                            result.IsSuccess = false;
                        }
                        else
                        {
                            var inspection = db.Inspection.First(x => x.UniqueID == Model.InspectionUniqueID);

                            subject = string.Format("【{0}】#{1}", Resources.Resource.Inspection, inspection.VHNO);

                            if (!string.IsNullOrEmpty(inspection.ConstructionFirmUniqueID))
                            {
                                var firm = db.ConstructionFirm.FirstOrDefault(x => x.UniqueID == inspection.ConstructionFirmUniqueID);

                                if (firm != null)
                                {
                                    description = string.Format("{0}:{1} ", Resources.Resource.ConstructionFirm, firm.Name);
                                }
                                else
                                {
                                    description = string.Format("{0}:{1} ", Resources.Resource.ConstructionFirm, inspection.ConstructionFirmRemark);
                                }
                            }

                            if (!string.IsNullOrEmpty(inspection.ConstructionTypeUniqueID))
                            {
                                var type = db.ConstructionType.FirstOrDefault(x => x.UniqueID == inspection.ConstructionTypeUniqueID);

                                if (type != null)
                                {
                                    description += string.Format("{0}:{1}", Resources.Resource.ConstructionType, type.Description);
                                }
                                else
                                {
                                    description += string.Format("{0}:{1}", Resources.Resource.ConstructionType, inspection.ConstructionTypeRemark);
                                }
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(Model.ConstructionUniqueID))
                    {
                        var query = db.Dialog.FirstOrDefault(x => x.ConstructionUniqueID == Model.ConstructionUniqueID);

                        if (query != null)
                        {
                            subject = string.Empty;
                            description = string.Empty;

                            result.Message = "該討論主題已存在";
                            result.IsSuccess = false;
                        }
                        else
                        {
                            var construction = db.Construction.First(x => x.UniqueID == Model.ConstructionUniqueID);

                            subject = string.Format("【{0}】#{1}", Resources.Resource.Construction, construction.VHNO);

                            if (!string.IsNullOrEmpty(construction.ConstructionFirmUniqueID))
                            {
                                var firm = db.ConstructionFirm.FirstOrDefault(x => x.UniqueID == construction.ConstructionFirmUniqueID);

                                if (firm != null)
                                {
                                    description = string.Format("{0}:{1} ", Resources.Resource.ConstructionFirm, firm.Name);
                                }
                                else
                                {
                                    description = string.Format("{0}:{1} ", Resources.Resource.ConstructionFirm, construction.ConstructionFirmRemark);
                                }
                            }

                            if (!string.IsNullOrEmpty(construction.ConstructionTypeUniqueID))
                            {
                                var type = db.ConstructionType.FirstOrDefault(x => x.UniqueID == construction.ConstructionTypeUniqueID);

                                if (type != null)
                                {
                                    description += string.Format("{0}:{1}", Resources.Resource.ConstructionType, type.Description);
                                }
                                else
                                {
                                    description += string.Format("{0}:{1}", Resources.Resource.ConstructionType, construction.ConstructionTypeRemark);
                                }
                            }
                        }
                    }
                    else
                    { 
                        subject = Model.Subject;
                        description = Model.Description;
                    }

                    if (!string.IsNullOrEmpty(subject))
                    {
                        var dialog = new Dialog()
                        {
                            UniqueID = UniqueID,
                            Subject = subject,
                            Description = description,
                            PipelineAbnormalUniqueID = Model.PipelineAbnormalUniqueID,
                            InspectionUniqueID = Model.InspectionUniqueID,
                            ConstructionUniqueID = Model.ConstructionUniqueID,
                            Extension = extension
                        };

                        db.Dialog.Add(dialog);

                        db.SaveChanges();

                        result.ReturnData(new DialogModel()
                        {
                            UniqueID = dialog.UniqueID,
                            Subject = dialog.Subject,
                            Description = dialog.Description,
                            PipelineAbnormalUniqueID = dialog.PipelineAbnormalUniqueID,
                            InspectionUniqueID = dialog.InspectionUniqueID,
                            ConstructionUniqueID = dialog.ConstructionUniqueID,
                            Extension = dialog.Extension,
                            UserID = Model.UserID
                        });
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

        public static RequestResult GetDialog()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    result.ReturnData(db.Dialog.Select(x => new DialogModel
                    {
                        UniqueID = x.UniqueID,
                        Subject = x.Subject,
                        Description = x.Description,
                        PipelineAbnormalUniqueID = x.PipelineAbnormalUniqueID,
                        InspectionUniqueID = x.InspectionUniqueID,
                        ConstructionUniqueID = x.ConstructionUniqueID,
                        Extension = x.Extension
                    }).ToList());
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

        public static RequestResult GetDialog(List<string> uniqueIDList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {

                    var itemList = new List<DialogModel>();
                    var dialogs = db.Dialog.Where(x => uniqueIDList.Contains(x.UniqueID))
                        .Select(x => new DialogModel {

                            UniqueID = x.UniqueID,
                            Subject = x.Subject,
                            Description = x.Description,
                            PipelineAbnormalUniqueID = x.PipelineAbnormalUniqueID,
                            InspectionUniqueID = x.InspectionUniqueID,
                            ConstructionUniqueID = x.ConstructionUniqueID,
                            Extension = x.Extension
                        
                        
                        }).ToList();
                    itemList = dialogs;
                    result.ReturnData(itemList);
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


        public static RequestResult GetDialog(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var dialog = db.Dialog.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DialogModel()
                    {
                        UniqueID = dialog.UniqueID,
                        Subject = dialog.Subject,
                        Description = dialog.Description,
                        PipelineAbnormalUniqueID = dialog.PipelineAbnormalUniqueID,
                        InspectionUniqueID = dialog.InspectionUniqueID,
                        ConstructionUniqueID = dialog.ConstructionUniqueID,
                        Extension = dialog.Extension
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

        public static RequestResult NewMessage(string UniqueID, MessageFormInput Model, string TempPhotoFolder)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    int seq = 1;

                    var message = db.Message.Where(x => x.DialogUniqueID == Model.DialogUniqueID).OrderByDescending(x => x.Seq).FirstOrDefault();

                    if (message != null)
                    {
                        seq = message.Seq + 1;
                    }

                    var extension = string.Empty;

                    if (!string.IsNullOrEmpty(TempPhotoFolder))
                    {
                        var zipFile = Path.Combine(TempPhotoFolder, string.Format("{0}.zip", UniqueID));

                        var photoFile = string.Empty;

                        using (var zip = ZipFile.Read(zipFile))
                        {
                            if (zip.Count > 0)
                            {
                                photoFile = zip[0].FileName;

                                zip[0].Extract(TempPhotoFolder);
                            }
                        }

                        if (!string.IsNullOrEmpty(photoFile))
                        {
                            var fileInfo = new FileInfo(Path.Combine(TempPhotoFolder, photoFile));

                            extension = fileInfo.Extension.Replace(".", "");

                            System.IO.File.Copy(fileInfo.FullName, Path.Combine(Config.ChatPhotoFolderPath, string.Format("{0}_{1}.{2}", Model.DialogUniqueID, seq, extension)));
                        }
                    }

                    var newMessage = new Message()
                    {
                        DialogUniqueID = Model.DialogUniqueID,
                        Seq = seq,
                        Message1 = Model.Message,
                        MessageTime = DateTime.Now,
                        UserID = Model.UserID,
                        Extension = extension
                    };

                    db.Message.Add(newMessage);

                    db.SaveChanges();

                    result.ReturnData(new MessageModel()
                    {
                        DialogUniqueID = newMessage.DialogUniqueID,
                        Seq = newMessage.Seq,
                        Message = newMessage.Message1,
                        MessageTime = newMessage.MessageTime,
                        StrMessageTime = newMessage.MessageTime.ToString(DateFormateConsts.UI_S_yyyyMMddhhmmss),
                        User = UserHelper.Get(newMessage.UserID),
                        Extension = newMessage.Extension
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

        public static RequestResult GetMessage(string DialogUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<MessageModel>();

                using (PDbEntities db = new PDbEntities())
                {
                    var messageList = db.Message.Where(x => x.DialogUniqueID == DialogUniqueID).OrderBy(x => x.Seq).ToList();

                    foreach (var message in messageList)
                    {
                        itemList.Add(new MessageModel()
                        {
                            DialogUniqueID = message.DialogUniqueID,
                            Seq = message.Seq,
                            Message = message.Message1,
                            MessageTime = message.MessageTime,
                            User = UserHelper.Get(message.UserID),
                            Extension = message.Extension
                        });
                    }
                }

                result.ReturnData(itemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetMessage(string DialogUniqueID, int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<MessageModel>();

                using (PDbEntities db = new PDbEntities())
                {
                    var messageList = db.Message.Where(x => x.DialogUniqueID == DialogUniqueID && x.Seq > Seq).OrderBy(x => x.Seq).ToList();

                    foreach (var message in messageList)
                    {
                        itemList.Add(new MessageModel()
                        {
                            DialogUniqueID = message.DialogUniqueID,
                            Seq = message.Seq,
                            Message = message.Message1,
                            MessageTime = message.MessageTime,
                            User = UserHelper.Get(message.UserID),
                            Extension = message.Extension
                        });
                    }
                }

                result.ReturnData(itemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetMessagePhoto(string DialogUniqueID, int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var message = db.Message.Where(x => x.DialogUniqueID == DialogUniqueID && x.Seq == Seq).FirstOrDefault();
                    if (message != null)
                    {
                        var fileName = string.Format("{0}_{1}.{2}", message.DialogUniqueID, message.Seq, message.Extension);
                        var attachFileName = Path.Combine(Config.ChatPhotoFolderPath, fileName);
                        Logger.Log("attachFileName:" + attachFileName);
                        if (System.IO.File.Exists(attachFileName))
                        {
                            result.ReturnData(attachFileName);
                        }
                        else
                        {
                            throw new Exception(string.Format("file:{0} not found",attachFileName));
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

        /// <summary>
        /// 取得自行開單的討論串
        /// </summary>
        /// <param name="UniqueID"></param>
        /// <returns></returns>
        public static RequestResult GetDialogPhoto(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var chatDialog = db.Dialog.Where(x => x.UniqueID == UniqueID).FirstOrDefault();
                    var zipFileName = Path.Combine(Config.TempFolder, string.Format("{0}{1}.zip", UniqueID, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)));
                    var zipFile = new ZipFile(zipFileName);
                    if(chatDialog != null && string.IsNullOrEmpty(chatDialog.ConstructionUniqueID)  && string.IsNullOrEmpty(chatDialog.PipelineAbnormalUniqueID) && string.IsNullOrEmpty(chatDialog.InspectionUniqueID))
                    {
                        
                        zipFile.AddFile(Path.Combine(Config.ChatPhotoFolderPath, string.Format("{0}.{1}", chatDialog.UniqueID, chatDialog.Extension)), "");
                    }

                    zipFile.Save();

                    result.ReturnData(zipFileName);
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
    }
}
