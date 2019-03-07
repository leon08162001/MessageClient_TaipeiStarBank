using Android.App;
using Android.OS;
using DBModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MessageClient.Utils
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
            string FolderPath = "";
            string PackageName = Application.Context.PackageName;
            FolderPath = Application.Context.GetExternalFilesDir("").AbsolutePath;
            try
            {
                DirectoryInfo FolderInfo = new DirectoryInfo(FolderPath + @"/Attachment" + @"/" + pushID);
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
                Common.LogHelper.MoneySQLogger.LogError<MessageManager>(ex);
            }
            return result;
        }
        public static bool DeleteMessageFiles(List<MessageAddressee> HisMessages)
        {
            bool result = false;
            string FolderPath = "";
            string PackageName = Application.Context.PackageName;
            FolderPath = Application.Context.GetExternalFilesDir("").AbsolutePath;
            try
            {
                foreach(MessageAddressee HisMessage in HisMessages)
                {
                    string pushID = HisMessage.PushMessageID;
                    DirectoryInfo pushIDFolderInfo = new DirectoryInfo(FolderPath + @"/Attachment" + @"/" + pushID);
                    if(pushIDFolderInfo.Exists)
                    {
                        pushIDFolderInfo.Delete(true);
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<MessageManager>(ex);
            }
            return result;
        }
    }
}