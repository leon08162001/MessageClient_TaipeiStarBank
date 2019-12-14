using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Widget;
using Common;
using Common.LinkLayer;
using DBLogic;
using DBModels;
using MessageClient.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Utility;

namespace MessageClient.Services
{
    [Service]
    public class MQService : Service
    {
        public static bool IsNewMessageData = false;
        public static IMQAdapter MQJefferiesExcuReport;
        public static PendingIntent MQJefferiesExcuReportPendingIntent;
        public static bool IsFinishedMqService = false;
        public static string ErrorInMainApp = "";
        private NotificationManager nMgr;
        private Notification MQJefferiesExcuReportNotification;
        private WifiManager WifiManager;

        //服務被建立的事件處理
        public override void OnCreate()
        {
            base.OnCreate();
            WifiManager = GetSystemService(Context.WifiService) as WifiManager;

            //取得系統的通知管理員
            nMgr = (NotificationManager)GetSystemService(NotificationService);

            //宣告MQJefferiesExcuReportNotification物件
            MQJefferiesExcuReportNotification = new Notification(Resource.Drawable.message, "MQJefferiesExcuReport推播訊息通知");
            MQJefferiesExcuReportNotification.Defaults = NotificationDefaults.Sound;
            MQJefferiesExcuReportNotification.Flags = NotificationFlags.AutoCancel;

            MQJefferiesExcuReport = TopicMQFactory.GetMQAdapterInstance(MQAdapterType.BatchMQAdapter);
            MQJefferiesExcuReport.Uri = Config.MQ_network + ":" + Config.MQ_service;
            MQJefferiesExcuReport.DestinationFeature = DestinationFeature.Queue;
            //MQJefferiesExcuReport.DestinationFeature = DestinationFeature.Queue;
            MQJefferiesExcuReport.MessageTimeOut = ((double)1 / 24) * ((double)1 / 60);
            MQJefferiesExcuReport.SendName = "";
            Profile Profile = DBProfile.GetProfile(MainApp.GlobalVariable.DBFile.FullName);
            MQJefferiesExcuReport.ListenName = Profile.ID.Trim();

            MQJefferiesExcuReport.UserName = DecEncCode.AESDecrypt(Config.MQUserID);
            MQJefferiesExcuReport.PassWord = DecEncCode.AESDecrypt(Config.MQPwd);
            MQJefferiesExcuReport.UseSSL = Config.MQ_useSSL;
            MQJefferiesExcuReport.IsEventInUIThread = true;
            (MQJefferiesExcuReport as BatchMQAdapter).DataType = typeof(MessageAddressee);
            (MQJefferiesExcuReport as BatchMQAdapter).MQMessageHandleFinished += MQJefferiesExcuReport_MQMessageHandleFinished;
            (MQJefferiesExcuReport as BatchMQAdapter).MQBatchFinished += MQJefferiesExcuReport_MQBatchFinished;
        }

        //服務被啟動時的事件處理
        public override void OnStart(Intent intent, int startId)
        {
            base.OnStart(intent, startId);
            try
            {
                ExecuteMulticastLock("AppMulticast");
                MQJefferiesExcuReport.Start(Android.OS.Build.Serial);
                Common.LogHelper.MoneySQLogger.LogInfo<MQService>(string.Format("MQ訊息推播服務連結({0})成功", MQJefferiesExcuReport.Uri));
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "MQ訊息推播服務無法連結,系統將終止({0})", ex.Message);
                Common.LogHelper.MoneySQLogger.LogError<MQService>(ex);
                ErrorInMainApp = string.Format("MQ訊息推播服務無法連結,系統將終止({0})", ex.Message);
            }
            finally
            {
                IsFinishedMqService = true;
            }
        }

        //停止服務時的事件處理
        public override void OnDestroy()
        {
            try
            {
                (MQJefferiesExcuReport as BatchMQAdapter).MQMessageHandleFinished -= MQJefferiesExcuReport_MQMessageHandleFinished;
                (MQJefferiesExcuReport as BatchMQAdapter).MQBatchFinished -= MQJefferiesExcuReport_MQBatchFinished;
                MQJefferiesExcuReport.RemoveAllEvents();
                MQJefferiesExcuReport.Close();
                MQJefferiesExcuReport.CloseSharedConnection();
                ExecuteMulticastRelease("AppMulticast");
                GC.Collect(0);
                base.OnDestroy();
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "MQService OnDestroy() Error({0})", ex.Message);
                Common.LogHelper.MoneySQLogger.LogError<MQService>(ex);
            }
            finally
            {
                MQJefferiesExcuReport = null;
                MQJefferiesExcuReportPendingIntent = null;
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
        void MQJefferiesExcuReport_MQMessageHandleFinished(object sender, MQMessageHandleFinishedEventArgs e)
        {
            if (e.errorMessage != "")
            {
                Common.LogHelper.MoneySQLogger.LogInfo<MQService>(e.errorMessage);
                Toast.MakeText(this, e.errorMessage, ToastLength.Long).Show();
            }
        }

        void MQJefferiesExcuReport_MQBatchFinished(object sender, MQBatchFinishedEventArgs e)
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
                //When MQJefferiesExcuReportNotification arrive,make screen bright on
                PowerManager powerManager = (PowerManager)GetSystemService(Context.PowerService);
                PowerManager.WakeLock wakeLock = powerManager.NewWakeLock((WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup), "WakeDevice");
                wakeLock.Acquire();
                //宣告MQJefferiesExcuReportPendingIntent物件，讓Notification知道當使用點選通知時，要返回哪一個Activity或Service
                List<MessageAddressee> messages = Common.Utility.Util.DataTableToList<MessageAddressee>(e.BatchResultTable);
                //string sJson = JsonConvert.SerializeObject(messages, Formatting.None,
                //            new JsonSerializerSettings()
                //            {
                //                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                //            });
                MQJefferiesExcuReportPendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainActivity)).PutExtra("MQJefferiesExcuReportMessage", "").SetFlags(ActivityFlags.ClearTop), PendingIntentFlags.UpdateCurrent);
                MQJefferiesExcuReportNotification.Number += 1;
                MQJefferiesExcuReportNotification.When = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                //透過Notification的SetLatestEventInfo方法設定Notification
                MQJefferiesExcuReportNotification.SetLatestEventInfo(this, "MoneySQ推播訊息通知", "親愛的客戶，您好!目前系統收到一筆來自MoneySQ的推播通知，詳細訊息請點擊此處瀏覽", MQJefferiesExcuReportPendingIntent);
                //呼叫NotificationManager的Notify方法發送通知
                nMgr.Notify(0, MQJefferiesExcuReportNotification);

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

                //Toast.MakeText(this, "MQJefferiesExcuReport Server 處理完成", ToastLength.Long).Show();

                Toast.MakeText(this, "親愛的客戶，您好!目前系統收到一筆來自MoneySQ的推播通知，詳細訊息請瀏覽訊息公告", ToastLength.Long).Show();
                //SnackbarWrapper.make(Android.App.Application.Context, "親愛的客戶，您好!目前系統收到一筆來自MoneySQ的推播通知，詳細訊息請瀏覽訊息公告", 3000).Show();

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