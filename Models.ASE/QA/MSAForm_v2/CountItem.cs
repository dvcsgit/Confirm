﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MSAForm_v2
{
    public class CountItem
    {
         public string Appraiser { get; set; }

        public string UserID { get; set; }

        public List<CountValue> ValueList { get; set; }

        public CountItem() {
            ValueList = new List<CountValue>();
        }
    }
}
