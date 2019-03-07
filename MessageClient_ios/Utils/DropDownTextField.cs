using Foundation;
using System;
using UIKit;

namespace Util
{
    public partial class DropDownTextField : UITextField
    {
        public DropDownTextField (IntPtr handle) : base (handle)
        {
        }

		public DropDownTextField()
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
			this.BorderStyle = UITextBorderStyle.RoundedRect;
			this.TextColor = MoneySQStyle.color2B89AC;

			this.RightView = new UIImageView(UIImage.FromBundle("assets/btn_dropright_n.png"));
			this.RightViewMode = UITextFieldViewMode.Always;
		}

		public void setRightImage(string imageName)
		{ 
			this.RightView = new UIImageView(UIImage.FromBundle("assets/" + imageName));
		}
    }
}