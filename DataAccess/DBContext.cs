using DBModels;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace DataAccess
{
    public class DBContext : DbContext
    {
        private dbtype DbType { get; set; }
        private string DatabasePathOrConnectionName { get; set; }
        public DBContext()
        {
        }
        public DBContext(dbtype dbType, string databasePathOrConnectionName)
        {
            DbType = dbType;
            if (dbType == dbtype.SQL_Server)
            {
                string ConnStr = ConfigurationManager.ConnectionStrings[databasePathOrConnectionName].ToString();
                DatabasePathOrConnectionName = DBConnection.GetEntityServerPlainConnString(ConnStr);
            }
            else if (dbType == dbtype.Sqlite)
            {
                DatabasePathOrConnectionName = databasePathOrConnectionName;
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (DbType == dbtype.SQL_Server)
            {
                optionsBuilder.UseSqlServer(DatabasePathOrConnectionName);
            }
            else if (DbType == dbtype.Sqlite)
            {
                optionsBuilder.UseSqlite($"Filename={DatabasePathOrConnectionName}");
            }
        }
        public virtual DbSet<MessageAddressee> MessageAddressee { get; set; }

        public virtual DbSet<Profile> Profile { get; set; }
    }
}
