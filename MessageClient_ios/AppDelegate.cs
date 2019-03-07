using Common;
using Common.LinkLayer;
using DBLogic;
using DBModels;
using Foundation;
using MessageClient_ios.Services;
using MessageClient_ios.Util;
using MessageClient_ios.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using UIKit;

namespace MessageClient_ios
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
        // class-level declarations
        public static GlobalVariable GlobalVariable = new GlobalVariable();
        public static IMQAdapter MQJefferiesExcuReport;

        public override UIWindow Window {
			get;
			set;
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method
            ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
            AlertHelper.AppWindow = this.Window;
            SQLitePCL.Batteries_V2.Init();
            bool result = false;
            result = InitialConfig();
            if(!result)
            {
                return result;
            }
            //跑模擬器需註解
            //result = CheckNetWorkStatus();
            //if (!result)
            //{
            //    return result;
            //}
            result = InitialDB();
            if (!result)
            {
                return result;
            }
            try
            {
                InitialNotification();
                if (DBProfile.CheckProfileExist(AppDelegate.GlobalVariable.DBFile.FullName))
                {
                    Common.LogHelper.MoneySQLogger.DeleteLogFile(ConfigIOS.preservedDaysForLog, Common.LogHelper.MoneySQLogger.logPath);
                    MQService.StartService();
                    result = true;
                }
                else
                {
                    //開啟推播身分設定頁面
                    AlertHelper.ShowTextInputAlert("推播身份設定", "請輸入欲接收推播的身分ID.", UIAlertControllerStyle.Alert, "身份ID", "", null,
                    (r, id) => { result = CheckProfileOK(r, id); }, 
                    cancel=> {
                        result = false;
                        AlertHelper.ShowOKAlert("推播身份設定", "尚未設定接收推播的身分ID!", UIAlertControllerStyle.Alert, null, null);
                    });
                }
        }
            catch (Exception ex)
            {
                result = false;
                MQService.ErrorInMainApp = ex.ToString();
                Common.LogHelper.MoneySQLogger.LogError<AppDelegate>(ex);
            }
            // check for a notification

            if (launchOptions != null)
            {
                // check for a local notification
                if (launchOptions.ContainsKey(UIApplication.LaunchOptionsLocalNotificationKey))
                {
                    var localNotification = launchOptions[UIApplication.LaunchOptionsLocalNotificationKey] as UILocalNotification;
                    if (localNotification != null)
                    {
                        UIAlertController okayAlertController = UIAlertController.Create(localNotification.AlertAction, localNotification.AlertBody, UIAlertControllerStyle.Alert);
                        okayAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

                        Window.RootViewController.PresentViewController(okayAlertController, true, null);

                        // reset our badge
                        UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
                    }
                }
            }
            return result;
		}
        public override void OnResignActivation (UIApplication application)
		{
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
            //當app進入非使用狀態時,切換到歷史訊息頁籤,以便點系統通知欄重啟app時能重新trigger訊息公告頁籤的事件
            UITabBarController TabController = (UITabBarController)this.Window.RootViewController;
            AppDelegate.GlobalVariable.CurrentTabIndex = (int)TabController.SelectedIndex;
            TabController.SelectedIndex = 1;
        }

		public override void DidEnterBackground (UIApplication application)
		{
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
            //StartService();
            MQService.StartService();
        }

        public override void WillEnterForeground (UIApplication application)
		{
			// Called as part of the transiton from background to active state.
			// Here you can undo many of the changes made on entering the background.
		}

		public override void OnActivated (UIApplication application)
		{
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
            UITabBarController TabController = (UITabBarController)this.Window.RootViewController;
            TabController.SelectedIndex = AppDelegate.GlobalVariable.CurrentTabIndex;
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
        /// <summary>
        /// Config init
        /// </summary>
        private bool InitialConfig()
        {
            bool result = false;
            try
            {
                FileInfo IniFile = new FileInfo(Path.Combine(NSBundle.MainBundle.BundlePath, ".common.ini"));
                if (!IniFile.Exists)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    var resource = assembly.GetName().Name + ".Resources.common.ini";
                    using (var stream = assembly.GetManifestResourceStream(resource))
                    {
                        if (stream != null)
                        {
                            //先設定log檔的路徑為MyDocuments
                            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            var logDir = Path.Combine(documentsPath, "");
                            Common.LogHelper.MoneySQLogger.logPath = logDir;
                            ConfigIOS.ConfigStream = stream;
                            ConfigIOS.ReadParameter();
                            //ini檔讀取成功後重新設定log檔的路徑
                            logDir = Path.Combine(documentsPath, ConfigIOS.logDir);
                            Common.LogHelper.MoneySQLogger.logPath = logDir;
                            Common.LogHelper.MoneySQLogger.LogInfo<AppDelegate>("讀取INI參數成功");
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                MQService.ErrorInMainApp = ex.ToString();
                Common.LogHelper.MoneySQLogger.LogError<AppDelegate>(ex);
            }
            return result;
        }
        /// <summary>
        /// DB init
        /// </summary>
        private bool InitialDB()
        {
            bool Result = false;
            try
            {
                if (!AppDelegate.GlobalVariable.DBFile.Exists)
                {
                    string document = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // Documents folder
                    string documentPath = Path.Combine(document, "..", "Library");
                    if (!Directory.Exists(documentPath))
                    {
                        Directory.CreateDirectory(documentPath);
                    }
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    var resource = assembly.GetName().Name + ".Resources.db.MoneySQ.db";
                    using (var stream = assembly.GetManifestResourceStream(resource))
                    {
                        if (stream != null)
                        {
                            using (FileStream file = new FileStream(Path.Combine(documentPath, "MoneySQ.db"), FileMode.Create, FileAccess.ReadWrite))
                            {
                                stream.CopyTo(file);
                                Result = true;
                            }
                        }
                    }
                }
                Result = true;
            }
            catch (Exception ex)
            {
                Result = false;
                MQService.ErrorInMainApp = ex.ToString();
                Common.LogHelper.MoneySQLogger.LogError<AppDelegate>(ex);
            }
            return Result;
        }
        /// <summary>
        /// 檢查wifi和行動網路狀態
        /// </summary>
        private bool CheckNetWorkStatus()
        {
            bool result = false;
            if(Reachability.InternetConnectionStatus() == NetworkStatus.NotReachable)
            {
                string logText = "WIFI or 行動網路尚未開啟連線";
                MQService.ErrorInMainApp = logText;
                MQService.IsFinishedMqService = true;
                Common.LogHelper.MoneySQLogger.LogInfo<AppDelegate>(logText);
            }
            result = true;
            return result;
        }
        private void InitialNotification()
        {
            //if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            //{
            //    // Ask the user for permission to get notifications on iOS 10.0+
            //    UNUserNotificationCenter.Current.RequestAuthorization(
            //            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
            //            (approved, error) => { });
            //}
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                // Ask the user for permission to get notifications on iOS 8.0+
                var settings = UIUserNotificationSettings.GetSettingsForTypes(
                        UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                        new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }
            //if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            //{
            //    var notificationSettings = UIUserNotificationSettings.GetSettingsForTypes(
            //        UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, null
            //    );

            //    application.RegisterUserNotificationSettings(notificationSettings);
            //}
        }
        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            var PushID = (notification.UserInfo[new NSString("PushMessageID")] as NSString).ToString();
            MessageAddressee MA = DBMessageAddressee.GetMessageByPushID(PushID, AppDelegate.GlobalVariable.DBFile.FullName);
            AppDelegate.GlobalVariable.MessageID = MA.PushMessageID;
            AppDelegate.GlobalVariable.Subject = MA.Subject;
            AppDelegate.GlobalVariable.MessageText = MA.Message;
            AppDelegate.GlobalVariable.Attachments = MA.Attachments == null ? "" : MA.Attachments;
            AppDelegate.GlobalVariable.SendedMessageTime = MA.SendedMessageTime;
            AppDelegate.GlobalVariable.ReceivedMessageTime = MA.ReceivedMessageTime;
            AppDelegate.GlobalVariable.CurrentTabIndex = 0;


            UITabBarController TabController = (UITabBarController)this.Window.RootViewController;
            //TabController.SelectedIndex = 0;
            MQService.UnReadNotificationCount = 0;
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = MQService.UnReadNotificationCount;
            UIApplication.SharedApplication.CancelAllLocalNotifications();
        }
        private bool CheckProfileOK(bool Result, string InputID)
        {
            bool res = false;
            if (!InputID.Trim().Equals(""))
            {
                var client = new RestClient(ConfigIOS.dbWebService);
                var request = new RestRequest("api/MoneySQ/JA_EMPOLYEE/GetEmployeeID", Method.POST);
                request.AddParameter("ID", InputID.Trim(), ParameterType.QueryString);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                IRestResponse response = client.Execute(request);
                if (response.ErrorMessage != null && response.ErrorMessage != "")
                {
                    Common.LogHelper.MoneySQLogger.LogInfo<AppDelegate>(response.ErrorMessage);
                    AlertHelper.ShowOKAlert("推播身份設定", response.ErrorMessage, UIAlertControllerStyle.Alert, null, null);
                    return res;
                }
                if (!IsValidJson(response.Content))
                {
                    string ID = EscapeForQuote(response.Content);
                    if (ID.Equals(""))
                    {
                        request = new RestRequest("api/MoneySQ/ZZ_APPLICATION/GetApplicantIDByID", Method.POST);
                        request.AddParameter("ID", InputID.Trim(), ParameterType.QueryString);
                        request.AddHeader("cache-control", "no-cache");
                        request.AddHeader("content-type", "application/json");
                        response = client.Execute(request);
                        if (!IsValidJson(response.Content))
                        {
                            ID = EscapeForQuote(response.Content);
                        }
                        else
                        {
                            ShowJsonError(response.Content);
                        }
                    }
                    if (!ID.Equals(""))
                    {
                        if (ID.ToUpper().Equals(InputID.Trim().ToUpper()))
                        {
                            Profile Profile = new Profile();
                            Profile.ID = ID;
                            if (DBProfile.InsertProfile(Profile, AppDelegate.GlobalVariable.DBFile.FullName))
                            {
                                //更新MoneySQ後台DB(JA_EMPOLYEE)
                                request = new RestRequest("api/MoneySQ/JA_EMPOLYEE/UpdateEmployeePushByID", Method.PUT);
                                request.AddParameter("ID", ID, ParameterType.QueryString);
                                request.AddHeader("cache-control", "no-cache");
                                request.AddHeader("content-type", "application/json");
                                response = client.Execute(request);
                                //更新MoneySQ後台DB(ZZ_APPLICATION)
                                request = new RestRequest("api/MoneySQ/ZZ_APPLICATION/UpdateApplicantPushByID", Method.PUT);
                                request.AddParameter("ID", ID, ParameterType.QueryString);
                                request.AddHeader("cache-control", "no-cache");
                                request.AddHeader("content-type", "application/json");
                                response = client.Execute(request);
                                MQService.StartService();
                                res = true;
                                AlertHelper.ShowOKAlert("推播身份設定", "設定完成接收推播的ID!", UIAlertControllerStyle.Alert, null, null);
                            }
                            else
                            {
                                Common.LogHelper.MoneySQLogger.LogInfo<AppDelegate>("無法新增ID至Profile設定檔!");
                                AlertHelper.ShowOKAlert("推播身份設定", "無法新增ID至Profile設定檔!", UIAlertControllerStyle.Alert, null, null);
                            }
                        }
                        else
                        {
                            Common.LogHelper.MoneySQLogger.LogInfo<AppDelegate>(ID);
                            AlertHelper.ShowOKAlert("推播身份設定", ID, UIAlertControllerStyle.Alert, null, null);
                        }
                    }
                    else
                    {
                        Common.LogHelper.MoneySQLogger.LogInfo<AppDelegate>("設定失敗,因申請檔及員工檔找不到符合的ID!");
                        AlertHelper.ShowOKAlert("推播身份設定", "設定失敗,因申請檔及員工檔找不到符合的ID!", UIAlertControllerStyle.Alert, null, null);
                    }
                }
                else
                {
                    ShowJsonError(response.Content);
                }
            }
            else
            {
                AlertHelper.ShowOKAlert("推播身份設定", "請輸入您的身分ID!", UIAlertControllerStyle.Alert, null, a => AlertHelper.ShowTextInputAlert("推播身份設定", "請輸入欲接收推播的身分ID.", UIAlertControllerStyle.Alert, "身份ID", "", null,
                (r, id) => { res = CheckProfileOK(r, id); },
                cancel => {
                    res = false;
                    AlertHelper.ShowOKAlert("推播身份設定", "尚未設定接收推播的身分ID!", UIAlertControllerStyle.Alert, null, null);
                }));
            }
            return res;
        }
        private bool IsValidJson(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }
            var value = s.Trim();
            if ((value.StartsWith("{") && value.EndsWith("}")) || //For object
                (value.StartsWith("[") && value.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(value);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
            }
            return false;
        }
        private void ShowJsonError(string json)
        {
            JObject JObj = JsonConvert.DeserializeObject<JObject>(json);
            if (JObj["ErrorCode"].Type != JTokenType.Null && JObj["ErrorMessage"].Type != JTokenType.Null)
            {
                Common.LogHelper.MoneySQLogger.LogInfo<AppDelegate>(JObj["ErrorMessage"].ToString());
                AlertHelper.ShowOKAlert("推播身份設定", JObj["ErrorMessage"].ToString(), UIAlertControllerStyle.Alert, null, null);
            }
        }
        private string EscapeForQuote(string s)
        {
            string result = s;
            result = result.Replace(@"""", "");
            return result;
        }
    }
}