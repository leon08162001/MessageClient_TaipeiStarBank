using CoreGraphics;
using Foundation;
using MessageClient_ios.Util;
using System;
using System.Drawing;
using UIKit;

namespace MessageClient_ios
{
	public partial class HisMessageViewController : GesturesViewController
    {
		public HisMessageViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
            // Perform any additional setup after loading the view, typically from a nib.
            //designerScrollView.BackgroundColor = UIColor.Blue;
            //designerScrollView.ContentSize = new SizeF(1000, 1000);

            //ResizeHeigthWithText(Label1);
            Label1.BackgroundColor = UIColor.Red;

            //ScrollView.WidthAnchor.ConstraintEqualTo(400).Active = true;
            //ScrollView.FullSizeOf(View);
            ScrollView.BackgroundColor = UIColor.Green;
            //ScrollView.ContentSize = new SizeF(1000, 1000);
            ScrollView.ContentSize = Label1.Frame.Size;
        }

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

        public void ResizeHeigthWithText(UILabel label, float maxHeight = 960f)
        {
            float width = (float)label.Frame.Width;
            SizeF size = (SizeF)((NSString)label.Text).StringSize(label.Font, constrainedToSize: new SizeF(width, maxHeight),
                    lineBreakMode: UILineBreakMode.WordWrap);
            var labelFrame = label.Frame;
            labelFrame.Size = new SizeF(width, size.Height);
            label.Frame = labelFrame;
        }
    }
}

