using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Utility;

namespace Models.AuthGroupManagement
{
    public class FormInput
    {
        [Display(Name = "AuthGroupID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "AuthGroupIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "AuthGroupName", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "AuthGroupNameRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        public string WebPermissionFunctions { get; set; }

        public List<WebPermissionFunction> WebPermissionFunctionList
        {
            get
            {
                var webPermissionFunctionList = new List<WebPermissionFunction>();

                var temp = JsonConvert.DeserializeObject<List<string>>(WebPermissionFunctions);

                foreach (var t in temp)
                {
                    string[] x = t.Split(Define.Seperators, StringSplitOptions.None);

                    webPermissionFunctionList.Add(new WebPermissionFunction()
                    {
                        WebPermissionID = x[0],
                        WebFunctionID = x[1]
                    });
                }

                return webPermissionFunctionList;
            }
        }
    }
}
