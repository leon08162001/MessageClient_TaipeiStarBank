//******************************************************************
//*  author：榮剛
//*  Function：鍵盤管理類
//*  Create Date：2016.12.02
//*  Modify Record：()
//*  <author>           <time>          <TaskID>                <desc>
//*    榮剛            2016.12.02          N/A                    
//*******************************************************************
using System;
using UIKit;
using Foundation;
using CoreGraphics;

namespace Util
{
	public class KeyboardManager : NSObject
	{
		public UITextField CurrentTextField { get; set; }

		private KeyboardManager() { }
		static KeyboardManager() { }
		private static readonly KeyboardManager _instance = new KeyboardManager();
		public static KeyboardManager Intance
		{ 
			get
			{
				return _instance;
			}
		}

		/// <summary>
		/// 開始監控鍵盤
		/// </summary>
		public void startMonitor()
		{
			//處理鍵盤被遮擋
			NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("keyboardWillShowNotification:"), UIKeyboard.WillShowNotification, null);
			NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("keyboardWillHideNotification:"), UIKeyboard.WillHideNotification, null);
			NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("textDidBeginEditingNotification:"), UITextField.TextDidBeginEditingNotification, null);
			NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("textDidEndEditingNotification:"), UITextField.TextDidEndEditingNotification, null);
		}
		/// <summary>
		/// 停止監控鍵盤
		/// </summary>
		public void stopMonitor()
		{ 
			NSNotificationCenter.DefaultCenter.RemoveObserver(this);
		}

		#region 處理鍵盤被遮擋
		[Export("keyboardWillShowNotification:")]
		void keyboardWillShowNotification(NSNotification noti)
		{
			NSValue endFrameValue = (NSValue)noti.UserInfo.ValueForKey(UIKeyboard.FrameEndUserInfoKey);
			Console.WriteLine(endFrameValue);
			nfloat kbHeight = endFrameValue.CGRectValue.Height;
			if (CurrentTextField == null) { return; }
			UIWindow window = UIApplication.SharedApplication.KeyWindow;
			CGRect rect = CurrentTextField.ConvertRectToView(CurrentTextField.Bounds, window);
			double offset = rect.GetMaxY() - (window.Frame.Height - kbHeight - 55.0);
			if (offset > 0)
			{
				UIView.Animate(0.25, () =>
				{
					window.Frame = new CGRect(0, -offset, window.Frame.Width, window.Frame.Height);
				});
			}
		}

		[Export("keyboardWillHideNotification:")]
		void keyboardWillHideNotification(NSNotification noti)
		{
			UIWindow window = UIApplication.SharedApplication.KeyWindow;
			UIView.Animate(0.25, () =>
			{
				window.Frame = new CGRect(0, 0, window.Frame.Width, window.Frame.Height);
			});
		}

		[Export("textDidBeginEditingNotification:")]
		void textDidBeginEditingNotification(NSNotification noti)
		{
			UITextField textField = noti.Object as UITextField;
			CurrentTextField = textField;
		}

		[Export("textDidEndEditingNotification:")]
		void textDidEndEditingNotification(NSNotification noti)
		{
			CurrentTextField = null;
		}

		#endregion

	}
}

