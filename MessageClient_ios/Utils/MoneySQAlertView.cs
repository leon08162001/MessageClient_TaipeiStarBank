using System;
using UIKit;
using Foundation;
using CoreGraphics;
using System.Collections.Generic;

namespace Util
{
	/// <summary>
	/// 彈窗類型
	/// </summary>
	public enum AlertType
	{ 
		AlertIconWarn,
		AlertIconError,
		AlertIconSuccess,
		AlertIconQuery,
		AlertIconNone
	}

	public class MoneySQAlertView
	{
		private static Stack<UIWindow> _windowList = new Stack<UIWindow>();
		private static UIWindow _lastWindow;

		private static List<UIViewController> _childViewControllers = new List<UIViewController>();

		/// <summary>
		/// 顯示帶取消按鈕的彈窗
		/// </summary>
		public static void showAlert(string title, string content, AlertType type, Action confirmCallback, Action cancelCallback)
		{
			var contentView = createContentView();
			contentView.Bounds = new CGRect(0, 0, 405, 226f);

			var topView = createTopView(contentView);

			//标题
			var titleLabel = createTitleLabel(title);
			topView.AddSubview(titleLabel);
			titleLabel.EqualSuperLeft(18).EqualSuperCenterY();

			//关闭按钮
			//var closeBtn = createCloseButton();
			//topView.AddSubview(closeBtn);
			//closeBtn.EqualWidth(30).EqualHeight(30).EqualSuperCenterY().EqualSuperRight(-18);

			var middleView = createMiddleView();
			contentView.AddSubview(middleView);

			//内容
			var contentLabel = createContentLabel(content);
			middleView.AddSubview(contentLabel);

			if (type == AlertType.AlertIconNone)
			{
				contentLabel.PreferredMaxLayoutWidth = contentView.Bounds.Width - 36;
				contentLabel.EqualSuperCenterX().EqualSuperCenterY();
			}
			else
			{
				var imageView = createImageView(type);
				middleView.AddSubview(imageView);

				imageView.EqualWidth(50).EqualHeight(50).EqualSuperCenterY().EqualViewLeft(contentLabel, -28);
				contentLabel.PreferredMaxLayoutWidth = contentView.Bounds.Width - 120;
				contentLabel.EqualSuperCenterX(39).EqualViewCenterY(imageView);
			}

			var bottomView = createBottomView();
			contentView.AddSubview(bottomView);
			bottomView.BackgroundColor = MoneySQStyle.color999999;
			bottomView.EqualSuperLeft(0).EqualSuperRight(0).EqualSuperBottom(0).EqualHeight(49f);
			middleView.EqualSuperLeft(0).EqualViewBottom(topView, 0.5f).EqualSuperRight(0).EqualViewTop(bottomView, -0.5f);

			//取消按钮
			var cancelBtn = createCancelButton(cancelCallback);
			bottomView.AddSubview(cancelBtn);
			cancelBtn.EqualSuperLeft(0).EqualSuperTop(0).EqualSuperBottom(0);

			//确认按钮
			var confirmBtn = createConfirmButton(confirmCallback);
			bottomView.AddSubview(confirmBtn);
			confirmBtn.EqualViewRight(cancelBtn, 0.5f).EqualSuperTop(0).EqualSuperRight(0).EqualSuperBottom(0).EqualWidthToView(cancelBtn, 1.0f, 0);

			showCustomView(contentView);
		}
		/// <summary>
		/// 顯示Alert
		/// </summary>
		/// <param name="title">標題</param>
		/// <param name="content">內容</param>
		/// <param name="type">彈框類型</param>
		/// <param name="callback">點擊確認後的操作，可以為null</param>
		public static void showAlert(string title, string content, AlertType type, Action callback)
		{
			var contentView = createContentView();

			var topView = createTopView(contentView);

			//标题
			var titleLabel = createTitleLabel(title);
			topView.AddSubview(titleLabel);
			titleLabel.EqualSuperLeft(18).EqualSuperCenterY();

			//关闭按钮
			//var closeBtn = createCloseButton();
			//topView.AddSubview(closeBtn);
			//closeBtn.EqualWidth(30).EqualHeight(30).EqualSuperCenterY().EqualSuperRight(-18);
		
			var middleView = createMiddleView();
			contentView.AddSubview(middleView);

			var scrollView = new UIScrollView();
			middleView.AddSubview(scrollView);

			//内容
			var contentLabel = createContentLabel(content);
			scrollView.AddSubview(contentLabel);

			var bottomView = createBottomView();
			contentView.AddSubview(bottomView);
			bottomView.EqualSuperLeft(0).EqualSuperRight(0).EqualSuperBottom(0).EqualHeight(49);
			middleView.EqualSuperLeft(0).EqualViewBottom(topView, 0.5f).EqualSuperRight(0).EqualViewTop(bottomView, -0.5f);

			//确认按钮
			var confirmBtn = createConfirmButton(callback);
			bottomView.AddSubview(confirmBtn);
			confirmBtn.EqualSuperLeft(0).EqualSuperTop(0).EqualSuperRight(0).EqualSuperBottom(0);
		
			if (type == AlertType.AlertIconNone)
			{
				contentLabel.PreferredMaxLayoutWidth = contentView.Bounds.Width - 36;
				contentLabel.EqualSuperCenterY().EqualSuperCenterX();
				scrollView.EqualSuperTop(0).EqualSuperLeft(0).EqualSuperBottom(0).EqualSuperRight(0);
			}
			else
			{
				var imageView = createImageView(type);
				middleView.AddSubview(imageView);
				float maxWidth = (float)(contentView.Bounds.Width - 150);
				imageView.EqualWidth(50).EqualHeight(50).EqualSuperCenterY().EqualViewLeft(scrollView, -28);
				scrollView.EqualWidth(maxWidth).EqualSuperRight(0).EqualSuperTop(0).EqualSuperBottom(0);
				contentLabel.PreferredMaxLayoutWidth = maxWidth - 18;
				contentLabel.EqualSuperLeft(0).EqualSuperCenterY();
			}
			contentLabel.SizeToFit();
			Console.WriteLine(contentLabel.Frame.Height);
			scrollView.ContentSize = new CGSize(contentLabel.Bounds.Size);
			if (contentLabel.Bounds.Height - (contentView.Bounds.Height - 99) > UIScreen.MainScreen.Bounds.Height - 40)
			{
				contentView.Bounds = new CGRect(0, 0, contentView.Bounds.Width, UIScreen.MainScreen.Bounds.Height - 40);
			}
			else if (contentLabel.Bounds.Height  < (contentView.Bounds.Height - 99))
			{
				contentView.Bounds = new CGRect(0, 0, contentView.Bounds.Width, contentView.Bounds.Height);
			}
			else
			{
				contentView.Bounds = new CGRect(0, 0, contentView.Bounds.Width,  + contentLabel.Bounds.Height + 99);
			}
			showCustomView(contentView);
		}
        //public static void showAlert(string title, string content, UIAlertControllerStyle type, Action callback)
        //{
        //    UIAlertController Alert = UIAlertController.Create(title, content, type);
        //    // Add Actions
        //    Alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, (action) => callback));
        //    // Required for iPad - You must specify a source for the Action Sheet since it is
        //    // displayed as a popover
        //    UIPopoverPresentationController presentationPopover = Alert.PopoverPresentationController;
        //    if (presentationPopover != null)
        //    {
        //        presentationPopover.SourceView = this.View;
        //        presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
        //    }
        //    // Display the alert
        //    this.PresentViewController(Alert, true, null);
        //}
            /// <summary>
            /// 顯示密碼驗證彈窗
            /// </summary>
            public static void showPasswordAlert(Action<string> callback)
		{ 
			var contentView = createContentView();

			var topView = createTopView(contentView);

			//标题
			var titleLabel = createTitleLabel("密碼驗證");
			topView.AddSubview(titleLabel);
			titleLabel.EqualSuperLeft(18).EqualSuperCenterY();

			var middleView = createMiddleView();
			contentView.AddSubview(middleView);

			var hintLabel = createTitleLabel("請輸入密碼");
			middleView.AddSubview(hintLabel);

			var passwordField = createTextField();
			passwordField.SecureTextEntry = true;
			middleView.AddSubview(passwordField);

			passwordField.EqualWidth(280).EqualHeight(33).EqualSuperCenterY().EqualViewRight(hintLabel, 15);
			hintLabel.EqualSuperCenterX(-165).EqualViewCenterY(passwordField);

			var bottomView = createBottomView();
			contentView.AddSubview(bottomView);
			bottomView.EqualSuperLeft(0).EqualSuperRight(0).EqualSuperBottom(0).EqualHeight(49);
			middleView.EqualSuperLeft(0).EqualViewBottom(topView, 0.5f).EqualSuperRight(0).EqualViewTop(bottomView, -0.5f);

			//确认按钮
			var confirmBtn = createConfirmButton(callback);
			bottomView.AddSubview(confirmBtn);
			confirmBtn.EqualSuperLeft(0).EqualSuperTop(0).EqualSuperRight(0).EqualSuperBottom(0);

			showCustomView(contentView);
		}

		/// <summary>
		/// 顯示存儲建議書彈窗
		/// </summary>
		public static void showSaveProposalAlert(Action<string> callback)
		{ 
			var contentView = createContentView();

			var topView = createTopView(contentView);

			//标题
			var titleLabel = createTitleLabel("儲存建議書");
			topView.AddSubview(titleLabel);
			titleLabel.EqualSuperLeft(18).EqualSuperCenterY();

			var middleView = createMiddleView();
			contentView.AddSubview(middleView);

			var hintLabel = createTitleLabel("請輸入建議書編號");
			middleView.AddSubview(hintLabel);

			var contentField = createTextField();
			middleView.AddSubview(contentField);

			contentField.EqualWidth(280).EqualHeight(33).EqualSuperCenterY().EqualViewRight(hintLabel, 15);
			hintLabel.EqualSuperCenterX(-148).EqualViewCenterY(contentField);

			var bottomView = createBottomView();
			contentView.AddSubview(bottomView);
			bottomView.BackgroundColor = MoneySQStyle.color999999;
			bottomView.EqualSuperLeft(0).EqualSuperRight(0).EqualSuperBottom(0).EqualHeight(49f);
			middleView.EqualSuperLeft(0).EqualViewBottom(topView, 0.5f).EqualSuperRight(0).EqualViewTop(bottomView, -0.5f);

			//取消按钮
			var cancelBtn = createCancelButton(null);
			bottomView.AddSubview(cancelBtn);
			cancelBtn.EqualSuperLeft(0).EqualSuperTop(0).EqualSuperBottom(0);

			//确认按钮
			var confirmBtn = createConfirmButton(callback);
			bottomView.AddSubview(confirmBtn);
			confirmBtn.EqualViewRight(cancelBtn, 0.5f).EqualSuperTop(0).EqualSuperRight(0).EqualSuperBottom(0).EqualWidthToView(cancelBtn, 1.0f, 0);

			showCustomView(contentView);
		}

		/// <summary>
		/// 顯示客制視圖
		/// </summary>
		/// <returns>The custom view.</returns>
		/// <param name="view">View.</param>
		public static void showCustomView(UIView view)
		{
			//獲取window
			_lastWindow = UIApplication.SharedApplication.Delegate.GetWindow();
			UIWindow currentWindow = new UIWindow(UIScreen.MainScreen.Bounds);
			currentWindow.WindowLevel = UIWindowLevel.Alert;

			UIViewController contentViewController = new UIViewController();
			contentViewController.View.Frame = currentWindow.Bounds;
			contentViewController.View.BackgroundColor = UIColor.FromWhiteAlpha(0.1f, 0.3f);
			MoneySQiOSHelper.hideKeyboard(contentViewController.View);
			view.Center = contentViewController.View.Center;

			contentViewController.View.AddSubview(view);
			currentWindow.RootViewController = contentViewController;
			currentWindow.MakeKeyAndVisible();
			_windowList.Push(currentWindow);
		}

		/// <summary>
		/// 從指定view顯示
		/// </summary>
		/// <returns>The custom view from view.</returns>
		public static void showCustomViewFromView(UIViewController viewController, UIView fromView)
		{
			//獲取window
			_lastWindow = UIApplication.SharedApplication.Delegate.GetWindow();
			UIWindow currentWindow = new UIWindow(UIScreen.MainScreen.Bounds);
			currentWindow.WindowLevel = UIWindowLevel.Normal;
			_childViewControllers.Add(viewController);
			UIViewController contentViewController = new UIViewController();
			contentViewController.View.Frame = currentWindow.Bounds;
			contentViewController.View.BackgroundColor = UIColor.FromWhiteAlpha(0.1f, 0.3f);

			CGRect frame = fromView.ConvertRectToView(fromView.Bounds, contentViewController.View);

			var imageView = new UIImageView(new CGRect(0, frame.GetMaxY() + 10, contentViewController.View.Bounds.Width, contentViewController.View.Bounds.Height - (frame.GetMaxY() + 10)));
			imageView.Image = drawArrowImage(imageView.Frame.Size, frame);
			contentViewController.View.AddSubview(imageView);

			viewController.View.Frame = new CGRect(0, frame.GetMaxY() + 20, contentViewController.View.Bounds.Width, contentViewController.View.Bounds.Height - (frame.GetMaxY() + 20));
			contentViewController.View.AddSubview(viewController.View);
			currentWindow.RootViewController = contentViewController;
			currentWindow.MakeKeyAndVisible();
			_windowList.Push(currentWindow);
		}

        public static void showMailViewFromView(UIViewController viewController, UIView fromView)
        {
            //獲取window
            _lastWindow = UIApplication.SharedApplication.Delegate.GetWindow();
            UIWindow currentWindow = new UIWindow(UIScreen.MainScreen.Bounds);
            currentWindow.WindowLevel = UIWindowLevel.Normal;
            _childViewControllers.Add(viewController);
            UIViewController contentViewController = new UIViewController();
            contentViewController.View.Frame = new CGRect(0,0,currentWindow.Frame.Width,currentWindow.Frame.Height);
            contentViewController.View.BackgroundColor = UIColor.Clear;

            //CGRect frame = fromView.ConvertRectToView(fromView.Bounds, contentViewController.View);

            //var imageView = new UIImageView(new CGRect(0, 10, contentViewController.View.Bounds.Width, contentViewController.View.Bounds.Height + 10));
            //imageView.Image = drawArrowImage(imageView.Frame.Size, frame);
            //imageView.BackgroundColor = UIColor.Yellow;
            //contentViewController.View.AddSubview(imageView);

            viewController.View.Frame = new CGRect(0, 0, contentViewController.View.Bounds.Width, contentViewController.View.Bounds.Height);
            contentViewController.View.AddSubview(viewController.View);
            currentWindow.RootViewController = contentViewController;
            currentWindow.BackgroundColor = UIColor.Clear;
            currentWindow.MakeKeyAndVisible();
            _windowList.Push(currentWindow);
        }

		private static UIImage drawArrowImage(CGSize size, CGRect frame)
		{
			UIGraphics.BeginImageContextWithOptions(size, false, 0);
			var ctx = UIGraphics.GetCurrentContext();
			var path = new CGPath();
			path.MoveToPoint(0, 10);
			path.AddLineToPoint(0, size.Height);
			path.AddLineToPoint(size.Width, size.Height);
			path.AddLineToPoint(size.Width, 10);
			path.AddLineToPoint(frame.GetMidX() + 10, 10);
			path.AddLineToPoint(frame.GetMidX(), 0);
			path.AddLineToPoint(frame.GetMidX() - 10, 10);
			ctx.AddPath(path);
			ctx.SetFillColor(UIColor.White.CGColor);
			ctx.FillPath();
			UIImage image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
			return image;
		}

		private static UIView createContentView()
		{ 
			var contentView = new UIView();
			contentView.Bounds = new CGRect(0, 0, 575, 226);
			contentView.BackgroundColor = MoneySQStyle.color999999;
			contentView.Layer.MasksToBounds = true;
			contentView.Layer.CornerRadius = 6;
			return contentView;
		}

		private static UIView createTopView(UIView contentView)
		{ 
			var topView = new UIView();
			contentView.AddSubview(topView);
			topView.BackgroundColor = MoneySQStyle.colorF7F7F7;
			topView.EqualSuperLeft(90).EqualSuperRight(0).EqualSuperTop(0).EqualHeight(50);
			return topView;
		}

		private static UIView createMiddleView()
		{ 
			var middleView = new UIView();
			middleView.BackgroundColor = MoneySQStyle.colorFFFFFF;
			return middleView;
		}

		private static UIView createBottomView()
		{
			var bottomView = new UIView();
			bottomView.BackgroundColor = MoneySQStyle.colorFFFFFF;
			return bottomView;
		}

		private static UILabel createTitleLabel(string title)
		{
			//标题
			var titleLabel = new UILabel();
			titleLabel.Text = title;
			titleLabel.Font = MoneySQStyle.boldFont18;
			titleLabel.TextColor = MoneySQStyle.color000000;
			return titleLabel;
		}

		private static UIButton createCloseButton()
		{
			//关闭按钮
			var closeBtn = UIButton.FromType(UIButtonType.Custom);
			closeBtn.BackgroundColor = UIColor.Yellow;
			closeBtn.TouchUpInside += (sender, e) =>
			{
				dismiss();
			};
			return closeBtn;
		}

		private static UILabel createContentLabel(string content)
		{
			//内容
			var contentLabel = new UILabel();
			contentLabel.Text = content;
			contentLabel.Font = MoneySQStyle.boldFont18;
			contentLabel.TextColor = MoneySQStyle.color000000;
			contentLabel.Lines = 0;
			//NSMutableParagraphStyle style = new NSMutableParagraphStyle();
			//style.LineSpacing = 10;
			//UIStringAttributes attrs = new UIStringAttributes
			//{
			//	ParagraphStyle = style
			//};
			//NSAttributedString attrString = new NSAttributedString(content, attrs);
			//contentLabel.AttributedText = attrString;
			return contentLabel;
		}

		private static BorderTextField createTextField()
		{
			var textField = new BorderTextField();
			textField.Tag = 999;
			textField.KeyboardType = UIKeyboardType.ASCIICapable;

			return textField;
		}

		private static UIButton createConfirmButton(Action callback)
		{
			//确认按钮
			var confirmBtn = UIButton.FromType(UIButtonType.Custom);
			confirmBtn.BackgroundColor = MoneySQStyle.colorFFFFFF;
			confirmBtn.SetTitle("確認", UIControlState.Normal);
			confirmBtn.SetTitleColor(MoneySQStyle.color000000, UIControlState.Normal);
			confirmBtn.TitleLabel.Font = MoneySQStyle.boldFont18;
			confirmBtn.TouchUpInside += (sender, e) =>
			{
				dismiss();
				if (callback != null)
				{
					callback();
				}
			};
			return confirmBtn;
		}

		private static UIButton createConfirmButton(Action<string> callback)
		{
			//确认按钮
			var confirmBtn = UIButton.FromType(UIButtonType.Custom);
			confirmBtn.BackgroundColor = MoneySQStyle.colorFFFFFF;
			confirmBtn.SetTitle("確認", UIControlState.Normal);
			confirmBtn.SetTitleColor(MoneySQStyle.color000000, UIControlState.Normal);
			confirmBtn.TitleLabel.Font = MoneySQStyle.boldFont18;
			confirmBtn.TouchUpInside += (sender, e) =>
			{
				dismiss();
				if (callback != null)
				{
					UITextField pwdField = (UITextField)confirmBtn.Superview.Superview.ViewWithTag(999);
					callback(pwdField.Text);
				}
			};
			return confirmBtn;
		}

		private static UIButton createCancelButton(Action callback)
		{
			//取消按钮
			var cancelBtn = UIButton.FromType(UIButtonType.Custom);
			cancelBtn.BackgroundColor = MoneySQStyle.colorFFFFFF;
			cancelBtn.SetTitle("取消", UIControlState.Normal);
			cancelBtn.SetTitleColor(MoneySQStyle.color000000, UIControlState.Normal);
			cancelBtn.TitleLabel.Font = MoneySQStyle.boldFont18;
			cancelBtn.TouchUpInside += (sender, e) =>
			{
				dismiss();
				if (callback != null)
				{
					callback();
				}
			};
			return cancelBtn;
		}

		private static UIImageView createImageView(AlertType type)
		{
			string imageName = null;
			switch (type)
			{
				case AlertType.AlertIconError:
					imageName = "assets/ic_alert_error.png";
					break;
				case AlertType.AlertIconWarn:
					imageName = "assets/ic_alert_warn.png";
					break;
				case AlertType.AlertIconQuery:
					imageName = "assets/ic_alert_query.png";
					break;
				case AlertType.AlertIconSuccess:
					imageName = "assets/ic_alert_success.png";
					break;
			}
			var imageView = new UIImageView();
			imageView.Image = UIImage.FromBundle(imageName);
			return imageView;
		}

		/// <summary>
		/// 從指定viewController顯示
		/// </summary>
		/// <returns>The custom view from view.</returns>
		public static void showCustomView(UIViewController viewController)
		{
			_childViewControllers.Add(viewController);
			showCustomView(viewController.View);
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
				_childViewControllers.Clear();
				_lastWindow.MakeKeyAndVisible();
			}
		}
	}
}

