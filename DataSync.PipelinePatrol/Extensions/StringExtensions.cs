using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace DataSync.PipelinePatrol.Extensions
{
    public static class StringExtensions
    {
        public static string GetMD5HashForJAVA(this string instance)
        {
            DataContractSerializer serializer = new DataContractSerializer(instance.GetType());
            Logger.Log("=======");
            Encoding utf8 = Encoding.UTF8;
            byte[] bytes = utf8.GetBytes(instance);//enc.GetBytes(s);

            ////byte[] bytes = memoryStream.ToArray();
            //foreach (var item in bytes)
            //{
            //    Logger.Log(item.ToString());
            //}

            //byte[] bytes = { 0x35, 0x24, 0x76, 0x12 };
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(bytes);

            
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                sb.Append(result[i].ToString("x2"));
                
            }
            Logger.Log("=======");
            return sb.ToString();

            //using (MemoryStream memoryStream = new MemoryStream())
            //{
            //    serializer.WriteObject(memoryStream, instance);
            //    //cryptoServiceProvider.ComputeHash(memoryStream.ToArray());
            //    //return Convert.ToBase64String(cryptoServiceProvider.Hash);
   
            //}

            //byte[] bytes = { 0x35, 0x24, 0x76, 0x12 };

            //MD5 md5 = new MD5CryptoServiceProvider();
            //byte[] result = md5.ComputeHash(bytes);
            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < result.Length; i++)
            //{
            //    sb.Append(result[i].ToString("x2"));
            //}
            //Console.WriteLine(sb);
            //return sb.ToString();

        }
    }
}
