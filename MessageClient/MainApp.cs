using Android.App;
using Android.Content;
using Android.Net;
using Android.Runtime;
using Common;
using DBLogic;
using MessageClient.Services;
using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MessageClient
{
    [Application(Icon = "@drawable/moneysq")]
    public class MainApp : Android.App.Application
    {
        public static GlobalVariable GlobalVariable = new GlobalVariable();
        Intent MqService;

        public MainApp(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            TimerCallback timerDelegate = new TimerCallback(RestartApp);
            Timer RestartAppTimer = new Timer(timerDelegate, null, 1000, 1000);
            //config init ...
            Task.Run(() =>
            {
                FileInfo IniFile = new FileInfo(Path.Combine(Context.GetExternalFilesDir("").AbsolutePath, ".common.ini"));
                if (!IniFile.Exists)
                {
                    var type = this.GetType();
                    var resource = type.Namespace + ".Resources.common.ini";
                    using (var stream = type.Assembly.GetManifestResourceStream(resource))
                    {
                        if (stream != null)
                        {
                            using (Stream file = File.Create(Path.Combine(Context.GetExternalFilesDir("").AbsolutePath, ".common.ini")))
                            {
                                stream.CopyTo(file);
                            }
                        }
                    }
                }
                using (FileStream FS = IniFile.OpenRead())
                {
                    try
                    {
                        //Config.Context = this;
                        Config.ConfigStream = FS;
                        Config.ReadParameter();
                        Common.LogHelper.MoneySQLogger.logPath = Path.Combine(Context.GetExternalFilesDir("").AbsolutePath, Config.logDir);
                    }
                    catch (Exception ex)
                    {
                        Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "MainApp OnCreate() Error({0})", ex.Message);
                        Common.LogHelper.MoneySQLogger.LogError<MQService>(ex);
                    }
                    //develop stage temporarily reserved,product stage must delele for user can not change ini file
                    //IniFile.Delete();
                }

                //WifiManager WifiManager = (WifiManager)GetSystemService(WifiService);
                //if (!WifiManager.IsWifiEnabled)
                //{
                //    if (WifiManager.SetWifiEnabled(true))
                //    {
                //        WifiManager.SetWifiEnabled(false);
                //    }
                //}

                //if (!CheckNetworkConnection())
                //{
                //    string logText = "WIFI or 行動網路尚未開啟連線";
                //    MQService.ErrorInMainApp = logText;
                //    MQService.IsFinishedMqService = true;
                //    Common.LogHelper.MoneySQLogger.LogInfo<MainApp>(logText);
                //    return;
                //}
                //init db
                if (!MainApp.GlobalVariable.DBFile.Exists)
                {
                    var type = this.GetType();
                    var resource = type.Namespace + ".Resources.db.MoneySQ.db";
                    using (var stream = type.Assembly.GetManifestResourceStream(resource))
                    {
                        if (stream != null)
                        {
                            using (Stream file = File.Create(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "MoneySQ.db")))
                            {
                                stream.CopyTo(file);
                            }
                        }
                    }
                }
                try
                {
                    if (DBProfile.CheckProfileExist(MainApp.GlobalVariable.DBFile.FullName))
                    {
                        Common.LogHelper.MoneySQLogger.DeleteLogFile(Config.preservedDaysForLog, Common.LogHelper.MoneySQLogger.logPath);
                        int fstIdx = Config.dbWebService.IndexOf("/");
                        int lstIdx = Config.dbWebService.LastIndexOf("/");
                        string url = Config.dbWebService.Substring(fstIdx, lstIdx - fstIdx).Replace("/", "");
                        string port = string.Empty;
                        port = url.IndexOf(":") > -1 ? url.Split(new char[] { ':' })[1] : "443";
                        url = url.IndexOf(":") > -1 ? url.Split(new char[] { ':' })[0] : url;
                        bool IsWebServiceAlive = MainApp.CheckServiceAlive(url, int.Parse(port));
                        NetworkState NetworkState = MainApp.GetNetworkStatus();
                        if (IsWebServiceAlive && (NetworkState == NetworkState.ConnectedWifi || NetworkState == NetworkState.ConnectedData))
                        {
                            if (LoginService.CheckIDValidation())
                            {
                                MqService = new Intent(this, typeof(Services.MQService));
                                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                                {
                                    StartForegroundService(MqService);
                                }
                                else
                                {
                                    StartService(MqService);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "MainApp OnCreate() Error({0})", ex.Message);
                    Common.LogHelper.MoneySQLogger.LogError<MainApp>(ex);
                }
            });
        }

        public override void OnTerminate()
        {
            base.OnTerminate();
            try
            {
                StopService(MqService);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "MainApp OnTerminate() Error({0})", ex.Message);
                Common.LogHelper.MoneySQLogger.LogError<MainApp>(ex);
            }
        }

        private bool CheckNetworkConnection()
        {
            bool CanNetworkConnection;
            ConnectivityManager connectivityManager = GetSystemService(Context.ConnectivityService) as ConnectivityManager;
            var activeConnection = connectivityManager.ActiveNetworkInfo;
            if ((activeConnection == null) || (activeConnection != null && (activeConnection.Type != ConnectivityType.Mobile && activeConnection.Type != ConnectivityType.Wifi)))
            {
                CanNetworkConnection = false;
            }
            else
            {
                CanNetworkConnection = true;
            }
            return CanNetworkConnection;
        }
        private void RestartApp(object state)
        {
            if ((DateTime.Now.Hour == 9 || DateTime.Now.Hour == 15) && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
            {
                if (CheckNetworkConnection())
                {
                    var i = PackageManager.GetLaunchIntentForPackage(this.PackageName);
                    i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
                    this.StartActivity(i);
                    Java.Lang.Runtime.GetRuntime().Exit(0);
                }
            }
        }
        /// <summary>
        /// 檢查服務可用性
        /// </summary>
        /// <param name="url"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool CheckServiceAlive(string url, int port)
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp))
            {
                var result = socket.BeginConnect(url, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(5000, false); // test the connection for 5 seconds
                var resturnVal = socket.Connected;
                return resturnVal;
            }
        }
        public static NetworkState GetNetworkStatus()
        {
            NetworkState _state = NetworkState.Unknown;
            try
            {
                // Retrieve the connectivity manager service
                var connectivityManager = (ConnectivityManager)
                    Application.Context.GetSystemService(
                        Context.ConnectivityService);

                // Check if the network is connected or connecting.
                // This means that it will be available, 
                // or become available in a few seconds.
                var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;

                if (activeNetworkInfo != null && activeNetworkInfo.IsConnectedOrConnecting)
                {
                    // Now that we know it's connected, determine if we're on WiFi or something else.
                    _state = activeNetworkInfo.Type == ConnectivityType.Wifi ?
                        NetworkState.ConnectedWifi : NetworkState.ConnectedData;
                }
                else
                {
                    _state = NetworkState.Disconnected;
                }
            }
            catch(Exception ex)
            {
            }
            return _state;
        }
    }

    public enum NetworkState
    {
        Unknown,
        ConnectedWifi,
        ConnectedData,
        Disconnected
    }
}