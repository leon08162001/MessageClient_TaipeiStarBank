using Android.App;
using Android.Content;
using Android.OS;
using Android.Webkit;
using Common;
using System;

namespace MessageClient
{
    [Activity(Label = "MoneySQWebViewTabActivity")]
    public class MoneySQWebViewTabActivity : BaseActivity
    {
        private Action<int, Result, Intent> resultCallbackvalue;
        public static string WebUrl { get; set; }

        //[AndroidView]
        protected WebView WebView1;
        public MoneySQWebViewTabActivity()
        {
            WebUrl = Config.moneysqWebSite;
        }
        public void StartActivity(Intent intent, int requestCode, Action<int, Result, Intent> resultCallback)
        {
            this.resultCallbackvalue = resultCallback;
            StartActivityForResult(intent, requestCode);
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.MoneySQWebView);
            WebView1 = FindViewById<WebView>(Resource.Id.WebView1);

            //啟用Javascript Enable
            WebView1.Settings.JavaScriptEnabled = true;
            WebView1.Settings.UseWideViewPort = true;
            WebView1.Settings.AllowFileAccess = true;
            WebView1.Settings.LoadWithOverviewMode = true;
            WebView1.Settings.BuiltInZoomControls = true;
            WebView1.Settings.DisplayZoomControls = true;
            WebView1.Settings.DomStorageEnabled = true;
            WebView1.Settings.CacheMode = CacheModes.CacheElseNetwork;
            WebView1.Settings.SetAppCacheEnabled(true);
            WebView1.SetWebChromeClient(new CustomWebChromeClient(this));
            WebView1.LoadUrl(WebUrl);
            // 請注意這行，如果不加入巢狀Class 會必成呼叫系統讓系統來裁決開啟http 的方式
            WebView1.SetWebViewClient(new CustWebViewClient());
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (this.resultCallbackvalue != null)
            {
                this.resultCallbackvalue(requestCode, resultCode, data);
                this.resultCallbackvalue = null;
            }
        }
        protected override void OnDestroy()
        {
            WebView1.DestroyDrawingCache();
            WebView1.Destroy();
            GC.Collect(0);
            base.OnDestroy();
        }
    }
    /// <summary>
    /// 巢狀Class 繼承WebViewClient
    /// </summary>
    public class CustWebViewClient : WebViewClient
    {
        public override bool ShouldOverrideUrlLoading(WebView view, string url)
        {
            view.LoadUrl(url);
            return true;
        }
        public override void OnReceivedSslError(WebView view, SslErrorHandler handler, Android.Net.Http.SslError error)
        {
            handler.Proceed();
        }
    }
    public class CustomWebChromeClient : WebChromeClient
    {
        private static int filechooser = 2;
        private IValueCallback message;
        private MoneySQWebViewTabActivity activity = null;
        public CustomWebChromeClient(MoneySQWebViewTabActivity context)
        {
            this.activity = context;
        }
        public override bool OnShowFileChooser(WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
        {
            this.message = filePathCallback;
            Intent chooserIntent = fileChooserParams.CreateIntent();
            chooserIntent.AddCategory(Intent.CategoryOpenable);
            this.activity.StartActivity(Intent.CreateChooser(chooserIntent, "File Chooser"), filechooser, this.OnActivityResult);
            return true;
        }
        private void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (data != null)
            {
                if (requestCode == filechooser)
                {
                    if (null == this.message)
                    {
                        //enter code here
                        return;
                    }

                    this.message.OnReceiveValue(WebChromeClient.FileChooserParams.ParseResult((int)resultCode, data));
                    this.message = null;
                }
            }
        }
    }
}