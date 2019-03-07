using UIKit;

namespace MessageClient_ios.Util
{
    public class RoundedView : UIView
    {
        public RoundedView(float radius = 8)
        {
            Layer.CornerRadius = radius;   
        }
        public void ScaleBackImage(string BundlePath)
        {
            UIImage img = UIImage.FromBundle(BundlePath);
            img = img.Scale(UIScreen.MainScreen.Bounds.Size);
            this.BackgroundColor = UIColor.FromPatternImage(img);
        }
    }
}