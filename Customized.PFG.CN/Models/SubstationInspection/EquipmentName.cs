using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.PFG.CN.Models.SubstationInspection
{
    public class EquipmentName
    {
        public static  string[] AllEquipment()
        {
            string[] equipmentName=new string[]{
                "TRA1","TRA2","TRA3","TRA4","TRA5","TRA7","TRA8","TRA9","TRA10","TRA11","TRA12","TRA13","TRA14",
                "TRB1","TRB2","TRB3","TRB4","TRB5","TRB7","TRB8","TRB9","TRB10","TRB11","TRB12",
                "TRC1","TRC2","TRC3","TRC4","TRC5","TRC7","TRC8","TRC9","TRC10","TRC11","TRC12","純氧二期饋電盤",
                "TRD1","TRD2","TRD3","TRD4","TRD5","TRD7","TRD8","TRD9","TRD10","TRD11","純氧一期饋電盤",
                "空壓機CA01","空壓機CA02","空壓機CA201","空壓機CA202","空壓機CA401","空壓機CA03","空壓機IA01",
                "冷凍機CH01","冷凍機CH02","冷凍機CH03","冷凍機CH04","冷凍機CH201","冷凍機CH202","冷凍機CH203","冷凍機CH301","冷凍機CH302","冷凍機CH303","冷凍機CH401","冷凍機CH402",
                "A饋主受電盤","B饋主受電盤","C饋主受電盤","D饋主受電盤","室外氣溫"
            };

            return equipmentName;
        }
    }
}
