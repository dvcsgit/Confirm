﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.AbnormalNotify.AbnormalNotifyGroup
{
    public class FormInput
    {
        [DisplayName("通知群組名稱")]
        [Required(ErrorMessage = "請輸入通知群組名稱")]
        public string Description { get; set; }
    }
}
