using Common;
using DBLogic;
using RestSharp;
using System;

namespace MessageClient.Services
{
    public static class LoginService
    {
        public static String IDValidationError = String.Empty;
        public static bool IsIDValidationDone = false;
        /// <summary>
        /// 呼叫WebService驗證ID有效性
        /// </summary>
        /// <returns></returns>
        public static bool CheckIDValidation()
        {
            bool IDValidationResult = false;
            string ID = DBProfile.GetProfile(MainApp.GlobalVariable.DBFile.FullName).ID;
            long UserType = DBProfile.GetUserType(ID, MainApp.GlobalVariable.DBFile.FullName);
            //員工身份
            if (UserType == 1)
            {
                var client = new RestClient(Config.dbWebService);
                client.Timeout = Config.webServiceTimeOut * 1000;
                var request = new RestRequest("api/MoneySQ/JA_EMPOLYEE/CheckIDValidation", Method.POST);
                request.AddParameter("ID", ID, ParameterType.GetOrPost);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                IRestResponse response = client.Execute(request);

                if (response.ErrorMessage != null && response.ErrorMessage != "")
                {
                    IDValidationResult = false;
                    IDValidationError = response.ErrorMessage + "(推播通知服務將無法使用)";
                }
                else
                {
                    if (!Convert.ToBoolean(response.Content))
                    {
                        IDValidationResult = false;
                        IDValidationError = "您的ID已失效,推播通知服務將無法使用";
                    }
                    else
                    {
                        IDValidationResult = true;
                        IDValidationError = String.Empty;
                    }
                }
            }
            //客戶身份
            else if (UserType == 2)
            {
                IDValidationError = "您目前為客戶身份,推播通知服務將無法使用";
            }
            IsIDValidationDone = true;
            return IDValidationResult;
        }
    }
}