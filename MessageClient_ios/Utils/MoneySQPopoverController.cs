using System;
using UIKit;
using Foundation;
using CoreGraphics;
using ObjCRuntime;

namespace Util
{
	public class MoneySQPopoverController : NSObject
	{
		private CGSize _popoverContentSize = new CGSize(320, 300);
		public CGSize PopoverContentSize 
		{ 
			get
			{
				return _popoverContentSize;
			}
			set
			{
				_popoverContentSize = value;
			}
		}

		private UIViewController _viewController;
		private UIView _backgroudView;
		private UIView _contentView;
		private UIWindow _window = UIApplication.SharedApplication.Delegate.GetWindow();

		// Constructor
		public MoneySQPopoverController(UIViewController viewController)
		{
			_viewController = viewController;
			_backgroudView = new UIView(UIScreen.MainScreen.Bounds);
			_backgroudView.BackgroundColor = UIColor.Clear;
			UITapGestureRecognizer tap = new UITapGestureRecognizer(this, new Selector("handleTap"));
			_backgroudView.AddGestureRecognizer(tap);
			_window.AddSubview(_backgroudView);

			_contentView = new UIView();
			_contentView.BackgroundColor = UIColor.Clear;
			_backgroudView.AddSubview(_contentView);
		}

		public void presentPopover(CGRect rect, UIView view)
		{
			CGRect newRect = view.ConvertRectToView(view.Bounds, _window);
			_contentView.Frame = new CGRect(newRect.GetMidX() - _popoverContentSize.Width / 2, newRect.GetMaxY() + 5, _popoverContentSize.Width, _popoverContentSize.Height);
			_viewController.View.Frame = _contentView.Bounds;
			_contentView.AddSubview(_viewController.View);
		}

		#region "tap"
		[Export("handleTap")]
		void handleTap()
		{
			dismiss();
		}
		#endregion

		void dismiss()
		{
			_backgroudView.RemoveFromSuperview();
		}
	}
}

