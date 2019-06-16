using Android.App;
using Android.OS;
using Android.Webkit;
using Common;

namespace MessageClient
{
    [Activity(Label = "WebChatTabActivity")]
    public class WebChatTabActivity : MoneySQWebViewTabActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            WebUrl = Config.WebChatUrl;
            WebView1.LoadUrl(WebUrl);
        }
    }
}