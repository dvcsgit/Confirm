﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.CalibrationForm
{
    public class VerifyFormInput
    {
        [Display(Name = "VerifyComment", ResourceType = typeof(Resources.Resource))]
        public string Comment { get; set; }
    }
}
