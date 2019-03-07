//******************************************************************
//*  author：榮剛
//*  Function：带下划线的按钮
//*  Create Date：2016.09.08
//*  Modify Record：()
//*  <author>           <time>          <TaskID>                <desc>
//*    榮剛            2016.09.08          N/A                    
//*******************************************************************
using Foundation;
using System;
using CoreGraphics;
using UIKit;

namespace MessageClient_ios.Util
{
    public partial class UnderLineButton : UIButton
    {
        public UnderLineButton (IntPtr handle) : base (handle)
        {
        }

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);

			var textRect = this.TitleLabel.Frame;
			var ctx = UIGraphics.GetCurrentContext();
			nfloat descender = this.TitleLabel.Font.Descender;
			ctx.SetStrokeColor(this.TitleColor(UIControlState.Normal).CGColor);
			ctx.MoveTo(textRect.X, textRect.Y + textRect.Height + descender + 2);
			ctx.AddLineToPoint(textRect.X + textRect.Width, textRect.Y + textRect.Height + descender + 2);
			ctx.ClosePath();
			ctx.DrawPath(CGPathDrawingMode.Stroke);
		}
    }
}