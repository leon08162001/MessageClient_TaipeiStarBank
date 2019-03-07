using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Android.Widget;
using Common;
using Common.LinkLayer;
using DBLogic;
using DBModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Utility;
using MessageClient.Utils;

namespace MessageClient.Services
{
    [Service]
    public class EMSService : Service
    {
        public static bool IsNewMessageData = false;
        public static IEMSAdapter EMSJefferiesExcuReport;
        public static PendingIntent EMSJefferiesExcuReportPendingIntent;
        public static bool IsFinishedEmsService = false;
        public static string ErrorInMainApp = "";
        private NotificationManager nMgr;
        private Notification EMSJefferiesExcuReportNotification;
        private WifiManager WifiManager;

        //服務被建立的事件處理
        public override void OnCreate()
        {
            base.OnCreate();
            WifiManager = GetSystemService(Context.WifiService) as WifiManager;

            //取得系統的通知管理員
            nMgr = (NotificationManager)GetSystemService(NotificationService);

            //宣告EMSJefferiesExcuReportNotification物件
            EMSJefferiesExcuReportNotification = new Notification(Resource.Drawable.message, "EMSJefferiesExcuReport推播訊息通知");
            EMSJefferiesExcuReportNotification.Defaults = NotificationDefaults.Sound;
            EMSJefferiesExcuReportNotification.Flags = NotificationFlags.AutoCancel;

            EMSJefferiesExcuReport = TopicEMSFactory.GetEMSAdapterInstance(EMSAdapterType.BatchEMSAdapter);
            EMSJefferiesExcuReport.Uri = Config.EMS_network + ":" + Config.EMS_service;
            EMSJefferiesExcuReport.DestinationFeature = DestinationFeature.Topic;
            //EMSJefferiesExcuReport.DestinationFeature = DestinationFeature.Queue;
            EMSJefferiesExcuReport.MessageTimeOut = ((double)1 / 24) * ((double)1 / 60);
            EMSJefferiesExcuReport.SendName = "";
            Profile Profile = DBProfile.GetProfile(MainApp.GlobalVariable.DBFile.FullName);
            EMSJefferiesExcuReport.ListenName = "messageclient." + Profile.ID.Trim();

            EMSJefferiesExcuReport.UserName = DecEncCode.AESDecrypt(Config.EMSUserID);
            EMSJefferiesExcuReport.PassWord = DecEncCode.AESDecrypt(Config.EMSPwd);
            EMSJefferiesExcuReport.UseSSL = Config.EMS_useSSL;
            EMSJefferiesExcuReport.IsEventInUIThread = true;
            (EMSJefferiesExcuReport as BatchEMSAdapter).DataType = typeof(MessageAddressee);
            (EMSJefferiesExcuReport as BatchEMSAdapter).EMSMessageHandleFinished += EMSJefferiesExcuReport_EMSMessageHandleFinished;
            (EMSJefferiesExcuReport as BatchEMSAdapter).EMSBatchFinished += EMSJefferiesExcuReport_EMSBatchFinished;
        }

        //服務被啟動時的事件處理
        public override void OnStart(Intent intent, int startId)
        {
            base.OnStart(intent, startId);
            try
            {
                ExecuteMulticastLock("AppMulticast");
                EMSJefferiesExcuReport.Start(Android.OS.Build.Serial, true);
                Common.LogHelper.MoneySQLogger.LogInfo<EMSService>(string.Format("EMS訊息推播服務連結({0})成功", EMSJefferiesExcuReport.Uri));
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "EMS訊息推播服務無法連結,系統將終止({0})", ex.Message);
                Common.LogHelper.MoneySQLogger.LogError<EMSService>(ex);
                ErrorInMainApp = string.Format("EMS訊息推播服務無法連結,系統將終止({0})", ex.Message);
            }
            finally
            {
                IsFinishedEmsService = true;
            }
        }

        //停止服務時的事件處理
        public override void OnDestroy()
        {
            try
            {
                (EMSJefferiesExcuReport as BatchEMSAdapter).EMSMessageHandleFinished -= EMSJefferiesExcuReport_EMSMessageHandleFinished;
                (EMSJefferiesExcuReport as BatchEMSAdapter).EMSBatchFinished -= EMSJefferiesExcuReport_EMSBatchFinished;
                EMSJefferiesExcuReport.RemoveAllEvents();
                EMSJefferiesExcuReport.Close();
                EMSJefferiesExcuReport.CloseSharedConnection();
                ExecuteMulticastRelease("AppMulticast");
                GC.Collect(0);
                base.OnDestroy();
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "EMSService OnDestroy() Error({0})", ex.Message);
                Common.LogHelper.MoneySQLogger.LogError<EMSService>(ex);
            }
            finally
            {
                EMSJefferiesExcuReport = null;
                EMSJefferiesExcuReportPendingIntent = null;
                ErrorInMainApp = null;
            }
        }
        /// <summary> 
        /// By overriding this, we're specifying that the service is a _Started Service_, and therefore, we're 
        /// supposed to manage it's lifecycle (shutting it down, for instance). 
        /// </summary> 
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Android.Util.Log.Debug(MethodBase.GetCurrentMethod().DeclaringType.ToString(), string.Format("{0} Started", this.GetType().Name));
            // let our users know that this service is running, because it'll be a foregrounded service
            // this isn't usually needed unless you're doing something like a music player app, because
            // services hardly every get recycle from memory pressure, especially sticky ones.
            var ongoingNotification = new Notification(Resource.Drawable.moneysq, string.Format("{0} Running Foreground", this.GetType().Name));
            ongoingNotification.Defaults = NotificationDefaults.Sound;
            ongoingNotification.Flags = NotificationFlags.ForegroundService;
            // the pending intent specifies the activity to launch when a user clicks on the notification
            // in this case, we want to take the user to the music player  
            Intent MainActivityIntent = new Intent(this, typeof(MainActivity));
            MainActivityIntent.SetFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainActivity)).SetFlags(ActivityFlags.ClearTop), PendingIntentFlags.UpdateCurrent);
            ongoingNotification.SetLatestEventInfo(this, MethodBase.GetCurrentMethod().DeclaringType.ToString(), string.Format("{0} is now runing as a messaging service", this.GetType().Name), pendingIntent);
            // start our service foregrounded, that way it won't get cleaned up from memory pressure
            StartForeground((int)NotificationFlags.ForegroundService, ongoingNotification);
            // tell the OS that if this service ever gets killed, to redilever the intent when it's started 
            base.OnStartCommand(intent, flags, startId);

            return StartCommandResult.Sticky;
        }
        void EMSJefferiesExcuReport_EMSMessageHandleFinished(object sender, EMSMessageHandleFinishedEventArgs e)
        {
            if (e.errorMessage != "")
            {
                Common.LogHelper.MoneySQLogger.LogInfo<EMSService>(e.errorMessage);
                Toast.MakeText(this, e.errorMessage, ToastLength.Long).Show();
            }
        }

        void EMSJefferiesExcuReport_EMSBatchFinished(object sender, EMSBatchFinishedEventArgs e)
        {
            //接收檔案
            if (e.BatchResultTable.TableName.Equals("file"))
            {
                DataTable MessageDT = e.BatchResultTable;
                if (MessageDT.Rows.Count > 0)
                {
                    DataRow dr = MessageDT.Rows[0];
                    string pushID = dr["id"].ToString();
                    string fileName = dr["filename"].ToString();
                    bool result = MessageManager.CreateMessageFile(pushID, fileName, (dr["content"] as byte[]));
                    //更新Table MessageAddressee欄位Attachments
                    MessageAddressee MA = DBMessageAddressee.GetMessageByPushID(pushID, MainApp.GlobalVariable.DBFile.FullName);
                    MA.Attachments += fileName + ";";
                    MainApp.GlobalVariable.MessageID = pushID;
                    MainApp.GlobalVariable.Attachments = MA.Attachments;
                    result = DBMessageAddressee.UpdateAttachments(MA, MainApp.GlobalVariable.DBFile.FullName);
                    MainApp.GlobalVariable.DBMessages = DBMessageAddressee.GetDBAllMessage(MainApp.GlobalVariable.DBFile.FullName);
                    IsNewMessageData = true;
                }
            }
            //接收訊息
            else
            {
                //When EMSJefferiesExcuReportNotification arrive,make screen bright on
                PowerManager powerManager = (PowerManager)GetSystemService(Context.PowerService);
                PowerManager.WakeLock wakeLock = powerManager.NewWakeLock((WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup), "WakeDevice");
                wakeLock.Acquire();
                //宣告EMSJefferiesExcuReportPendingIntent物件，讓Notification知道當使用點選通知時，要返回哪一個Activity或Service
                List<MessageAddressee> messages = Common.Utility.Util.DataTableToList<MessageAddressee>(e.BatchResultTable);
                //string sJson = JsonConvert.SerializeObject(messages, Formatting.None,
                //            new JsonSerializerSettings()
                //            {
                //                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                //            });
                EMSJefferiesExcuReportPendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainActivity)).PutExtra("EMSJefferiesExcuReportMessage", "").SetFlags(ActivityFlags.ClearTop), PendingIntentFlags.UpdateCurrent);
                EMSJefferiesExcuReportNotification.Number += 1;
                EMSJefferiesExcuReportNotification.When = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                //透過Notification的SetLatestEventInfo方法設定Notification
                EMSJefferiesExcuReportNotification.SetLatestEventInfo(this, "MoneySQ推播訊息通知", "親愛的客戶，您好!目前系統收到一筆來自MoneySQ的推播通知，詳細訊息請點擊此處瀏覽", EMSJefferiesExcuReportPendingIntent);
                //呼叫NotificationManager的Notify方法發送通知
                nMgr.Notify(0, EMSJefferiesExcuReportNotification);

                string MessageID = messages[0].PushMessageID;
                string SendedMessageTime = messages[0].SendedMessageTime;
                string Subject = messages[0].Subject;
                DateTime ReceivedMessageTime = DateTime.Now;
                MainApp.GlobalVariable.SendedMessageTime = SendedMessageTime;
                MainApp.GlobalVariable.ReceivedMessageTime = ReceivedMessageTime.ToString("yyyy/MM/dd HH:mm:ss");
                MainApp.GlobalVariable.Subject = Subject;
                MainApp.GlobalVariable.MessageText = messages[0].Message + "\n";

                //Insert Push Message to DB
                MessageAddressee MessageAddressee = new MessageAddressee();
                MessageAddressee.PushMessageID = MessageID;
                MessageAddressee.application_no = messages[0].application_no;
                MessageAddressee.contract_number = messages[0].contract_number;
                MessageAddressee.Addressee = "";
                MessageAddressee.Subject = MainApp.GlobalVariable.Subject;
                MessageAddressee.Message = messages[0].Message;
                MessageAddressee.SendedMessageTime = SendedMessageTime;
                MessageAddressee.ReceivedMessageTime = ReceivedMessageTime.ToString("yyyy/MM/dd HH:mm:ss");
                MessageAddressee.CreatedDate = ReceivedMessageTime.ToString("yyyy/MM/dd HH:mm:ss");
                DBMessageAddressee.InsertPushMessageToDB(MessageAddressee, MainApp.GlobalVariable.DBFile.FullName);

                //delete history Message Files
                List<MessageAddressee> HistroyMessages = DBMessageAddressee.GetDeleteHistroyMessages(Config.preservedRowsForMessge, MainApp.GlobalVariable.DBFile.FullName);
                bool result = MessageManager.DeleteMessageFiles(HistroyMessages);

                //delete history Messages
                DBMessageAddressee.DeleteHistroyMessages(Config.preservedRowsForMessge, MainApp.GlobalVariable.DBFile.FullName);

                MainApp.GlobalVariable.DBMessages = DBMessageAddressee.GetDBAllMessage(MainApp.GlobalVariable.DBFile.FullName);
                IsNewMessageData = true;

                //Toast.MakeText(this, "EMSJefferiesExcuReport Server 處理完成", ToastLength.Long).Show();
                Toast.MakeText(this, "親愛的客戶，您好!目前系統收到一筆來自MoneySQ的推播通知，詳細訊息請瀏覽訊息公告", ToastLength.Long).Show();
                //var v = (Vibrator)Android.App.Application.Context.GetSystemService(Android.App.Application.VibratorService);
                //v.Vibrate(250);
                wakeLock.Release();
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
            //throw new NotImplementedException();
        }

        private void ExecuteMulticastLock(string LockName)
        {
            try
            {
                if (WifiManager != null)
                {
                    WifiManager.MulticastLock MulticastLock = WifiManager.CreateMulticastLock(LockName);
                    MulticastLock.Acquire();
                }
            }
            catch (Exception ex) { }
        }
        private void ExecuteMulticastRelease(string LockName)
        {
            try
            {
                if (WifiManager != null)
                {
                    WifiManager.MulticastLock MulticastLock = WifiManager.CreateMulticastLock(LockName);
                    MulticastLock.Release();
                }
            }
            catch (Exception ex) { }
        }
    }
}