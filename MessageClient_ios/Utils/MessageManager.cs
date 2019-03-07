using DBModels;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MessageClient_ios.Util
{
    public class MessageManager
    {
        public static bool IsEmail(string email)
        {
            return Regex.IsMatch(email, @"^([\w-]+\.)*?[\w-]+@[\w-]+\.([\w-]+\.)*?[\w]+$");
        }
        public static bool CreateMessageFile(string pushID, string fileName, byte[] fileBytes)
        {
            bool result = false;
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string PackageName = NSBundle.MainBundle.BundleIdentifier;
            var PackageFolderPath = Path.Combine(documentsPath, PackageName);
            try
            {
                DirectoryInfo FolderInfo = new DirectoryInfo(PackageFolderPath);
                if (!FolderInfo.Exists)
                {
                    FolderInfo.Create();
                }
                FolderInfo = new DirectoryInfo(PackageFolderPath + @"/" + pushID);
                if (!FolderInfo.Exists)
                {
                    FolderInfo.Create();
                }
                File.Delete(FolderInfo.FullName + @"/" + fileName);
                File.WriteAllBytes(FolderInfo.FullName + @"/" + fileName, fileBytes);
                result = true;
            }
            catch (Exception ex)
            {
                //Logger.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), string.Format("MessageManager CreateMessageFile() Error({0})", ex.Message), Java.Lang.Throwable.FromException(ex));
                Common.LogHelper.MoneySQLogger.LogError<MessageManager>(ex);
            }
            return result;
        }
        public static bool DeleteMessageFiles(List<MessageAddressee> HisMessages)
        {
            bool result = false;
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string PackageName = NSBundle.MainBundle.BundleIdentifier;
            var PackageFolderPath = Path.Combine(documentsPath, PackageName);
            try
            {
                foreach (MessageAddressee HisMessage in HisMessages)
                {
                    string pushID = HisMessage.PushMessageID;
                    DirectoryInfo pushIDFolderInfo = new DirectoryInfo(PackageFolderPath + @"/" + pushID);
                    if (pushIDFolderInfo.Exists)
                    {
                        pushIDFolderInfo.Delete(true);
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                //Logger.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), string.Format("MessageManager DeleteMessageFiles() Error({0})", ex.Message), Java.Lang.Throwable.FromException(ex));
                Common.LogHelper.MoneySQLogger.LogError<MessageManager>(ex);
            }
            return result;
        }
    }
}