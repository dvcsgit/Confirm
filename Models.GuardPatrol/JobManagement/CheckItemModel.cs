﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.GuardPatrol.JobManagement
{
    public class CheckItemModel
    {
        public bool IsChecked { get; set; }

        public string UniqueID { get; set; }

        public string CheckType { get; set; }

        public string CheckItemID { get; set; }

        public string CheckItemDescription { get; set; }

        public int Seq { get; set; }
    }
}
