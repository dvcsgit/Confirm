﻿using Models.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.PipelinePatrol.FileManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "FilePath", ResourceType = typeof(Resources.Resource))]
        public string FullPathDescription { get; set; }

        public string FileName { get; set; }

        public string Extension { get; set; }

        [Display(Name = "FileName", ResourceType = typeof(Resources.Resource))]
        public string Display
        {
            get
            {
                return string.Format("{0}.{1}", FileName, Extension);
            }
        }

        public int Size { get; set; }

        [Display(Name = "FileSize", ResourceType = typeof(Resources.Resource))]
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

        [Display(Name = "IsDownload2Mobile", ResourceType = typeof(Resources.Resource))]
        public bool IsDownload2Mobile { get; set; }

        [Display(Name = "FileAuthor", ResourceType = typeof(Resources.Resource))]
        public UserModel User { get; set; }

        public DateTime LastModifyTime { get; set; }

        [Display(Name = "FileTime", ResourceType = typeof(Resources.Resource))]
        public string LastModifyTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(LastModifyTime);
            }
        }

        public DetailViewModel()
        {
            User = new UserModel();
        }
    }
}
