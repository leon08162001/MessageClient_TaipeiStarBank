using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Common;
using Common.Ciphers;
using Common.LinkLayer;
using DBLogic;
using DBModels;
using MessageClient.Utils;
using MessageClient.Ciphers;
using MessageClient.Services;
using MessageClient.ViewHolder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utility;
using MessageClinet;
using Android.Content.PM;

namespace MessageClient
{
    //[Activity( MainLauncher = true, Icon = "@drawable/moneysq")]
    [Activity(MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : TabActivity
    {
        private GestureDetector _gestureDetector;
        private GestureListener _gestureListener;
        private System.Timers.Timer ChkIsFinishedMqServiceTimer;
        AlertDialog builder = null;
        LoginView LoginView = null;

        protected override async void OnCreate(Bundle bundle)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (o, cert, chain, errors) => true;
                base.OnCreate(bundle);
                SetContentView(Resource.Layout.Main);
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
                    ExecuteProfileSetting();
                }
                else
                {
                    Common.LogHelper.MoneySQLogger.LogError<MainActivity>("離線模式使用中(身份ID登入驗證服務因網路問題無法連線)");
                    Toast.MakeText(this, "離線模式使用中(身份ID登入驗證服務因網路問題無法連線)", ToastLength.Long).Show();
                }
                ChkIsFinishedMqServiceTimer = new System.Timers.Timer();
                ChkIsFinishedMqServiceTimer.Interval = 100;
                ChkIsFinishedMqServiceTimer.Elapsed += ChkIsFinishedMqServiceTimer_Elapsed;
                ChkIsFinishedMqServiceTimer.Start();

                var tab = this.TabHost.NewTabSpec("訊息公告");
                tab.SetIndicator("訊息公告");
                if ((MQService.MQJefferiesExcuReportPendingIntent != null && this.Intent.HasExtra("MQJefferiesExcuReportMessage")))
                {
                    Intent MessageTabIntent = new Intent(this, typeof(MessageTabActivity));
                    MessageTabIntent.FillIn(this.Intent, FillInFlags.Data);
                    tab.SetContent(MessageTabIntent);
                    this.TabHost.AddTab(tab);
                    this.TabHost.SetCurrentTabByTag("訊息公告");
                    MQService.MQJefferiesExcuReportPendingIntent = null;
                }
                else
                {
                    tab.SetContent(new Intent(this, typeof(MessageTabActivity)));
                    this.TabHost.AddTab(tab);
                }

                tab = this.TabHost.NewTabSpec("歷史訊息");
                tab.SetIndicator("歷史訊息");
                tab.SetContent(new Intent(this, typeof(HisMessageTabActivity)));
                this.TabHost.AddTab(tab);

                //加入WebView瀏覽網頁頁簽
                tab = this.TabHost.NewTabSpec("MoneySQ Site");
                tab.SetIndicator("MoneySQ Site");
                tab.SetContent(new Intent(this, typeof(MoneySQWebViewTabActivity)));
                MoneySQWebViewTabActivity.WebUrl = Config.moneysqWebSite;
                this.TabHost.AddTab(tab);

                //手勢程式碼
                _gestureListener = new GestureListener();
                _gestureListener.LeftEvent += GestureLeft;
                _gestureListener.RightEvent += GestureRight;
                _gestureDetector = new GestureDetector(this, _gestureListener);

                #region RSA加解密及數位簽章
                //await CertificateSignDataSendTestAsync(this.ApplicationContext);
                //await CertificateSignDataSendTestAsync1();
                #endregion RSA加解密及數位簽章
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<MainActivity>(ex);
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }
        protected override void OnDestroy()
        {
            ChkIsFinishedMqServiceTimer.Elapsed -= ChkIsFinishedMqServiceTimer_Elapsed;
            _gestureListener.LeftEvent -= GestureLeft;
            _gestureListener.RightEvent -= GestureRight;
            ChkIsFinishedMqServiceTimer.Dispose();
            _gestureDetector.Dispose();
            _gestureListener.Dispose();
            this.TabHost.RemoveAllViews();
            this.TabHost.Dispose();
            if (builder != null) { builder.Dispose(); }
            if (LoginView != null) { LoginView.Dispose(); }
            GC.Collect(0);
            base.OnDestroy();
        }

        private void GestureLeft()
        {
            if (TabHost.CurrentTab == 2)
            {
                return;
            }
            if (TabHost.CurrentTab - 1 < 0)
            {
                TabHost.CurrentTab = TabHost.TabWidget.TabCount - 1;
            }
            else
            {
                TabHost.CurrentTab--;
            }
        }

        private void GestureRight()
        {
            if (TabHost.CurrentTab == 2)
            {
                return;
            }
            if (TabHost.CurrentTab + 1 >= TabHost.TabWidget.TabCount)
            {
                TabHost.CurrentTab = 0;
            }
            else
            {
                TabHost.CurrentTab++;
            }
        }

        public TabHost GetMyTabHost() { return this.TabHost; }
        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            _gestureDetector.OnTouchEvent(ev);
            return base.DispatchTouchEvent(ev);
        }

        public override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            this.Window.SetTitle(this.Resources.GetString(Resource.String.ApplicationName) + "-" + Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionName);
        }

        void ChkIsFinishedMqServiceTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(MQService.IsFinishedMqService)
            {
                ChkIsFinishedMqServiceTimer.Enabled = false;
                if (MQService.ErrorInMainApp != null && MQService.ErrorInMainApp != "")
                {
                    RunOnUiThread(
                        () =>
                        {
                            //Intent MqService = new Intent(this, typeof(Services.MQService));
                            //StopService(MqService);
                            AlertDialog.Builder alert = new AlertDialog.Builder(this);
                            alert.SetCancelable(false);
                            alert.SetTitle("MQService初始化錯誤通知");
                            alert.SetMessage(MQService.ErrorInMainApp);
                            alert.SetNegativeButton("確定", ExistSystem);
                            alert.SetIcon(Android.Resource.Drawable.IcDialogInfo);
                            alert.Show();
                        }
                        );
                }
            }
        }
        private void ExistSystem(object sender, EventArgs e)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                this.FinishAndRemoveTask();
            else
                Finish();
            Intent MqService = new Intent(this, typeof(Services.MQService));
            StopService(MqService);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        private void StopPushService(object sender, EventArgs e)
        {
            Intent MqService = new Intent(this, typeof(Services.MQService));
            StopService(MqService);
        }
        /// <summary>
        /// 檢查推播所用的身分ID是否已設定
        /// </summary>
        private void ExecuteProfileSetting()
        {
            bool IsProfileExist = DBProfile.CheckProfileExist(MainApp.GlobalVariable.DBFile.FullName);
            if (!IsProfileExist)
            {
                var metrics = Resources.DisplayMetrics;
                LayoutInflater inflater = (LayoutInflater)Application.Context.GetSystemService(Context.LayoutInflaterService);
                View view = inflater.Inflate(Resource.Layout.LoginAlertDialog, null);
                LoginView = new LoginView(view);

                view.SetMinimumWidth((int)(metrics.WidthPixels * 0.9f));
                view.SetMinimumHeight((int)(metrics.Ydpi * 1.8));
                builder = new AlertDialog.Builder(this).Create();
                builder.SetView(view);
                builder.SetCanceledOnTouchOutside(false);
                LoginView.btnLogin.Click += BtnLogin_Click;
                LoginView.btnClear.Click += BtnClear_Click;
                builder.Show();
                builder.Window.SetLayout(Convert.ToInt16(metrics.WidthPixels * 0.9f), Convert.ToInt16(metrics.Ydpi * 2));
            }
            //驗證ID有效性
            else
            {
                //string ID = DBProfile.GetProfile(MainApp.GlobalVariable.DBFile.FullName).ID;
                //long UserType = DBProfile.GetUserType(ID, MainApp.GlobalVariable.DBFile.FullName);
                ////員工身份
                //if (UserType == 1)
                //{
                //    var client = new RestClient(Config.dbWebService);
                //    client.Timeout = Config.webServiceTimeOut * 1000;
                //    var request = new RestRequest("api/MoneySQ/JA_EMPOLYEE/CheckIDValidation", Method.POST);
                //    request.AddParameter("ID", ID, ParameterType.GetOrPost);
                //    request.AddHeader("cache-control", "no-cache");
                //    request.AddHeader("content-type", "application/json");
                //    IRestResponse response = client.Execute(request);

                //    if (response.ErrorMessage != null && response.ErrorMessage != "")
                //    {
                //        Common.LogHelper.MoneySQLogger.LogInfo<MainActivity>(response.ErrorMessage);
                //        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                //        alert.SetCancelable(false);
                //        alert.SetTitle("檢查ID有效性錯誤通知");
                //        //alert.SetMessage(response.ErrorMessage);
                //        //alert.SetNegativeButton("確定", ExistSystem);
                //        alert.SetMessage(response.ErrorMessage + "(推播通知服務將無法使用)");
                //        //alert.SetNegativeButton("確定", StopPushService);
                //        alert.SetNegativeButton("確定", (sender, args) => { });
                //        alert.SetIcon(Android.Resource.Drawable.IcDialogInfo);
                //        alert.Show();
                //    }
                //    else
                //    {
                //        if (!Convert.ToBoolean(response.Content))
                //        {
                //            AlertDialog.Builder alert = new AlertDialog.Builder(this);
                //            alert.SetCancelable(false);
                //            alert.SetTitle("檢查ID有效性結果通知");
                //            //alert.SetMessage("您的ID已失效,App將終止執行");
                //            //alert.SetNegativeButton("確定", ExistSystem);
                //            alert.SetMessage("您的ID已失效,推播通知服務將無法使用");
                //            //alert.SetNegativeButton("確定", StopPushService);
                //            alert.SetNegativeButton("確定", (sender, args) => { });
                //            alert.SetIcon(Android.Resource.Drawable.IcDialogInfo);
                //            alert.Show();
                //        }
                //    }
                //}
                ////客戶身份
                //else if (UserType == 2)
                //{

                //}
                while (!LoginService.IsIDValidationDone)
                {
                }
                if (LoginService.IsIDValidationDone && LoginService.IDValidationError.Length > 0)
                {
                    Common.LogHelper.MoneySQLogger.LogInfo<MainActivity>(LoginService.IDValidationError);
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetCancelable(false);
                    alert.SetTitle("檢查ID有效性");
                    alert.SetMessage(LoginService.IDValidationError);
                    alert.SetNegativeButton("確定", (sender, args) => { });
                    alert.SetIcon(Android.Resource.Drawable.IcDialogInfo);
                    alert.Show();
                }
            }
        }
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (!LoginView.txtID.Text.Trim().Equals(""))
            {
                var client = new RestClient(Config.dbWebService);
                client.Timeout = Config.webServiceTimeOut * 1000;
                var request = new RestRequest("api/MoneySQ/JA_EMPOLYEE/GetEmployeeID", Method.POST);
                request.AddParameter("ID", LoginView.txtID.Text.Trim(), ParameterType.GetOrPost);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                IRestResponse response = client.Execute(request);
                if (response.ErrorMessage != null && response.ErrorMessage != "")
                {
                    Common.LogHelper.MoneySQLogger.LogInfo<MainActivity>(response.ErrorMessage);
                    Toast.MakeText(this, response.ErrorMessage, ToastLength.Long).Show();
                    return;
                }
                if (!IsValidJson(response.Content))
                {
                    long UserType;
                    string ID = EscapeForQuote(response.Content);
                    //ID為空值,繼續找客戶申請檔
                    if (ID.Equals(""))
                    {
                        request = new RestRequest("api/MoneySQ/ZZ_APPLICATION/GetApplicantIDByID", Method.POST);
                        request.AddParameter("ID", LoginView.txtID.Text.Trim(), ParameterType.GetOrPost);
                        request.AddHeader("cache-control", "no-cache");
                        request.AddHeader("content-type", "application/json");
                        response = client.Execute(request);
                        if (!IsValidJson(response.Content))
                        {
                            ID = EscapeForQuote(response.Content);
                        }
                        else
                        {
                            ShowJsonError(response.Content);
                        }
                        UserType = 2;
                    }
                    //代表身分為員工
                    else
                    {
                        UserType = 1;
                    }
                    if (!ID.Equals(""))
                    {
                        if (ID.ToUpper().Equals(LoginView.txtID.Text.Trim().ToUpper()))
                        {
                            Profile Profile = new Profile();
                            Profile.ID = ID;
                            Profile.UserType = UserType;
                            if (DBProfile.InsertProfile(Profile, MainApp.GlobalVariable.DBFile.FullName))
                            {
                                //更新MoneySQ後台DB(JA_EMPOLYEE)
                                request = new RestRequest("api/MoneySQ/JA_EMPOLYEE/UpdateEmployeePushByID", Method.PUT);
                                request.AddParameter("ID", ID, ParameterType.GetOrPost);
                                request.AddHeader("cache-control", "no-cache");
                                request.AddHeader("content-type", "application/json");
                                response = client.Execute(request);
                                //更新MoneySQ後台DB(ZZ_APPLICATION)
                                request = new RestRequest("api/MoneySQ/ZZ_APPLICATION/UpdateApplicantPushByID", Method.PUT);
                                request.AddParameter("ID", ID, ParameterType.GetOrPost);
                                request.AddHeader("cache-control", "no-cache");
                                request.AddHeader("content-type", "application/json");
                                response = client.Execute(request);
                                builder.Dismiss();
                                Intent MqService = new Intent(this, typeof(Services.MQService));
                                StopService(MqService);
                                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                                {
                                    StartForegroundService(MqService);
                                }
                                else
                                {
                                    StartService(MqService);
                                }
                                Toast.MakeText(this, "設定完成接收推播的ID!", ToastLength.Long).Show();
                            }
                            else
                            {
                                Common.LogHelper.MoneySQLogger.LogInfo<MainActivity>("無法新增ID至Profile設定檔!");
                                Toast.MakeText(this, "無法新增ID至Profile設定檔!", ToastLength.Long).Show();
                            }
                        }
                        else
                        {
                            Common.LogHelper.MoneySQLogger.LogInfo<MainActivity>(ID);
                            Toast.MakeText(this, ID, ToastLength.Long).Show();
                        }
                    }
                    else
                    {
                        Common.LogHelper.MoneySQLogger.LogInfo<MainActivity>("設定失敗,因申請檔及員工檔找不到符合的ID!");
                        Toast.MakeText(this, "設定失敗,因申請檔及員工檔找不到符合的ID!", ToastLength.Long).Show();
                    }
                }
                else
                {
                    ShowJsonError(response.Content);
                }
            }
            else
            {
                Toast.MakeText(this, "請輸入您的身分ID!", ToastLength.Long).Show();
            }
        }
        private void BtnClear_Click(object sender, EventArgs e)
        {
            builder.Dismiss();
            Toast.MakeText(this, "尚未設定接收推播的身分ID!", ToastLength.Long).Show();
        }
        private void MCmpClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                string ID = Encoding.UTF8.GetString(e.Result);
                if (!ID.Equals(""))
                {
                    Profile Profile = new Profile();
                    Profile.ID = LoginView.txtID.Text.Trim();
                    DBProfile.InsertProfile(Profile, MainApp.GlobalVariable.DBFile.FullName);
                    builder.Dismiss();
                    Intent MqService = new Intent(this, typeof(Services.MQService));
                    StopService(MqService);
                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                    {
                        StartForegroundService(MqService);
                    }
                    else
                    {
                        StartService(MqService);
                    }
                    Toast.MakeText(this, "設定完成接收推播的ID!", ToastLength.Long).Show();
                }
            });
        }
        private bool IsValidJson(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }
            var value = s.Trim();
            if ((value.StartsWith("{") && value.EndsWith("}")) || //For object
                (value.StartsWith("[") && value.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(value);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
            }
            return false;
        }
        private void ShowJsonError(string json)
        {
            JObject JObj = JsonConvert.DeserializeObject<JObject>(json);
            if (JObj["ErrorCode"].Type != JTokenType.Null && JObj["ErrorMessage"].Type != JTokenType.Null)
            {
                Common.LogHelper.MoneySQLogger.LogInfo<MainActivity>(JObj["ErrorMessage"].ToString());
                Toast.MakeText(this, JObj["ErrorMessage"].ToString(), ToastLength.Long).Show();
            }
        }
        private string EscapeForQuote(string s)
        {
            string result = s;
            result = result.Replace(@"""", "");
            return result;
        }
        private Task CertificateSignDataSendTestAsync(Context AppContext)
        {
            //android keystore certificate Sign Code begin
            //PlatformEncryptionKeyHelper _encryptionKeyHelper = new PlatformEncryptionKeyHelper(Application.Context, "A123456789");
            //string encryptedString = "";
            //if (!_encryptionKeyHelper.KeysExist())
            //{
            //    _encryptionKeyHelper.CreateKeyPair();
            //}
            //else
            //{
            //    if (_encryptionKeyHelper.IsExpired())
            //    {
            //        _encryptionKeyHelper.CreateKeyPair();
            //    }
            //    else if (!_encryptionKeyHelper.IsExpired())
            //    {
            //        IKey _privateKey = _encryptionKeyHelper.GetPrivateKey();
            //        IKey _publicKey = _encryptionKeyHelper.GetPublicKey();

            //        //encryption
            //        var transformation = "RSA/ECB/PKCS1Padding";
            //        var stringToEncrypt = "This is a simple string for demo purposes only. Nothing special here.";
            //        var cipher = Cipher.GetInstance(transformation);
            //        cipher.Init(CipherMode.EncryptMode, _publicKey);
            //        var encryptedData = cipher.DoFinal(Encoding.UTF8.GetBytes(stringToEncrypt));
            //        encryptedString = Encoding.UTF8.GetString(encryptedData);

            //        //Decryption
            //        transformation = "RSA/ECB/PKCS1Padding";
            //        cipher = Cipher.GetInstance(transformation);
            //        cipher.Init(CipherMode.DecryptMode, _privateKey);
            //        var decryptedBytes = cipher.DoFinal(encryptedData);
            //        var finalString = Encoding.UTF8.GetString(decryptedBytes);

            //        AlertDialog.Builder alert = new AlertDialog.Builder(this);
            //        alert.SetCancelable(true);
            //        alert.SetTitle("憑證加解密結果");
            //        alert.SetMessage("encryptedData=" + Encoding.UTF8.GetString(encryptedData) + "\n" +
            //                         "decryptedData=" + finalString);
            //        alert.SetIcon(Android.Resource.Drawable.IcDialogInfo);
            //        alert.Show();
            //    }
            //}
            //_encryptionKeyHelper.GenerateKeys();
            //android keystore certificate Sign Code end

            //SharedPreferences certificate Sign Code begin
            RSAKeyPairGenerator KPG = new RSAKeyPairGenerator(AppContext);
            return Task.Run(() =>
            {
                DateTime ExpireDate = KPG.GetExpireDate();
                if (KPG.IsExpired())
                {
                    KPG.GenerateKeys();
                }
                String pubKeyxml = CipherHelper.RSAPublicKeyJava2DotNet(KPG.GetPublicKeyString());
                String priKeyxml = CipherHelper.RSAPrivateKeyJava2DotNet(KPG.GetPrivateKeyString());

                //設定明文資料
                Messager.loanApplication_customer cust = new Messager.loanApplication_customer();
                cust.pk = 20;
                cust.order_nbr = "00000016";
                cust.country = "TWN";
                cust.seq = 2;
                cust.customer_type = "第三人(擔保人)";
                cust.nickname = "王勝達";
                cust.phone_home = "29158633";
                cust.phone_mobile = "0945225628";
                cust.phone_office = "71262678";
                cust.is_pay = true;
                cust.is_shareholder = true;
                cust.date_of_birth = new DateTime(1978, 6, 15);
                cust.add_1 = "新北市土城區金城路三段12號14樓";
                cust.add_2 = "新北市土城區金城路三段12號14樓";

                Messager.loanApplication_customer cust1 = new Messager.loanApplication_customer();
                cust1.pk = 16;
                cust1.order_nbr = "00000152";
                cust1.country = "TWN";
                cust1.seq = 3;
                cust1.customer_type = "第三人(擔保人)";
                cust1.nickname = "廖一民";
                cust1.phone_home = "22637608";
                cust1.phone_mobile = "0933810725";
                cust1.phone_office = "51263568";
                cust1.is_pay = true;
                cust1.is_shareholder = true;
                cust1.date_of_birth = new DateTime(1978, 6, 15);
                cust1.add_1 = "台北市中山區長安東路1段16號7樓之1";
                cust1.add_2 = "台北市中山區長安東路1段16號7樓之1";

                List<Messager.loanApplication_customer> lstCust = new List<Messager.loanApplication_customer>();
                lstCust.Add(cust);
                lstCust.Add(cust1);
                string plaintxt = JsonConvert.SerializeObject(lstCust);
                string ciphertxt = DecEncCode.AESEncrypt(plaintxt);

                //簽章
                string sign = CipherHelper.RsaSign(plaintxt, priKeyxml);
                //驗章
                bool IsOK = CipherHelper.RsaVerifyData(plaintxt, sign, pubKeyxml);
                ////加密
                //string ciphertxt1 = CipherHelper.RSAEncrypt(pubKeyxml, "hello world!");
                ////解密
                //string plaintxt1 = CipherHelper.RSADecrypt(priKeyxml, ciphertxt);

                //Use MQ傳送明文,簽章,公鑰給server begin
                Messager.SignMessage sm = new Messager.SignMessage();
                sm.plainText = plaintxt;
                sm.cipherText = ciphertxt;
                sm.sign = sign;
                sm.publickey = pubKeyxml;
                string jsonSM = JsonConvert.SerializeObject(sm);
                List<Dictionary<string, string>> DicMapList = Common.Utility.Util.ToMessageMapForJson(jsonSM);
                List<List<MessageField>> MultiMQMessage = new List<List<MessageField>>();
                foreach (Dictionary<string, string> DicMap in DicMapList)
                {
                    List<MessageField> MqMessage = new List<MessageField>();
                    //加入資料序號
                    MessageField MessageSeqenceField = new MessageField();
                    MessageSeqenceField.Name = "9999";
                    MessageSeqenceField.Value = "1";
                    MqMessage.Add(MessageSeqenceField);
                    //加入資料序號
                    foreach (string Dic in DicMap.Keys)
                    {
                        MessageField MessageField = new MessageField();
                        MessageField.Name = Dic;
                        MessageField.Value = DicMap[Dic];
                        MqMessage.Add(MessageField);
                    }
                    MultiMQMessage.Add(MqMessage);
                }
                IMQAdapter MQ;
                MQ = TopicMQFactory.GetMQAdapterInstance(MQAdapterType.BatchMQAdapter);
                MQ.Uri = Config.MQ_network + ":" + Config.MQ_service;
                MQ.DestinationFeature = DestinationFeature.VirtualTopic;
                MQ.MessageTimeOut = ((double)1 / 24) * ((double)1 / 60);
                MQ.SendName = "leon08162001@gmail.com.WebReq";
                MQ.UserName = DecEncCode.AESDecrypt(Config.MQUserID);
                MQ.PassWord = DecEncCode.AESDecrypt(Config.MQPwd);
                MQ.UseSSL = Config.MQ_useSSL;
                MQ.Start(Android.OS.Build.Serial);
                MQ.SendMQMessage("710", MultiMQMessage, 4, 25);
                //Use MQ傳送明文,簽章,公鑰給server end
                //SharedPreferences certificate Sign Code begin
            });
        }
        private async Task CertificateSignDataSendTestAsync1()
        {
            //android keystore certificate Sign Code begin
            //PlatformEncryptionKeyHelper _encryptionKeyHelper = new PlatformEncryptionKeyHelper(Application.Context, "A123456789");
            //string encryptedString = "";
            //if (!_encryptionKeyHelper.KeysExist())
            //{
            //    _encryptionKeyHelper.CreateKeyPair();
            //}
            //else
            //{
            //    if (_encryptionKeyHelper.IsExpired())
            //    {
            //        _encryptionKeyHelper.CreateKeyPair();
            //    }
            //    else if (!_encryptionKeyHelper.IsExpired())
            //    {
            //        IKey _privateKey = _encryptionKeyHelper.GetPrivateKey();
            //        IKey _publicKey = _encryptionKeyHelper.GetPublicKey();

            //        //encryption
            //        var transformation = "RSA/ECB/PKCS1Padding";
            //        var stringToEncrypt = "This is a simple string for demo purposes only. Nothing special here.";
            //        var cipher = Cipher.GetInstance(transformation);
            //        cipher.Init(CipherMode.EncryptMode, _publicKey);
            //        var encryptedData = cipher.DoFinal(Encoding.UTF8.GetBytes(stringToEncrypt));
            //        encryptedString = Encoding.UTF8.GetString(encryptedData);

            //        //Decryption
            //        transformation = "RSA/ECB/PKCS1Padding";
            //        cipher = Cipher.GetInstance(transformation);
            //        cipher.Init(CipherMode.DecryptMode, _privateKey);
            //        var decryptedBytes = cipher.DoFinal(encryptedData);
            //        var finalString = Encoding.UTF8.GetString(decryptedBytes);

            //        AlertDialog.Builder alert = new AlertDialog.Builder(this);
            //        alert.SetCancelable(true);
            //        alert.SetTitle("憑證加解密結果");
            //        alert.SetMessage("encryptedData=" + Encoding.UTF8.GetString(encryptedData) + "\n" +
            //                         "decryptedData=" + finalString);
            //        alert.SetIcon(Android.Resource.Drawable.IcDialogInfo);
            //        alert.Show();
            //    }
            //}
            //_encryptionKeyHelper.GenerateKeys();
            //android keystore certificate Sign Code end

            //SharedPreferences certificate Sign Code begin
            RSAKeyPairGenerator KPG = new RSAKeyPairGenerator(this.ApplicationContext);
            await Task.Factory.StartNew(() =>
            {
                DateTime ExpireDate = KPG.GetExpireDate();
                if (KPG.IsExpired())
                {
                    KPG.GenerateKeys();
                }
                String pubKeyxml = CipherHelper.RSAPublicKeyJava2DotNet(KPG.GetPublicKeyString());
                String priKeyxml = CipherHelper.RSAPrivateKeyJava2DotNet(KPG.GetPrivateKeyString());

                //設定明文資料
                Messager.loanApplication_customer cust = new Messager.loanApplication_customer();
                cust.pk = 20;
                cust.order_nbr = "00000016";
                cust.country = "TWN";
                cust.seq = 2;
                cust.customer_type = "第三人(擔保人)";
                cust.nickname = "王勝達";

                Messager.loanApplication_customer cust1 = new Messager.loanApplication_customer();
                cust1.pk = 16;
                cust1.order_nbr = "00000152";
                cust1.country = "TWN";
                cust1.seq = 3;
                cust1.customer_type = "第三人(擔保人)";
                cust1.nickname = "廖一民";

                List<Messager.loanApplication_customer> lstCust = new List<Messager.loanApplication_customer>();
                lstCust.Add(cust);
                lstCust.Add(cust1);
                string plaintxt = JsonConvert.SerializeObject(lstCust);
                string ciphertxt = DecEncCode.AESEncrypt(plaintxt);

                //簽章
                string sign = CipherHelper.RsaSign(plaintxt, priKeyxml);
                //驗章
                bool IsOK = CipherHelper.RsaVerifyData(plaintxt, sign, pubKeyxml);
                ////加密
                //string ciphertxt1 = CipherHelper.RSAEncrypt(pubKeyxml, "hello world!");
                ////解密
                //string plaintxt1 = CipherHelper.RSADecrypt(priKeyxml, ciphertxt);

                //Use MQ傳送明文,簽章,公鑰給server begin
                Messager.SignMessage sm = new Messager.SignMessage();
                sm.plainText = plaintxt;
                sm.cipherText = ciphertxt;
                sm.sign = sign;
                sm.publickey = pubKeyxml;
                string jsonSM = JsonConvert.SerializeObject(sm);
                List<Dictionary<string, string>> DicMapList = Common.Utility.Util.ToMessageMapForJson(jsonSM);
                List<List<MessageField>> MultiMQMessage = new List<List<MessageField>>();
                foreach (Dictionary<string, string> DicMap in DicMapList)
                {
                    List<MessageField> MqMessage = new List<MessageField>();
                    //加入資料序號
                    MessageField MessageSeqenceField = new MessageField();
                    MessageSeqenceField.Name = "9999";
                    MessageSeqenceField.Value = "1";
                    MqMessage.Add(MessageSeqenceField);
                    //加入資料序號
                    foreach (string Dic in DicMap.Keys)
                    {
                        MessageField MessageField = new MessageField();
                        MessageField.Name = Dic;
                        MessageField.Value = DicMap[Dic];
                        MqMessage.Add(MessageField);
                    }
                    MultiMQMessage.Add(MqMessage);
                }
                IMQAdapter MQ;
                MQ = TopicMQFactory.GetMQAdapterInstance(MQAdapterType.BatchMQAdapter);
                MQ.Uri = Config.MQ_network + ":" + Config.MQ_service;
                MQ.DestinationFeature = DestinationFeature.VirtualTopic;
                MQ.MessageTimeOut = ((double)1 / 24) * ((double)1 / 60);
                MQ.SendName = "leon08162001@gmail.com.WebReq";
                MQ.UserName = DecEncCode.AESDecrypt(Config.MQUserID);
                MQ.PassWord = DecEncCode.AESDecrypt(Config.MQPwd);
                MQ.UseSSL = Config.MQ_useSSL;
                MQ.Start(Android.OS.Build.Serial);
                MQ.SendMQMessage("710", MultiMQMessage, 4, 25);
                //Use MQ傳送明文,簽章,公鑰給server end
                //SharedPreferences certificate Sign Code begin
            }).ConfigureAwait(false);
        }
    }
}