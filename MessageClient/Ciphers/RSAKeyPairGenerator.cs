using Android.App;
using Android.Content;
using Java.IO;
using Java.Security;
using Java.Security.Spec;
using System;
using System.Reflection;

namespace MessageClinet.Ciphers
{
    public class RSAKeyPairGenerator : Activity
    {
        ISharedPreferences SP;
        ISharedPreferencesEditor SPE;
        Context context;
        public int KeySize { get; set; } = 2048;
        public RSAKeyPairGenerator(Context context)
        {
            this.context = context;
            SP = context.GetSharedPreferences("KeyPair", FileCreationMode.Private);
        }
        public void GenerateKeys(int ValidYears = 2)
        {
            try
            {
                DeleteKeyPair();
                DateTime ValidDate = DateTime.Now.AddYears(ValidYears);
                KeyPairGenerator generator;
                generator = KeyPairGenerator.GetInstance("RSA", "BC");
                generator.Initialize(KeySize, new SecureRandom());
                KeyPair pair = generator.GenerateKeyPair();
                IPublicKey PubKey = pair.Public;
                IPrivateKey PriKey = pair.Private;
                byte[] publicKeyBytes = PubKey.GetEncoded();
                String pubKeyStr = Convert.ToBase64String(publicKeyBytes);
                byte[] privKeyBytes = PriKey.GetEncoded();
                String privKeyStr = Convert.ToBase64String(privKeyBytes);
                SPE = SP.Edit();
                SPE.PutString("PublicKey", pubKeyStr);
                SPE.PutString("PrivateKey", privKeyStr);
                SPE.PutString("Validity", ValidDate.ToString("yyyy/MM/dd HH:mm:ss"));
                SPE.Commit();
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message);
                Common.LogHelper.MoneySQLogger.LogError<RSAKeyPairGenerator>(ex);
            }
        }
        public bool IsExpired()
        {
            String Validity = SP.GetString("Validity", "1970/1/1");
            return DateTime.Now > Convert.ToDateTime(Validity);
        }
        public IPublicKey GetPublicKey()
        {
            IPublicKey Result = null;
            String pubKeyStr = SP.GetString("PublicKey", "");
            byte[] sigBytes = Convert.FromBase64String(pubKeyStr);
            X509EncodedKeySpec x509KeySpec = new X509EncodedKeySpec(sigBytes);
            KeyFactory keyFact = null;
            try
            {
                keyFact = KeyFactory.GetInstance("RSA", "BC");
                Result = keyFact.GeneratePublic(x509KeySpec);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message);
                Common.LogHelper.MoneySQLogger.LogError<RSAKeyPairGenerator>(ex);
            }
            return Result;
        }
        public IPrivateKey GetPrivateKey()
        {
            IPrivateKey Result = null;
            String privKeyStr = SP.GetString("PrivateKey", "");
            byte[] sigBytes = Convert.FromBase64String(privKeyStr);
            PKCS8EncodedKeySpec pkcs8KeySpec = new PKCS8EncodedKeySpec(sigBytes);
            KeyFactory keyFact = null;
            try
            {
                keyFact = KeyFactory.GetInstance("RSA", "BC");
                Result = keyFact.GeneratePrivate(pkcs8KeySpec);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message);
                Common.LogHelper.MoneySQLogger.LogError<RSAKeyPairGenerator>(ex);
            }
            return Result;
        }
        public String GetPublicKeyString()
        {
            return SP.GetString("PublicKey", "");
        }
        public String GetPrivateKeyString()
        {
            return SP.GetString("PrivateKey", "");
        }
        public DateTime GetExpireDate()
        {
            return Convert.ToDateTime(SP.GetString("Validity", "1970/1/1"));
        }
        private void DeleteKeyPair()
        {
            File file = new File("/data/data/" + this.context.PackageName.ToString() + "/shared_prefs", "KeyPair.xml");
            if (file.Exists())
            {
                file.Delete();
            }
        }
    }
}