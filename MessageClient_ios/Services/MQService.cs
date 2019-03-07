using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Common.LinkLayer;
using DBModels;
using Common;
using DBLogic;
using Utility;
using System.Data;
using MessageClient_ios.Util;
using System.IO;

namespace MessageClient_ios.Services
{
    public class MQService
    {
        public static IMQAdapter MQJefferiesExcuReport;
        public static bool IsNewMessageData = false;
        public static bool IsFinishedMqService = false;
        public static string ErrorInMainApp = "";
        public static int UnReadNotificationCount = 0;

        /// <summary>
        /// 起始MQ Service
        /// </summary>
        public static void StartService()
        {
            MQJefferiesExcuReport = TopicMQFactory.GetMQAdapterInstance(MQAdapterType.BatchMQAdapter);
            MQJefferiesExcuReport.Uri = ConfigIOS.ApolloMQ_network + ":" + ConfigIOS.ApolloMQ_service;
            MQJefferiesExcuReport.DestinationFeature = DestinationFeature.VirtualTopic;
            MQJefferiesExcuReport.MessageTimeOut = ((double)1 / 24) * ((double)1 / 60);
            MQJefferiesExcuReport.SendName = "";
            Profile Profile = DBProfile.GetProfile(AppDelegate.GlobalVariable.DBFile.FullName);
            //Test Code
            MQJefferiesExcuReport.ListenName = Profile == null ? "" : Profile.ID;
            MQJefferiesExcuReport.UserName = DecEncCode.AESDecrypt(ConfigIOS.ApolloMQUserID);
            MQJefferiesExcuReport.PassWord = DecEncCode.AESDecrypt(ConfigIOS.ApolloMQPwd);
            MQJefferiesExcuReport.IsEventInUIThread = true;
            (MQJefferiesExcuReport as BatchMQAdapter).DataType = typeof(MessageAddressee);
            (MQJefferiesExcuReport as BatchMQAdapter).MQMessageHandleFinished += MQService_MQMessageHandleFinished;
            (MQJefferiesExcuReport as BatchMQAdapter).MQBatchFinished += MQService_MQBatchFinished;
            try
            {
                MQJefferiesExcuReport.Start();
            }
            catch (Exception ex)
            {
                MQService.ErrorInMainApp = "MQ訊息推播服務無法連結,系統將終止";
                Common.LogHelper.MoneySQLogger.LogError<AppDelegate>(ex, "MQ訊息推播服務無法連結,系統將終止");
            }
        }

        private static void MQService_MQBatchFinished(object sender, MQBatchFinishedEventArgs e)
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
                    if (result)
                    {
                        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string PackageName = NSBundle.MainBundle.BundleIdentifier;
                        var PackageFolderPath = Path.Combine(documentsPath, PackageName);
                        DirectoryInfo FolderInfo = new DirectoryInfo(PackageFolderPath + @"/" + pushID);
                        FileInfo FI = FolderInfo.GetFiles(fileName)[0];
                        long fileLength = FI.Length;
                    }
                    //更新Table MessageAddressee欄位Attachments
                    try
                    {
                        MessageAddressee MA = DBMessageAddressee.GetMessageByPushID(pushID, AppDelegate.GlobalVariable.DBFile.FullName);
                        MA.Attachments += fileName + ";";
                        AppDelegate.GlobalVariable.MessageID = pushID;
                        AppDelegate.GlobalVariable.Attachments = MA.Attachments;
                        result = DBMessageAddressee.UpdateAttachments(MA, AppDelegate.GlobalVariable.DBFile.FullName);
                        AppDelegate.GlobalVariable.DBMessages = DBMessageAddressee.GetDBAllMessage(AppDelegate.GlobalVariable.DBFile.FullName);
                        IsNewMessageData = true;

                        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string PackageName = NSBundle.MainBundle.BundleIdentifier;
                        var PackageFolderPath = Path.Combine(documentsPath, PackageName);
                        var filepath = PackageFolderPath + @"/" + pushID + @"/" + fileName;
                        AppDelegate.GlobalVariable.filepath = filepath;
                    }
                    catch (Exception ex)
                    {
                        Common.LogHelper.MoneySQLogger.LogError<AppDelegate>(ex);
                    }
                }
            }
            //接收訊息
            else
            {
                try
                {
                    List<MessageAddressee> messages = Common.Utility.Util.DataTableToList<MessageAddressee>(e.BatchResultTable);
                    string MessageID = messages[0].PushMessageID;
                    string SendedMessageTime = messages[0].SendedMessageTime;
                    string Subject = messages[0].Subject;
                    DateTime ReceivedMessageTime = DateTime.Now;
                    AppDelegate.GlobalVariable.SendedMessageTime = SendedMessageTime;
                    AppDelegate.GlobalVariable.ReceivedMessageTime = ReceivedMessageTime.ToString("yyyy/MM/dd HH:mm:ss");
                    AppDelegate.GlobalVariable.Subject = Subject;
                    AppDelegate.GlobalVariable.Attachments = "";
                    AppDelegate.GlobalVariable.MessageText = messages[0].Message + "\n";

                    //Insert Push Message to DB
                    MessageAddressee MessageAddressee = new MessageAddressee();
                    MessageAddressee.PushMessageID = MessageID;
                    MessageAddressee.application_no = messages[0].application_no;
                    MessageAddressee.contract_number = messages[0].contract_number;
                    MessageAddressee.Addressee = "";
                    MessageAddressee.Subject = AppDelegate.GlobalVariable.Subject;
                    MessageAddressee.Message = messages[0].Message;
                    MessageAddressee.SendedMessageTime = SendedMessageTime;
                    MessageAddressee.ReceivedMessageTime = ReceivedMessageTime.ToString("yyyy/MM/dd HH:mm:ss");
                    MessageAddressee.CreatedDate = ReceivedMessageTime.ToString("yyyy/MM/dd HH:mm:ss");
                    bool result1 = DBMessageAddressee.InsertPushMessageToDB(MessageAddressee, AppDelegate.GlobalVariable.DBFile.FullName);

                    //delete history Message Files
                    List<MessageAddressee> HistroyMessages = DBMessageAddressee.GetDeleteHistroyMessages(ConfigIOS.preservedRowsForMessge, AppDelegate.GlobalVariable.DBFile.FullName);
                    bool result = MessageManager.DeleteMessageFiles(HistroyMessages);

                    //delete history Messages
                    bool result2 = DBMessageAddressee.DeleteHistroyMessages(ConfigIOS.preservedRowsForMessge, AppDelegate.GlobalVariable.DBFile.FullName);

                    AppDelegate.GlobalVariable.DBMessages = DBMessageAddressee.GetDBAllMessage(AppDelegate.GlobalVariable.DBFile.FullName);
                    IsNewMessageData = true;

                    SetNotification(MessageID);
                }
                catch (Exception ex)
                {
                    Common.LogHelper.MoneySQLogger.LogError<AppDelegate>(ex);
                }
            }
        }

        private static void MQService_MQMessageHandleFinished(object sender, MQMessageHandleFinishedEventArgs e)
        {
            if (e.errorMessage != "")
            {
                //Toast.MakeText(this, e.errorMessage, ToastLength.Long).Show();
            }
        }
        /// <summary>
        /// 建立系統通知
        /// </summary>
        /// <param name="PushMessageID"></param>
        private static void SetNotification(string PushMessageID)
        {
            // create the notification
            UnReadNotificationCount += 1;
            var notification = new UILocalNotification();
            // set the fire date (the date time in which it will fire)
            notification.FireDate = NSDate.FromTimeIntervalSinceNow(0);
            // configure the alert
            notification.AlertTitle = "MoneySQ推播訊息通知";
            notification.AlertBody = "親愛的客戶，您好!目前系統收到一筆來自MoneySQ的推播通知，詳細訊息請點擊此處瀏覽";
            notification.Category = "MessageClient";
            notification.UserInfo = NSDictionary.FromObjectAndKey(new NSString(PushMessageID), new NSString("PushMessageID"));
            // modify the badge
            notification.ApplicationIconBadgeNumber = UnReadNotificationCount;
            // set the sound to be the default sound
            notification.SoundName = UILocalNotification.DefaultSoundName;
            // schedule it
            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
        }
    }
}