using CoreGraphics;
using DBModels;
using System.Collections.Generic;
using System.IO;
using UIKit;

namespace MessageClient_ios
{
    public class GlobalVariable
    {
        public FileInfo DBFile = new FileInfo(Path.Combine(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "..", "Library"), "MoneySQ.db"));
        public string MessageID = "";
        public string ReceivedMessageTime = "";
        public string SendedMessageTime = "";
        public string Subject = "";
        public string MessageText = "";
        public string Attachments = "";
        public List<MessageAddressee> DBMessages;
        public int CurrentTabIndex = 0;
        public string filepath = "";
    }
}