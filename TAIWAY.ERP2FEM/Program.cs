using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAIWAY.ERP2FEM
{
    class Program
    {
        static void Main(string[] args)
        {
            using (FTPHelper helper = new FTPHelper())
            {
                helper.Get();
            }
        }
    }
}
