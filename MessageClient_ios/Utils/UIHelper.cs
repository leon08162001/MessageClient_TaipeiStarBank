using Foundation;
using UIKit;

namespace MessageClient_ios.Utils
{
    class UIHelper
    {
        /// <summary>
        /// 設定表單背景圖
        /// </summary>
        /// <param name="View"></param>
        /// <param name="ImagePath"></param>
        public static void SetViewBackgroundImage(UIView View, string ImagePath)
        {
            UIImage i = UIImage.FromFile(ImagePath);
            i = i.Scale(View.Frame.Size);
            View.BackgroundColor = UIColor.FromPatternImage(i);
        }
        public static bool LaunchApp(string uri)
        {
            var canOpen = UIApplication.SharedApplication.CanOpenUrl(new NSUrl(new System.Uri(uri).AbsoluteUri));

            if (!canOpen)
                return false;

            return UIApplication.SharedApplication.OpenUrl(new NSUrl(new System.Uri(uri).AbsoluteUri));
        }
    }
}