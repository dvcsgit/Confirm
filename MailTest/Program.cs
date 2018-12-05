using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace MailTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var recipientList = new List<MailAddress>() 
            {
                new MailAddress("N000114964@fpg", "呂明宗")
            };

            MailHelper.SendMail(recipientList, "測試發送信件(FPCC.RFIDCHK)", "Hello! World!");
        }
    }
}
