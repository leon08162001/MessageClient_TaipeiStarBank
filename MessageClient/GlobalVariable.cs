using DBModels;
using System.Collections.Generic;
using System.IO;

namespace MessageClient
{
    public class GlobalVariable
    {
        public FileInfo DBFile = new FileInfo(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "MoneySQ.db"));
        public string MessageID = "";
        public string ReceivedMessageTime = "";
        public string SendedMessageTime = "";
        public string Subject = "";
        public string MessageText = "";
        public string Attachments = "";
        public List<MessageAddressee> DBMessages;
    }
}