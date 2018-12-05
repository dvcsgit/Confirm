using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models.Shared
{
    public class TreeItem
    {
        /// <summary>
        /// 標題
        /// </summary>
        [JsonProperty("data")]
        public string Title { get; set; }

        /// <summary>
        /// 屬性
        /// </summary>
        [JsonProperty("attr")]
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// 是否開啟
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }

        /// <summary>
        /// 子節點
        /// </summary>
        [JsonProperty("children")]
        public List<TreeItem> Children { get; set; }

        public TreeItem()
        {
            Attributes = new Dictionary<string, string>();
            Children = new List<TreeItem>();
        }
    }
}
