using DataAccess;
using DBModels;
using System;
using System.Collections.Generic;

namespace DBLogic
{
    public class DBMessageAddressee
    {
        public static List<MessageAddressee> GetDBTopMessage(int Number, string DbFilePath)
        {
            List<MessageAddressee> Messages = new List<MessageAddressee>();
            try
            {
                using (DBContext db = new DBContext(dbtype.Sqlite, DbFilePath))
                {
                    NewEntityRepository<MessageAddressee> tbl = new NewEntityRepository<MessageAddressee>(db);
                    Messages = tbl.GetTop(Number, "ReceivedMessageTime desc");
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<DBMessageAddressee>(ex);
            }
            return Messages;
        }
        public static List<MessageAddressee> GetDBAllMessage(string DbFilePath)
        {
            List<MessageAddressee> Messages = new List<MessageAddressee>();
            try
            {
                using (DBContext db = new DBContext(dbtype.Sqlite, DbFilePath))
                {
                    NewEntityRepository<MessageAddressee> tbl = new NewEntityRepository<MessageAddressee>(db);
                    Messages = tbl.GetAll("ReceivedMessageTime desc");
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<DBMessageAddressee>(ex);
            }
            return Messages;
        }
        public static MessageAddressee GetMessageByPushID(string PushID, string DbFilePath)
        {
            List<MessageAddressee> Messages = new List<MessageAddressee>();
            MessageAddressee Message = null;
            try
            {
                using (DBContext db = new DBContext(dbtype.Sqlite, DbFilePath))
                {
                    NewEntityRepository<MessageAddressee> tbl = new NewEntityRepository<MessageAddressee>(db);
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    dic.Add("PushMessageID", PushID);
                    Messages = tbl.Find("select * from MessageAddressee where PushMessageID=@PushMessageID", dic);
                    if (Messages.Count > 0)
                    {
                        Message = Messages[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<DBMessageAddressee>(ex);
            }
            return Message;
        }
        public static List<MessageAddressee> GetDeleteHistroyMessages(int preservedRows, string DbFilePath)
        {
            List<MessageAddressee> Messages = new List<MessageAddressee>();
            try
            {
                using (DBContext db = new DBContext(dbtype.Sqlite, DbFilePath))
                {
                    NewEntityRepository<MessageAddressee> tbl = new NewEntityRepository<MessageAddressee>(db);
                    int iAllCount = tbl.GetCount();
                    int iDeleteCount = iAllCount - preservedRows;
                    if (iDeleteCount > 0)
                    {
                        Messages = tbl.GetTop(iDeleteCount, "SendedMessageTime asc");
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<DBMessageAddressee>(ex);
            }
            return Messages;
        }
        public static bool InsertPushMessageToDB(MessageAddressee MessageAddressee, string DbFilePath)
        {
            bool result = false;
            try
            {
                using (DBContext db = new DBContext(dbtype.Sqlite, DbFilePath))
                {
                    NewEntityRepository<MessageAddressee> tbl = new NewEntityRepository<MessageAddressee>(db);
                    result = tbl.Add(MessageAddressee);
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<DBMessageAddressee>(ex);
            }
            return result;
        }
        public static bool UpdateAttachments(MessageAddressee MessageAddressee, string DbFilePath)
        {
            bool result = false;
            try
            {
                using (DBContext db = new DBContext(dbtype.Sqlite, DbFilePath))
                {
                    NewEntityRepository<MessageAddressee> tbl = new NewEntityRepository<MessageAddressee>(db);
                    result = tbl.Update(MessageAddressee);
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<DBMessageAddressee>(ex);
            }
            return result;
        }
        public static bool DeleteHistroyMessages(int preservedRows, string DbFilePath)
        {
            bool result = false;
            try
            {
                using (DBContext db = new DBContext(dbtype.Sqlite, DbFilePath))
                {
                    NewEntityRepository<MessageAddressee> tbl = new NewEntityRepository<MessageAddressee>(db);
                    int iAllCount = tbl.GetCount();
                    int iDeleteCount = iAllCount - preservedRows;
                    if (iDeleteCount > 0)
                    {
                        List<MessageAddressee> Messages = tbl.GetTop(iDeleteCount, "SendedMessageTime asc");
                        result = tbl.Delete(Messages);
                    }
                    else
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<DBMessageAddressee>(ex);
            }
            return result;
        }
    }
}
