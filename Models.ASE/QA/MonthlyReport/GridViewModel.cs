﻿using Models.ASE.QA.CALnMSAReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MonthlyReport
{
    public class GridViewModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<DateKey> KeyList
        {
            get
            {
                var keyList = new List<DateKey>();

                var date = BeginDate;

                while (date <= EndDate)
                {
                    keyList.Add(new DateKey()
                    {
                        BeginDate = date,
                        EndDate = date.AddMonths(1).AddDays(-1)
                    });

                    date = date.AddMonths(1);
                }

                return keyList;
            }
        }

        public List<CalGridItem> CalItemList { get; set; }

        public List<MSAGridItem> MSAItemList { get; set; }

        public int ItemCount
        {
            get
            {
                return CalItemList.Count + MSAItemList.Count;
            }
        }

        public GridViewModel()
        {
            CalItemList = new List<CalGridItem>();
            MSAItemList = new List<MSAGridItem>();
        }
    }
}
