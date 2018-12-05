using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace Utility.Models
{
    public class Error
    {
        public string OccurTime { get; private set; }

        private MethodBase Method { get; set; }

        public string MethodName
        {
            get
            {
                if (this.Method != null)
                {
                    return string.Format("{0}.{1}", Method.ReflectedType.FullName, Method.Name);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private Exception Exception { get; set; }

        public List<string> ErrorMessageList
        {
            get
            {
                var errorMessageList = new List<string>();

                var exception = Exception;

                while (exception != null && !string.IsNullOrEmpty(exception.Message))
                {
                    errorMessageList.Add(exception.Message);

                    exception = exception.InnerException;
                }

                return errorMessageList;
            }
        }

        public string ErrorMessage
        {
            get
            {
                if (ErrorMessageList != null && ErrorMessageList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var errorMessage in ErrorMessageList)
                    {
                        sb.Append(errorMessage);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public Error(MethodBase Method, Exception Exception)
        {
            this.OccurTime = DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now);
            this.Method = Method;
            this.Exception = Exception;
        }

        public Error(MethodBase Method, string ErrorMessage)
        {
            this.OccurTime = DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now);
            this.Method = Method;
            this.Exception = new Exception(ErrorMessage);
        }
    }
}
