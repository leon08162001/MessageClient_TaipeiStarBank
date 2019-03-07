//******************************************************************
//*  author：榮剛
//*  Function：带边框的UIView
//*  Create Date：2016.09.08
//*  Modify Record：()
//*  <author>           <time>          <TaskID>                <desc>
//*    榮剛            2016.09.08          N/A                    
//*******************************************************************
using Foundation;
using System;
using UIKit;
using CoreGraphics;

namespace Util
{
    public partial class BorderView : UIView
    {
		public Action pShrinkButtonCallback;

        public BorderView (IntPtr handle) : base (handle)
        {
			
        }

		[Export("initWithFrame:")]
		public BorderView(CGRect frame) : base(frame)
		{ 
			commonInit();
		}

		public BorderView()
		{ 
			commonInit();
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();
			commonInit();
		}

		void commonInit()
		{ 
			this.Layer.BorderWidth = 0.5f;
			this.Layer.BorderColor = MoneySQStyle.colorB0B0B0.CGColor;
			this.Layer.MasksToBounds = true;
		}
    }
}