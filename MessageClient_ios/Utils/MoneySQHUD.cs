using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;

namespace Util
{
	public enum HUDType
	{ 
		HUDTypeSuccess,
		HUDTypeFailure
	}

	public class MoneySQHUD
	{
        private static Stack<UIWindow> _windowList = new Stack<UIWindow>();
		private static UIWindow _lastWindow;
		private static UILabel _progressLabel;
		private static string _progressText;
		public static string progressText
		{
			set
			{
				_progressText = value;
				_progressLabel.Text = _progressText;
			}
			get
			{
				return _progressText;
			}
		}

		/// <summary>
		/// 顯示HUD
		/// </summary>
		public static void showHUD()
		{ 
			var contentView = new UIView();
			contentView.Bounds = new CGRect(0, 0, 175, 175);
			contentView.BackgroundColor = UIColor.FromWhiteAlpha(0.1f, 0.8f);
			contentView.Layer.MasksToBounds = true;
			contentView.Layer.CornerRadius = 6;

			var indicatorView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
			indicatorView.Transform = CGAffineTransform.MakeScale(2.0f, 2.0f);
			indicatorView.StartAnimating();
			contentView.AddSubview(indicatorView);
			indicatorView.EqualSuperCenterX().EqualSuperCenterY();
			showCustomHUD(contentView);
		}

		public static void showHUDWithSchedule()
		{
			var contentView = new UIView();
			contentView.Bounds = new CGRect(0, 0, 175, 175);
			contentView.BackgroundColor = UIColor.FromWhiteAlpha(0.1f, 0.8f);
			contentView.Layer.MasksToBounds = true;
			contentView.Layer.CornerRadius = 6;

			var indicatorView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
			indicatorView.Transform = CGAffineTransform.MakeScale(2.0f, 2.0f);
			indicatorView.StartAnimating();
			contentView.AddSubview(indicatorView);
			indicatorView.EqualSuperCenterX().EqualSuperCenterY(-10);

			var label = new UILabel();
			label.Font = MoneySQStyle.boldFont18;
			label.TextColor = MoneySQStyle.colorFFFFFF;
			label.TextAlignment = UITextAlignment.Center;
			label.Text = "已完成 1%";
			contentView.AddSubview(label);
			label.EqualWidth(100).EqualHeight(30).EqualSuperCenterX().EqualSuperCenterY(60);
			_progressLabel = label;
			showCustomHUD(contentView);
		
		}

		async public static void showHUDWithText(string text, HUDType type)
		{ 
			var contentView = new UIView();
			contentView.Bounds = new CGRect(0, 0, 175, 175);
			contentView.BackgroundColor = UIColor.FromWhiteAlpha(0.1f, 0.8f);
			contentView.Layer.MasksToBounds = true;
			contentView.Layer.CornerRadius = 6;

			//var indicatorView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
			//indicatorView.Transform = CGAffineTransform.MakeScale(2.0f, 2.0f);
			//indicatorView.StartAnimating();
			//contentView.AddSubview(indicatorView);
			//indicatorView.EqualSuperCenterX().EqualSuperCenterY();

			var imageView = new UIImageView();
			contentView.AddSubview(imageView);
			imageView.Image = UIImage.FromBundle("assets/ic_alert_success.png");
			imageView.EqualWidth(50).EqualHeight(50).EqualSuperCenterX().EqualSuperCenterY(-10);

			var label = new UILabel();
			label.Font = MoneySQStyle.regularFont15;
			label.TextColor = MoneySQStyle.colorFFFFFF;
			label.TextAlignment = UITextAlignment.Center;
			label.Text = text;
			contentView.AddSubview(label);
			label.EqualSuperCenterX().EqualSuperBottom(-20);

			showCustomHUD(contentView);
			await System.Threading.Tasks.Task.Delay(2000);
			dismiss();
		}

		/// <summary>
		/// 顯示客制視圖
		/// </summary>
		/// <returns>The custom view.</returns>
		/// <param name="view">View.</param>
		public static void showCustomHUD(UIView view)
		{
			//獲取window
			_lastWindow = UIApplication.SharedApplication.KeyWindow;
            UIWindow currentWindow = new UIWindow(UIScreen.MainScreen.Bounds);
			currentWindow.WindowLevel = UIWindowLevel.Alert;

			UIViewController contentViewController = new UIViewController();
			contentViewController.View.Frame = currentWindow.Bounds;
			contentViewController.View.BackgroundColor = UIColor.Clear;

			view.Center = contentViewController.View.Center;

			contentViewController.View.AddSubview(view);
			currentWindow.RootViewController = contentViewController;
			currentWindow.MakeKeyAndVisible();
            _windowList.Push(currentWindow);
		}

		/// <summary>
		/// 移除
		/// </summary>
		public static void dismiss()
		{
            if (_windowList != null && _windowList.Count > 0)
			{
				UIWindow window = _windowList.Pop();
                window.Hidden = true;
                window = null;
				_progressLabel = null;
				_progressText = null;
				_lastWindow.MakeKeyAndVisible();
			}
		}
	}
}

