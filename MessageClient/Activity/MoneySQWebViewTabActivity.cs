using Android.App;
using Android.OS;
using Android.Webkit;
using System;

namespace MessageClient
{
    [Activity(Label = "MoneySQWebViewTabActivity")]
    public class MoneySQWebViewTabActivity : BaseActivity
    {
        public static string WebUrl { get; set; }
        //[AndroidView]
        private WebView WebView1;
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
            WebView1.Settings.LoadWithOverviewMode = true;
            WebView1.Settings.BuiltInZoomControls = true;
            WebView1.Settings.DisplayZoomControls = true;
            //載入網址
            if (string.IsNullOrEmpty(MoneySQWebViewTabActivity.WebUrl))
            {
                MoneySQWebViewTabActivity.WebUrl = "https://www.google.com.tw/";
            }
            WebView1.LoadUrl(MoneySQWebViewTabActivity.WebUrl);
            // 請注意這行，如果不加入巢狀Class 會必成呼叫系統讓系統來裁決開啟http 的方式
            WebView1.SetWebViewClient(new CustWebViewClient());
        }
        protected override void OnDestroy()
        {
            WebView1.DestroyDrawingCache();
            WebView1.Destroy();
            GC.Collect(0);
            base.OnDestroy();
        }
        /// <summary>
        /// 巢狀Class 繼承WebViewClient
        /// </summary>
        private class CustWebViewClient : WebViewClient
        {
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                view.LoadUrl(url);
                return true;
            }

        }
    }
}