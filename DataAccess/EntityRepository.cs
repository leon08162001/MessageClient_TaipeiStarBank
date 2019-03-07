using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;

namespace DataAccess
{
    /// <summary>
    /// 實體化資料庫物件模型存取類別
    /// </summary>
    public class EntityRepository<T> where T : class, new()
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected readonly DbContext _DbContext;
        protected readonly DbSet<T> _dbset;

        public EntityRepository(DbContext DbContext)
        {
            _DbContext = DbContext;
            _dbset = _DbContext.Set<T>();
        }
        /// <summary>
        /// 取得某資料表物件模型的資料筆數
        /// </summary>
        /// <returns></returns>
        public virtual int GetCount()
        {
            return _dbset.AsQueryable().Count();
        }
        /// <summary>
        /// 取得符合物件模型篩選條件的資料筆數(以指定語述詞方式)
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual int GetCount(Expression<Func<T, bool>> predicate)
        {
            return _dbset.Where(predicate).Count();
        }
        /// <summary>
        /// 取得某資料表物件模型所有資料並排序(排序為選擇性參數)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual List<T> GetAll(params Expression<Func<T, IComparable>>[] SortKeys)
        {
            List<T> query = _dbset.AsEnumerable<T>().ToList();
            if (SortKeys != null && SortKeys.Any())
            {
                query.SortBy<T>(SortKeys);
            }
            return query;
        }
        /// <summary>
        /// 取得某資料表物件模型所有資料並排序(排序為選擇性參數)
        /// </summary>
        /// <param name="strSort"></param>
        /// <returns></returns>
        public virtual List<T> GetAll(string strSort = null)
        {
            List<T> query;
            if (strSort != null && strSort.Any())
            {
                query = _dbset.AsQueryable().OrderBy(strSort).ToList();
            }
            else
            {
                query = _dbset.AsQueryable().ToList();
            }
            return query;
        }
        /// <summary>
        /// 取得某資料表物件模型前N筆資料並排序(排序為選擇性參數)
        /// </summary>
        /// <param name="Number"></param>
        /// <param name="strSort"></param>
        /// <returns></returns>
        public virtual List<T> GetTop(int Number, string strSort = null)
        {
            List<T> query;
            if (strSort != null && strSort.Any())
            {
                query = _dbset.AsQueryable().OrderBy(strSort).Take(Number).ToList();
            }
            else
            {
                query = _dbset.AsQueryable().Take(Number).ToList();
            }
            return query;
        }
        /// <summary>
        /// 以指定陳述詞方式尋找符合物件模型篩選條件的資料並排序(排序為選擇性參數)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual List<T> Find(Expression<Func<T, bool>> WherePredicate, params Expression<Func<T, IComparable>>[] SortKeys)
        {
            List<T> query = _dbset.Where(WherePredicate).AsEnumerable<T>().ToList();
            if (SortKeys != null && SortKeys.Any())
            {
                query.SortBy<T>(SortKeys);
            }
            return query;
        }
        /// <summary>
        /// 以指定陳述詞方式尋找符合物件模型篩選條件的資料並排序(排序為選擇性參數)
        /// </summary>
        /// <param name="WherePredicate"></param>
        /// <param name="strSort"></param>
        /// <returns></returns>
        public virtual List<T> Find(Expression<Func<T, bool>> WherePredicate, string strSort = null)
        {
            List<T> query;
            if (strSort != null && strSort.Any())
            {
                query = _dbset.Where(WherePredicate).AsQueryable().OrderBy(strSort).ToList();
            }
            else
            {
                query = _dbset.Where(WherePredicate).AsEnumerable<T>().ToList();
            }
            return query;
        }
        /// <summary>
        /// 以設定物件模型欄位值方式尋找符合物件模型篩選條件的資料並排序(排序為選擇性參數)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TEntitySet"></param>
        /// <returns></returns>
        public virtual List<T> Find(List<T> TEntitySet, params Expression<Func<T, IComparable>>[] SortKeys)
        {
            List<T> query = new List<T>();
            if (TEntitySet.Count() > 0)
            {
                T Entity = TEntitySet[0];
                List<SqlParameter> SqlParameters = new List<SqlParameter>();
                List<SqliteParameter> SqliteParameters = new List<SqliteParameter>();
                try
                {
                    //ObjectContext objectContext = ((IObjectContextAdapter)_DbContext).ObjectContext;
                    ObjectContext objectContext = new ObjectContext(_DbContext.Database.GetDbConnection().ConnectionString);
                    ObjectSet<T> set = objectContext.CreateObjectSet<T>();
                    List<EdmMember> members = set.EntitySet.ElementType.Members.ToList();
                    foreach (EdmMember member in members)
                    {
                        if (_DbContext.Entry(Entity).Property(member.Name).CurrentValue != null && (!_DbContext.Entry(Entity).Property(member.Name).CurrentValue.ToString().Equals("") && !_DbContext.Entry(Entity).Property(member.Name).CurrentValue.ToString().Equals("0")))
                        {
                            if (_DbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                            {
                                SqliteParameters.Add(new SqliteParameter(member.Name, _DbContext.Entry(Entity).Property(member.Name).CurrentValue));
                            }
                            else
                            {
                                SqlParameters.Add(new SqlParameter(member.Name, _DbContext.Entry(Entity).Property(member.Name).CurrentValue));
                            }
                        }
                    }
                    if (_DbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                    {
                        query = _DbContext.Set<T>().FromSql(GenerateSQL(Entity), SqliteParameters.ToArray()).ToList();
                    }
                    else
                    {
                        query = _DbContext.Set<T>().FromSql(GenerateSQL(Entity), SqlParameters.ToArray()).ToList();
                    }
                    if (SortKeys != null && SortKeys.Any())
                    {
                        query.SortBy<T>(SortKeys);
                    }
                }
                catch (Exception ex)
                {
                    Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
                }
            }
            return query;
        }
        /// <summary>
        /// 以設定物件模型欄位值方式尋找符合物件模型篩選條件的資料並排序(排序為選擇性參數)
        /// </summary>
        /// <param name="TEntitySet"></param>
        /// <param name="strSort"></param>
        /// <returns></returns>
        public virtual List<T> Find(List<T> TEntitySet, string strSort = null)
        {
            List<T> query = new List<T>();
            if (TEntitySet.Count() > 0)
            {
                T Entity = TEntitySet[0];
                List<SqlParameter> SqlParameters = new List<SqlParameter>();
                List<SqliteParameter> SqliteParameters = new List<SqliteParameter>();
                try
                {
                    ObjectContext objectContext = new ObjectContext(_DbContext.Database.GetDbConnection().ConnectionString);
                    ObjectSet<T> set = objectContext.CreateObjectSet<T>();
                    List<EdmMember> members = set.EntitySet.ElementType.Members.ToList();
                    foreach (EdmMember member in members)
                    {
                        if (_DbContext.Entry(Entity).Property(member.Name).CurrentValue != null && (!_DbContext.Entry(Entity).Property(member.Name).CurrentValue.ToString().Equals("") && !_DbContext.Entry(Entity).Property(member.Name).CurrentValue.ToString().Equals("0")))
                        {
                            if (_DbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                            {
                                SqliteParameters.Add(new SqliteParameter(member.Name, _DbContext.Entry(Entity).Property(member.Name).CurrentValue));
                            }
                            else
                            {
                                SqlParameters.Add(new SqlParameter(member.Name, _DbContext.Entry(Entity).Property(member.Name).CurrentValue));
                            }
                        }
                    }
                    if (_DbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                    {
                        query = _DbContext.Set<T>().FromSql(GenerateSQL(Entity), SqliteParameters.ToArray()).ToList();
                    }
                    else
                    {
                        query = _DbContext.Set<T>().FromSql(GenerateSQL(Entity), SqlParameters.ToArray()).ToList();
                    }
                    if (strSort != null && strSort.Any())
                    {
                        if (_DbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                        {
                            query = _DbContext.Set<T>().FromSql(GenerateSQL(Entity), SqliteParameters.ToArray()).AsQueryable().OrderBy(strSort).ToList();
                        }
                        else
                        {
                            query = _DbContext.Set<T>().FromSql(GenerateSQL(Entity), SqlParameters.ToArray()).AsQueryable().OrderBy(strSort).ToList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
                }
            }
            return query;
        }
        /// <summary>
        /// 下一段Select SQL指令尋找符合物件模型篩選條件的資料(以帶SQL含參數指令方式)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Sql"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public virtual List<T> Find(string Sql, Dictionary<string, object> Parameters)
        {
            List<T> query = new List<T>();
            //using (var cmd = _DbContext.Database.Connection.CreateCommand())
            using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
            {
                try
                {
                    if (CheckQueryStatement(new string[] { Sql }))
                    {
                        cmd.CommandText = Sql;
                        foreach (KeyValuePair<string, object> param in Parameters)
                        {
                            DbParameter dbParameter = cmd.CreateParameter();
                            dbParameter.ParameterName = param.Key;
                            dbParameter.Value = param.Value;
                            cmd.Parameters.Add(dbParameter);
                        }
                        if (_DbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                        {
                            SqliteParameter[] Params = cmd.Parameters.Cast<SqliteParameter>().ToArray();
                            cmd.Parameters.Clear();
                            query = _DbContext.Set<T>().FromSql(cmd.CommandText, Params).ToList();
                        }
                        else
                        {
                            SqlParameter[] Params = cmd.Parameters.Cast<SqlParameter>().ToArray();
                            cmd.Parameters.Clear();
                            query = _DbContext.Set<T>().FromSql(cmd.CommandText, Params).ToList();
                            //query = _DbContext.Database.SqlQuery<T>(cmd.CommandText, Params).ToList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
                }
            }
            return query;
        }
        /// <summary>
        /// 下多個Select SQL指令尋找符合物件模型篩選條件的資料(以帶SQL含參數指令方式)
        /// </summary>
        /// <param name="Sql"></param>
        /// <param name="ParametersList"></param>
        /// <param name="RecordSetNum"></param>
        /// <param name="OutTypes"></param>
        /// <returns></returns>
        public virtual IList FindMultiRecordSet(string Sql, Dictionary<string, object> ParametersList, int RecordSetNum, Type[] OutTypes)
        {
            //dynamic[] AllQuerys = new dynamic[] { };
            List<object> AllQuerys = new List<object>();
            DbConnection DBCon = _DbContext.Database.GetDbConnection();
            try
            {
                Sql = Sql.EndsWith(";") ? Sql.Substring(0, Sql.Length - 1) : Sql;
                string[] arrySql = Sql.Split(';');
                if (arrySql.Length == RecordSetNum && RecordSetNum == OutTypes.Length && CheckQueryStatement(arrySql))
                {
                    //AllQuerys = new dynamic[RecordSetNum];
                    DBCon.Open();
                    DbCommand cmd = DBCon.CreateCommand();
                    cmd.CommandText = Sql;
                    cmd.CommandType = CommandType.Text;
                    foreach (KeyValuePair<string, object> param in ParametersList)
                    {
                        DbParameter inParameter = cmd.CreateParameter();
                        inParameter.ParameterName = param.Key;
                        inParameter.Value = param.Value;
                        cmd.Parameters.Add(inParameter);
                    }
                    using (var reader = cmd.ExecuteReader())
                    {
                        for (int i = 0; i < RecordSetNum; i++)
                        {
                            //AllQuerys[i] = Util.DataReaderToObjectList(OutTypes[i], reader);
                            AllQuerys.Add(Util.DataReaderToObjectList(OutTypes[i], reader));
                            reader.NextResult();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
            }
            finally
            {
                DBCon.Close();
            }
            return AllQuerys;
        }
        /// <summary>
        /// 執行無回傳資料集的預存程序(依預存程序需求帶入輸入輸出參數及預存程序回傳值)
        /// </summary>
        /// <param name="ProcedureName"></param>
        /// <param name="OutValue"></param>
        /// <param name="InParameters"></param>
        /// <param name="OutParameters"></param>
        public virtual void ExecuteProcedure(string ProcedureName, out object OutValue, Dictionary<string, object> InParameters = null, Dictionary<string, object> OutParameters = null)
        {
            DbConnection DBCon = _DbContext.Database.GetDbConnection();
            OutValue = 0;
            //Had to go this route since EF Code First doesn't support output parameters 
            //returned from sprocs very well at this point
            try
            {
                DBCon.Open();
                DbCommand cmd = DBCon.CreateCommand();
                cmd.CommandText = ProcedureName;
                cmd.CommandType = CommandType.StoredProcedure;
                if (InParameters != null)
                {
                    foreach (KeyValuePair<string, object> param in InParameters)
                    {
                        DbParameter inParameter = cmd.CreateParameter();
                        inParameter.ParameterName = param.Key;
                        inParameter.Value = param.Value;
                        cmd.Parameters.Add(inParameter);
                    }
                }
                var outParam = cmd.CreateParameter();
                if (OutParameters != null && OutParameters.Count() == 1)
                {
                    foreach (KeyValuePair<string, object> param in OutParameters)
                    {
                        outParam.ParameterName = param.Key;
                        outParam.Value = param.Value;
                        outParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outParam);
                    }
                }
                var reader = cmd.ExecuteReader();
                //Access output variable after reader is closed
                OutValue = (outParam.Value == null) ? outParam.Value : Convert.ToInt32(outParam.Value);
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
            }
            finally
            {
                DBCon.Close();
            }
        }
        /// <summary>
        /// 執行回傳單一資料集的預存程序(依預存程序需求帶入輸入輸出參數及預存程序回傳值)
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="ProcedureName"></param>
        /// <param name="OutValue"></param>
        /// <param name="InParameters"></param>
        /// <param name="OutParameters"></param>
        /// <returns></returns>
        public virtual List<T1> ExecuteProcedure<T1>(string ProcedureName, out object OutValue, Dictionary<string, object> InParameters = null, Dictionary<string, object> OutParameters = null) where T1 : new()
        {
            DbConnection DBCon = _DbContext.Database.GetDbConnection();
            List<T1> query = new List<T1>();
            OutValue = 0;
            //Had to go this route since EF Code First doesn't support output parameters 
            //returned from sprocs very well at this point
            try
            {
                DBCon.Open();
                DbCommand cmd = DBCon.CreateCommand();
                cmd.CommandText = ProcedureName;
                cmd.CommandType = CommandType.StoredProcedure;
                if (InParameters != null)
                {
                    foreach (KeyValuePair<string, object> param in InParameters)
                    {
                        DbParameter inParameter = cmd.CreateParameter();
                        inParameter.ParameterName = param.Key;
                        inParameter.Value = param.Value;
                        cmd.Parameters.Add(inParameter);
                    }
                }
                var outParam = cmd.CreateParameter();
                if (OutParameters != null && OutParameters.Count() == 1)
                {
                    foreach (KeyValuePair<string, object> param in OutParameters)
                    {
                        outParam.ParameterName = param.Key;
                        outParam.Value = param.Value;
                        outParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outParam);
                    }
                }
                using (var reader = cmd.ExecuteReader())
                {
                    query = Util.DataReaderToObjectList<T1>(reader);
                }
                //Access output variable after reader is closed
                OutValue = (outParam.Value == null) ? outParam.Value : Convert.ToInt32(outParam.Value);
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
            }
            finally
            {
                DBCon.Close();
            }
            return query;
        }
        /// <summary>
        /// 執行回傳多個資料集的預存程序(依預存程序需求帶入輸入輸出參數及預存程序回傳值)
        /// </summary>
        /// <param name="ProcedureName"></param>
        /// <param name="RecordSetNum"></param>
        /// <param name="OutTypes"></param>
        /// <param name="OutValue"></param>
        /// <param name="InParameters"></param>
        /// <param name="OutParameters"></param>
        /// <returns></returns>
        public virtual IList ExecuteProcedure(string ProcedureName, int RecordSetNum, Type[] OutTypes, out object OutValue, Dictionary<string, object> InParameters = null, Dictionary<string, object> OutParameters = null)
        {
            DbConnection DBCon = _DbContext.Database.GetDbConnection();
            OutValue = 0;
            //dynamic[] AllQuerys = new dynamic[] { };
            List<object> AllQuerys = new List<object>();
            //Had to go this route since EF Code First doesn't support output parameters 
            //returned from sprocs very well at this point
            try
            {
                if (RecordSetNum == OutTypes.Length)
                {
                    //AllQuerys = new dynamic[RecordSetNum];
                    DBCon.Open();
                    DbCommand cmd = DBCon.CreateCommand();
                    cmd.CommandText = ProcedureName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (InParameters != null)
                    {
                        foreach (KeyValuePair<string, object> param in InParameters)
                        {
                            DbParameter inParameter = cmd.CreateParameter();
                            inParameter.ParameterName = param.Key;
                            inParameter.Value = param.Value;
                            cmd.Parameters.Add(inParameter);
                        }
                    }
                    var outParam = cmd.CreateParameter();
                    if (OutParameters != null && OutParameters.Count() == 1)
                    {
                        foreach (KeyValuePair<string, object> param in OutParameters)
                        {
                            outParam.ParameterName = param.Key;
                            outParam.Value = param.Value;
                            outParam.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(outParam);
                        }
                    }
                    using (var reader = cmd.ExecuteReader())
                    {
                        for (int i = 0; i < RecordSetNum; i++)
                        {
                            //AllQuerys[i] = Util.DataReaderToObjectList(OutTypes[i], reader);
                            AllQuerys.Add(Util.DataReaderToObjectList(OutTypes[i], reader));
                            reader.NextResult();
                        }
                    }
                    //Access output variable after reader is closed
                    OutValue = (outParam.Value == null) ? outParam.Value : Convert.ToInt32(outParam.Value);
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
            }
            finally
            {
                DBCon.Close();
            }
            return AllQuerys;
        }
        public virtual bool Add(T entity)
        {
            bool result = false;
            try
            {
                _dbset.Add(entity);
                _DbContext.SaveChanges();
                result = true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ex.Entries.Single().Reload();
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
            }
            return result;
        }
        public virtual bool Add(List<T> TEntitySet)
        {
            bool result = false;
            _dbset.AddRange(TEntitySet);
            try
            {
                _DbContext.SaveChanges();
                result = true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ex.Entries.Single().Reload();
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
            }
            return result;
        }
        public virtual bool Update(T entity)
        {
            bool result = false;
            _dbset.Attach(entity);
            _DbContext.Entry(entity).State = EntityState.Modified;
            try
            {
                _DbContext.SaveChanges();
                result = true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ex.Entries.Single().Reload();
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
            }
            return result;
        }
        public virtual bool Update(List<T> TEntitySet)
        {
            bool result = false;
            //_DbContext.Configuration.AutoDetectChangesEnabled = false;
            //_DbContext.Configuration.ValidateOnSaveEnabled = false;
            foreach (T Entity in TEntitySet)
            {
                _dbset.Attach(Entity);
                _DbContext.Entry(Entity).State = EntityState.Modified;
            }
            try
            {
                _DbContext.SaveChanges();
                result = true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ex.Entries.Single().Reload();
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
            }
            //_DbContext.Configuration.AutoDetectChangesEnabled = true;
            //_DbContext.Configuration.ValidateOnSaveEnabled = true;
            return result;
        }
        public virtual bool Delete(T entity)
        {
            bool result = false;
            _dbset.Attach(entity);
            _DbContext.Entry(entity).State = EntityState.Deleted;
            try
            {
                _DbContext.SaveChanges();
                result = true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ex.Entries.Single().Reload();
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
            }
            return result;
        }
        public virtual bool Delete(List<T> TEntitySet)
        {
            bool result = false;
            //_DbContext.Configuration.AutoDetectChangesEnabled = false;
            //_DbContext.Configuration.ValidateOnSaveEnabled = false;
            foreach (T Entity in TEntitySet)
            {
                _dbset.Attach(Entity);
                _DbContext.Entry(Entity).State = EntityState.Deleted;
            }
            try
            {
                _DbContext.SaveChanges();
                result = true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ex.Entries.Single().Reload();
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
            }
            return result;
            //_DbContext.Configuration.AutoDetectChangesEnabled = true;
            //_DbContext.Configuration.ValidateOnSaveEnabled = true;
        }
        public virtual bool CheckPKExist(Dictionary<string, object> PkValueList)
        {
            bool isPKExist = false;
            List<SqlParameter> SqlParameters = new List<SqlParameter>();
            List<SqliteParameter> SqliteParameters = new List<SqliteParameter>();
            try
            {
                ObjectContext objectContext = new ObjectContext(_DbContext.Database.GetDbConnection().ConnectionString);
                ObjectSet<T> set = objectContext.CreateObjectSet<T>();
                List<EdmMember> PkMembers = set.EntitySet.ElementType.KeyMembers.ToList();
                if (CheckPrimaryKeyIdentity(PkMembers.Select(r => r.Name.ToLowerInvariant()).ToArray(), PkValueList.Keys.Select(r => r.ToLowerInvariant()).ToArray()))
                {
                    foreach (KeyValuePair<string, object> param in PkValueList)
                    {
                        if (_DbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                        {
                            SqliteParameters.Add(new SqliteParameter(param.Key, param.Value));
                        }
                        else
                        {
                            SqlParameters.Add(new SqlParameter(param.Key, param.Value));
                        }
                    }
                    if (_DbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                    {
                        isPKExist = (int)_DbContext.Set<object>().FromSql(GenerateCountSQL(PkValueList), SqliteParameters.ToArray()).First() > 0 ? true : false;
                    }
                    else
                    {
                        isPKExist = (int)_DbContext.Set<object>().FromSql(GenerateCountSQL(PkValueList), SqlParameters.ToArray()).First() > 0 ? true : false;
                    }
                }
                else
                {
                    isPKExist = false;
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<EntityRepository<T>>(ex);
            }
            return isPKExist;
        }
        private RawSqlString GenerateSQL(T TEntity)
        {
            ObjectContext objectContext = new ObjectContext(_DbContext.Database.GetDbConnection().ConnectionString);
            ObjectSet<T> set = objectContext.CreateObjectSet<T>();
            string tableName = set.EntitySet.ElementType.Name;
            List<EdmMember> members = set.EntitySet.ElementType.Members.ToList();
            StringBuilder sSQLBuilder = new StringBuilder("SELECT * FROM " + tableName + " where ");
            foreach (EdmMember member in members)
            {
                if (_DbContext.Entry(TEntity).Property(member.Name).CurrentValue != null && (!_DbContext.Entry(TEntity).Property(member.Name).CurrentValue.ToString().Equals("") && !_DbContext.Entry(TEntity).Property(member.Name).CurrentValue.ToString().Equals("0")))
                {
                    sSQLBuilder.Append(member.Name + "=@" + member.Name + " and ");
                }
            }
            string sSQL = sSQLBuilder.ToString();
            if (sSQL.EndsWith(" where "))
            {
                sSQL = sSQL.Substring(0, sSQL.Length - " where ".Length);
            }
            if (sSQL.EndsWith(" and "))
            {
                sSQL = sSQL.Substring(0, sSQL.Length - " and ".Length);
            }
            return new RawSqlString(sSQL);
        }
        private RawSqlString GenerateCountSQL(Dictionary<string, object> PkValueList)
        {
            ObjectContext objectContext = new ObjectContext(_DbContext.Database.GetDbConnection().ConnectionString);
            ObjectSet<T> set = objectContext.CreateObjectSet<T>();
            string tableName = set.EntitySet.ElementType.Name;
            List<EdmMember> PkMembers = set.EntitySet.ElementType.KeyMembers.ToList();
            if (CheckPrimaryKeyIdentity(PkMembers.Select(r => r.Name.ToLowerInvariant()).ToArray(), PkValueList.Keys.Select(r => r.ToLowerInvariant()).ToArray()))
            {
                StringBuilder sSQLBuilder = new StringBuilder("SELECT COUNT(*) FROM " + tableName + " where ");
                foreach (KeyValuePair<string, object> param in PkValueList)
                {
                    sSQLBuilder.Append(param.Key + "=@" + param.Key + " and ");
                }
                string sSQL = sSQLBuilder.ToString();
                if (sSQL.EndsWith(" where "))
                {
                    sSQL = sSQL.Substring(0, sSQL.Length - " where ".Length);
                }
                if (sSQL.EndsWith(" and "))
                {
                    sSQL = sSQL.Substring(0, sSQL.Length - " and ".Length);
                }
                return new RawSqlString(sSQL);
            }
            else
            {
                return new RawSqlString("");
            }
        }
        /// <summary>
        /// 檢查多個SQL指令是否都是查詢語法
        /// </summary>
        /// <param name="Sqls"></param>
        /// <returns></returns>
        private bool CheckQueryStatement(string[] Sqls)
        {
            bool result = true;
            foreach (string Sql in Sqls)
            {
                if (!Sql.Trim().ToLower().StartsWith("select "))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }
        private bool CheckPrimaryKeyIdentity(string[] FirstPKs, string[] SecondPKs)
        {
            bool result = false;
            if (FirstPKs.Length != SecondPKs.Length)
            {
                result = false;
            }
            else
            {
                result = FirstPKs.SequenceEqual(SecondPKs);
            }

            return result;
        }
    }
}
