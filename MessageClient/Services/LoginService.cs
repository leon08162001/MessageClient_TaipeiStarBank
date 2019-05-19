using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Common;
using DBLogic;
using RestSharp;

namespace MessageClient.Services
{
    public static class LoginService
    {
        public static String IDValidationError = String.Empty;
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
                        IDValidationError = "";
                    }
                }
            }
            //客戶身份
            else if (UserType == 2)
            {

            }
            return IDValidationResult;
        }
    }
}