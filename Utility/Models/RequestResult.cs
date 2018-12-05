using System;
using System.Reflection;

namespace Utility.Models
{
    public class RequestResult
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }

        public Error Error { get; set; }

        public void Success()
        {
            this.IsSuccess = true;
            this.Message = string.Empty;
            this.Data = null;
            this.Error = null;
        }

        public void ReturnData(object Data)
        {
            this.IsSuccess = true;
            this.Message = string.Empty;
            this.Data = Data;
            this.Error = null;
        }

        public void ReturnData(object Data, string Message)
        {
            this.IsSuccess = true;
            this.Message = Message;
            this.Data = Data;
            this.Error = null;
        }

        public void ReturnSuccessMessage(string Message)
        {
            this.IsSuccess = true;
            this.Message = Message;
            this.Data = null;
            this.Error = null;
        }

        public void Failed()
        {
            this.IsSuccess = false;
            this.Message = string.Empty;
            this.Data = null;
            this.Error = null;
        }

        public void ReturnFailedMessage(string Message)
        {
            this.IsSuccess = false;
            this.Message = Message;
            this.Data = null;
            this.Error = null;
        }

        public void ReturnError(Error Error)
        {
            this.IsSuccess = false;
            this.Message = Error.ErrorMessage;
            this.Data = null;
            this.Error = Error;
        }

        public void ReturnError(MethodBase Method, Exception Exception)
        {
            var err = new Error(Method, Exception);

            this.IsSuccess = false;
            this.Message = err.ErrorMessage;
            this.Data = null;
            this.Error = err;
        }
    }
}
