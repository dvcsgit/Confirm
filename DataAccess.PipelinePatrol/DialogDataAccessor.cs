using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using Models.Authenticated;
using Models.PipelinePatrol.Message;
using Models.PipelinePatrol.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.PipelinePatrol
{
    public class DialogDataAccessor
    {
        public static RequestResult GetDialog()
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<DialogGridItem>();

                using (PDbEntities db = new PDbEntities())
                {
                    var dialogList = db.Dialog.ToList();

                    foreach (var dialog in dialogList)
                    {
                        var lastMessage = db.Message.Where(x => x.DialogUniqueID == dialog.UniqueID).OrderByDescending(x => x.MessageTime).FirstOrDefault();

                        itemList.Add(new DialogGridItem()
                        {
                            UniqueID = dialog.UniqueID,
                            Subject = dialog.Subject,
                            Description = dialog.Description,
                            PipelineAbnormalUniqueID = dialog.PipelineAbnormalUniqueID,
                            InspectionUniqueID = dialog.InspectionUniqueID,
                            ConstructionUniqueID = dialog.ConstructionUniqueID,
                            Extension = dialog.Extension,
                            LastMessageTime = lastMessage != null ? lastMessage.MessageTime : default(DateTime?)
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

        public static RequestResult GetDialog(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DialogModel();

                using (PDbEntities db = new PDbEntities())
                {
                    var dialog = db.Dialog.First(x => x.UniqueID == UniqueID);

                    model = new DialogModel()
                    {
                        UniqueID = dialog.UniqueID,
                        Subject = dialog.Subject,
                        Description = dialog.Description
                    };

                    var messageList = db.Message.Where(x => x.DialogUniqueID == UniqueID).OrderBy(x => x.MessageTime).ToList();

                    foreach (var message in messageList)
                    {
                        model.MessageList.Add(new MessageModel()
                        {
                            DialogUniqueID = message.DialogUniqueID,
                            Seq = message.Seq,
                            Message = message.Message1,
                            MessageTime = message.MessageTime,
                            User = GetUser(message.UserID),
                            Extension = message.Extension
                        });   
                    }
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

        public static RequestResult GetMessage(string DialogUniqueID, int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<MessageModel>();

                using (PDbEntities db = new PDbEntities())
                {
                    var messageList = db.Message.Where(x => x.DialogUniqueID == DialogUniqueID && x.Seq > Seq).OrderBy(x => x.MessageTime).ToList();

                    foreach (var message in messageList)
                    {
                        itemList.Add(new MessageModel()
                        {
                            DialogUniqueID = message.DialogUniqueID,
                            Seq = message.Seq,
                            Message = message.Message1,
                            MessageTime = message.MessageTime,
                            User = GetUser(message.UserID),
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

        public static RequestResult NewMessage(string DialogUniqueID, string Message, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    int seq = 1;

                    var messages = db.Message.Where(x => x.DialogUniqueID == DialogUniqueID).OrderByDescending(x => x.Seq).ToList();

                    if (messages.Count > 0)
                    {
                        seq = messages.Max(x => x.Seq) + 1;
                    }

                    var message = new Message()
                    {
                        DialogUniqueID = DialogUniqueID,
                        Seq = seq,
                        Message1 = Message,
                        MessageTime = DateTime.Now,
                        UserID = Account.ID
                    };

                    db.Message.Add(message);

                    db.SaveChanges();

                    result.Success();

                    FCMTools fcmTools = new FCMTools();
                    var fcmRes = fcmTools.PushData<PushMessageModel>(new PushMessageModel {
                        DialogUniqueID = message.DialogUniqueID,
                        Seq = message.Seq,
                        Message = message.Message1,
                        MessageTime = message.MessageTime,
                        StrMessageTime = message.MessageTime.ToString(DateFormateConsts.UI_S_yyyyMMddhhmmss),
                        User = GetUser(message.UserID),
                        Extension = message.Extension
                    }, FCMTools.TOPIC_MESSAGE + "_" + DialogUniqueID);
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

        public static RequestResult NewPhoto(string DialogUniqueID, string TempFile, string Extension, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    int seq = 1;

                    var messages = db.Message.Where(x => x.DialogUniqueID == DialogUniqueID).OrderByDescending(x => x.Seq).ToList();

                    if (messages.Count > 0)
                    {
                        seq = messages.Max(x => x.Seq) + 1;
                    }

                    var message = new Message()
                    {
                        DialogUniqueID = DialogUniqueID,
                        Seq = seq,
                        Extension = Extension,
                        MessageTime = DateTime.Now,
                        UserID = Account.ID
                    };

                    db.Message.Add(message);

                    System.IO.File.Copy(TempFile, Path.Combine(Config.ChatPhotoFolderPath, string.Format("{0}_{1}.{2}", DialogUniqueID, seq, Extension)));

                    db.SaveChanges();

                    result.Success();

                    FCMTools fcmTools = new FCMTools();
                    var fcmRes = fcmTools.PushData<PushMessageModel>(new PushMessageModel
                    {
                        DialogUniqueID = message.DialogUniqueID,
                        Seq = message.Seq,
                        Message = message.Message1,
                        MessageTime = message.MessageTime,
                        StrMessageTime = message.MessageTime.ToString(DateFormateConsts.UI_S_yyyyMMddhhmmss),
                        User = GetUser(message.UserID),
                        Extension = message.Extension
                    }, FCMTools.TOPIC_MESSAGE + "_" + DialogUniqueID);
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

        public static UserModel GetUser(string UserID)
        {
            var userModel = new UserModel();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    userModel = (from u in db.User
                                 join o in db.Organization
                                 on u.OrganizationUniqueID equals o.UniqueID
                                 where u.ID == UserID
                                 select new UserModel
                                 {
                                     OrganizationDescription = o.Description,
                                     ID = u.ID,
                                     Name = u.Name
                                 }).FirstOrDefault();

                    if (userModel == null)
                    {
                        userModel = new UserModel() { ID = UserID };
                    }
                    else
                    {
                        var photo = db.UserPhoto.FirstOrDefault(x => x.UserID == UserID);

                        if (photo != null)
                        {
                            userModel.Photo = new UserPhotoModel
                            {
                                FileUniqueID = photo.FileUniqueID,
                                Extension = photo.Extension
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                userModel = new UserModel() { ID = UserID };

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return userModel;
        }
    }

    public class PushMessageModel
    {
        public string DialogUniqueID { get; set; }

        public int Seq { get; set; }

        public UserModel User { get; set; }

        public bool IsPhoto
        {
            get
            {
                return !string.IsNullOrEmpty(Extension);
            }
        }

        public string Photo
        {
            get
            {
                if (IsPhoto)
                {
                    return string.Format("{0}_{1}.{2}", DialogUniqueID, Seq, Extension);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Message { get; set; }

        public string Extension { get; set; }

        public DateTime MessageTime { get; set; }

        public string StrMessageTime { get; set; }

        public PushMessageModel()
        {
            User = new UserModel();
        }
    }
}
