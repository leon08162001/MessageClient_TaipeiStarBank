using Android.App;
using Android.OS;
using Android.Webkit;
using Common;

namespace MessageClient
{
    [Activity(Label = "WebChatTabActivity")]
    public class WebChatTabActivity : MoneySQWebViewTabActivity
    {
        public WebChatTabActivity()
        {
            WebUrl = Config.WebChatUrl;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //WebView1.LoadUrl(WebUrl);
        }
    }
}