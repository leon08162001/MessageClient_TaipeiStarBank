//******************************************************************
//*  author：榮剛
//*  Function：带边框的UITextField
//*  Create Date：2016.09.08
//*  Modify Record：()
//*  <author>           <time>          <TaskID>                <desc>
//*    榮剛            2016.09.08          N/A                    
//*******************************************************************
using Foundation;
using System;
using CoreGraphics;
using UIKit;

namespace Util
{
    public partial class BorderTextField : UITextField
    {
        public BorderTextField (IntPtr handle) : base (handle)
        {
        }

		public BorderTextField()
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
			this.Layer.BorderWidth = 0.5f;
			this.Layer.BorderColor = MoneySQStyle.color58595B.CGColor;
			this.TextColor = MoneySQStyle.color2B89AC;
		}

		public override CGRect TextRect(CGRect forBounds)
		{
			return new CGRect(forBounds.X + 10, forBounds.Y, forBounds.Width - 10, forBounds.Height);
		}

		public override CGRect EditingRect(CGRect forBounds)
		{
			return new CGRect(forBounds.X + 10, forBounds.Y, forBounds.Width - 10, forBounds.Height);
		}

		public override CGRect PlaceholderRect(CGRect forBounds)
		{
			return new CGRect(forBounds.X + 10, forBounds.Y, forBounds.Width - 10, forBounds.Height);
		}
    }
}