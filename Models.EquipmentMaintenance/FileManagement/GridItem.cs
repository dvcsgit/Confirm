using System;
using Utility;

namespace Models.EquipmentMaintenance.FileManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string FullPathDescription { get; set; }

        public string FileName { get; set; }

        public string Extension { get; set; }

        public string Display
        {
            get
            {
                return string.Format("{0}.{1}", FileName, Extension);
            }
        }

        public int Size { get; set; }

        public string FileSize
        {
            get
            {
                var index = 0;

                var size = Size;

                while (size > 1024)
                {
                    size = size / 1024;
                    index++;
                }

                return string.Format("{0} {1}", size, Define.FileSizeDescription[index]);
            }
        }

        public string UserID { get; set; }

        public string UserName { get; set; }

        public string User
        {
            get
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    return string.Format("{0}/{1}", UserID, UserName);
                }
                else
                {
                    return UserID;
                }
            }
        }

        public DateTime LastModifyTime { get; set; }

        public string LastModifyTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(LastModifyTime);
            }
        }
    }
}
