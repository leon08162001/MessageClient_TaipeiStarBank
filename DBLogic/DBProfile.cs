using DataAccess;
using DBModels;
using System;
using System.Collections.Generic;

namespace DBLogic
{
    public class DBProfile
    {
        public static Profile GetProfile(string DbFilePath)
        {
            Profile Profile = null;
            try
            {
                using (DBContext db = new DBContext(dbtype.Sqlite, DbFilePath))
                {
                    NewEntityRepository<Profile> tbl = new NewEntityRepository<Profile>(db);
                    List<Profile> ProfileSet = tbl.GetTop(1);
                    if (ProfileSet.Count > 0) Profile = ProfileSet[0];
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<DBProfile>(ex);
            }
            return Profile;
        }
        public static bool InsertProfile(Profile Profile, string DbFilePath)
        {
            bool result = false;
            try
            {
                using (DBContext db = new DBContext(dbtype.Sqlite, DbFilePath))
                {
                    NewEntityRepository<Profile> tbl = new NewEntityRepository<Profile>(db);
                    result = tbl.Add(Profile);
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<DBProfile>(ex);
            }
            return result;
        }
        public static bool ClearProfile(string DbFilePath)
        {
            bool result = false;
            try
            {
                using (DBContext db = new DBContext(dbtype.Sqlite, DbFilePath))
                {
                    NewEntityRepository<Profile> tbl = new NewEntityRepository<Profile>(db);
                    List<Profile> Profiles = tbl.GetAll();
                    result = tbl.Delete(Profiles);
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<DBProfile>(ex);
            }
            return result;
        }

        public static bool CheckProfileExist(string DbFilePath)
        {
            bool result = false;
            try
            {
                using (DBContext db = new DBContext(dbtype.Sqlite, DbFilePath))
                {
                    NewEntityRepository<Profile> tbl = new NewEntityRepository<Profile>(db);
                    result = tbl.GetCount() > 0;
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<DBProfile>(ex);
            }
            return result;
        }
        public static long GetUserType(string ID, string DbFilePath)
        {
            long UserType = 0;
            try
            {
                using (DBContext db = new DBContext(dbtype.Sqlite, DbFilePath))
                {
                    NewEntityRepository<Profile> tbl = new NewEntityRepository<Profile>(db);
                    List<Profile> Profile = tbl.Find(x => x.ID == ID);
                    if (Profile.Count > 0)
                    {
                        UserType = Profile[0].UserType;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<DBProfile>(ex);
            }
            return UserType;
        }
    }
}
