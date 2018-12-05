using System.Web.Mvc;
using System.Collections.Generic;
using Utility;
using System.IO;

namespace Models.GuardPatrol.MobileRelease
{
    public class UploadFormModel
    {
        public string TempGuid { get; set; }

        public string Extension { get; set; }

        public string TempFile
        {
            get
            {
                return Path.Combine(Config.TempFolder, TempGuid + "." + Extension);
            }
        }

        public List<SelectListItem> DeviceSelectItemList { get; set; }

        public FormInput FormInput { get; set; }

        public UploadFormModel()
        {
            DeviceSelectItemList = new List<SelectListItem>() 
            { 
                Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                new SelectListItem() { Text = Define.EnumDevice.Android.ToString(), Value = Define.EnumDevice.Android.ToString() },
                new SelectListItem() { Text = Define.EnumDevice.WindowsMobile.ToString(), Value = Define.EnumDevice.WindowsMobile.ToString() },
                new SelectListItem() { Text = Define.EnumDevice.WindowsCE.ToString(), Value = Define.EnumDevice.WindowsCE.ToString() }
            };

            FormInput = new FormInput();
        }
    }
}
