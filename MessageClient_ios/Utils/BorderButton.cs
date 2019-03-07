using Foundation;
using System;
using UIKit;
using CoreGraphics;

namespace Util
{
    public partial class BorderButton : UIButton
    {
        public BorderButton (IntPtr handle) : base (handle)
        {
        }

		public BorderButton()
		{
			CommonInit();
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();
			CommonInit();
		}

		private void CommonInit()
		{
			UIColor whiteColor = MoneySQStyle.colorFFFFFF;
			UIColor blueColor = MoneySQStyle.color2B89AC;
			CGSize size = this.Bounds.Size;
			//設置按鈕背景顔色
			this.SetBackgroundImage(MoneySQiOSHelper.getImageFromColor(whiteColor, size), UIControlState.Normal);
			this.SetBackgroundImage(MoneySQiOSHelper.getImageFromColor(blueColor, size), UIControlState.Selected);
			this.SetBackgroundImage(MoneySQiOSHelper.getImageFromColor(blueColor, size), UIControlState.Highlighted);
			//設置按鈕文字顔色
			this.SetTitleColor(blueColor, UIControlState.Normal);
			this.SetTitleColor(whiteColor, UIControlState.Selected);
			//設置按鈕邊框
			this.Layer.BorderWidth = 1.0f;
			this.Layer.BorderColor = MoneySQStyle.color9E9E9E.CGColor;
		}

		public override bool Highlighted
		{
			get
			{
				return base.Highlighted;
			}
			set
			{
				
			}
		}
    }
}