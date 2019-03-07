using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Text;

namespace DataAccess
{
    public class DBConnection
    {
        public static string GetEntityServerPlainConnString(string EncryptConnString)
        {
            EntityConnectionStringBuilder entBuilder = new EntityConnectionStringBuilder();
            entBuilder.Provider = "System.Data.SqlClient";
            entBuilder.ProviderConnectionString = GetSqlServerPlainConnString(EncryptConnString);
            entBuilder.Metadata = @"res://*/Model1.csdl|res://*/Model1.ssdl|res://*/Model1.msl";
            return entBuilder.ToString();
        }
        /// <summary>
        /// 取得SQL Server解密後的連線字串
        /// </summary>
        /// <param name="EncryptConnString">加密的連線字串</param>
        /// <returns></returns>
        public static string GetSqlServerPlainConnString(string EncryptConnString)
        {
            SqlConnectionStringBuilder SCSB = new SqlConnectionStringBuilder(EncryptConnString);
            SCSB.DataSource = DecEncCode.AESDecrypt(SCSB.DataSource);
            SCSB.UserID = DecEncCode.AESDecrypt(SCSB.UserID);
            SCSB.Password = DecEncCode.AESDecrypt(SCSB.Password);
            SCSB.InitialCatalog = DecEncCode.AESDecrypt(SCSB.InitialCatalog);
            return SCSB.ToString();
        }
        /// <summary>
        /// 取得Oracle解密後的連線字串
        /// </summary>
        /// <param name="EncryptConnString">加密的連線字串</param>
        /// <returns></returns>
        public static string GetOraclePlainConnString(string EncryptConnString)
        {
            StringBuilder SB = new StringBuilder();
            string[] ConnStr = EncryptConnString.Split(new char[] { ';' });
            foreach (string ConKeyWord in ConnStr)
            {
                if (ConKeyWord.Contains("="))
                {
                    string LeftWord = "";
                    string RightWord = "";
                    string[] WholeKeyWord = ConKeyWord.Split(new char[] { '=' });
                    if (WholeKeyWord.Length == 2)
                    {
                        LeftWord = WholeKeyWord[0];
                        RightWord = DecEncCode.AESDecrypt(WholeKeyWord[1]);
                    }
                    else if (WholeKeyWord.Length > 2)
                    {
                        LeftWord = WholeKeyWord[0];
                        string RightEncryptWord = "";
                        for (int i = 1; i < WholeKeyWord.Length; i++)
                        {
                            if (WholeKeyWord[i] != "")
                            {
                                RightEncryptWord += WholeKeyWord[i];
                            }
                            else
                            {
                                RightEncryptWord += "=";
                            }
                        }
                        RightWord = DecEncCode.AESDecrypt(RightEncryptWord);
                    }
                    SB.AppendFormat("{0}={1};", LeftWord, RightWord);
                }
            }
            return SB.ToString();
        }
    }
}
