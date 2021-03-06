﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.PipelinePatrol
{
    public class FCMTools
    {
        /// <summary>
        /// 請抽成 設定值
        /// </summary>
        protected readonly string SERVER_KEY = "AAAAOKqJgYs:APA91bEdYG_oP7fzUiPvmIhbexQvMhOxA_OM_NZNkCtnbj7RyHg3ZKudLo2Y7NQUDbgk_V-nZ3YeG91c1Qe0tYC5ibswPp-srnqMtv-S88AaoutE8N0QNRuxj8SOLYl0lLmhhXQTkRNJ";
        protected readonly string SERVER_URL = "https://fcm.googleapis.com/fcm/send"; //google server

        /// <summary>
        /// 全域
        /// </summary>
        public static readonly string TOPIC_GLOBAL = "global";

        /// <summary>
        /// 對話
        /// </summary>
        public static readonly string TOPIC_DIALOG = "dialog";

        /// <summary>
        /// 對話訊息
        /// </summary>
        public static readonly string TOPIC_MESSAGE = "message";

        /// <summary>
        /// 人員登入
        /// </summary>
        public static readonly string TOPIC_USER = "user";

        /// <summary>
        /// 單據
        /// </summary>
        public static readonly string TOPIC_FORM = "form";

        public enum Priority
        {
            normal,//such as notifications of new email or other data to sync
            high,//Apps with instant messaging, chat, or voice call alerts
        }

        /// <summary>
        /// 訊息推送
        /// NOTE: 同步的 (等 FCM server response 才會執行下一步)
        /// </summary>
        /// <typeparam name="T">pass 給 google 的 FCM server 的型別</typeparam>
        /// <param name="source">資料</param>
        /// <param name="topic"></param>
        /// <returns>TODO 目前回傳字串 需要看官方文件 才能知道會回傳什麼資料</returns>
        public string PushData<T>(T source, string topic)
        {
            var client = new RestClient(SERVER_URL);
            var request = new RestRequest(Method.POST);

            // easily add HTTP Headers
            request.AddHeader("Authorization", "key=" + SERVER_KEY);
            request.AddHeader("Content-Type", "application/json");
            var sendData = new MessageData<T>()
            {
                priority = Priority.high.ToString(),
                data = source,
                to = "/topics/" + topic
            };

            request.AddJsonBody(sendData);

            // execute the request
            IRestResponse response = client.Execute(request);

            return response.Content; // raw content as string
        }
    }

    public class MessageData<T>
    {
        /// <summary>
        /// limit 4K
        /// </summary>
        public T data { get; set; }

        //Optional
        public string to { get; set; }

        /// <summary>
        /// high or normal
        /// </summary>
        public string priority { get; set; }

        /// <summary>
        /// TTL range: 
        /// 0 to 2,419,200 seconds
        /// (about 28 days )
        /// </summary>
        public int time_to_live { get; set; }



        public List<string> registration_ids { get; set; }

        /// <summary>
        /// 塞topic
        /// </summary>
        public string condition { get; set; }


        /// <summary>
        /// 最多4個
        /// </summary>
        public string collapse_key { get; set; }


        /// <summary>
        /// true => 
        /// </summary>
        public bool delay_while_idle { get; set; }



    }
}
