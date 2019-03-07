//******************************************************************
//*  author：榮剛
//*  Function：工具類方法
//*  Create Date：2016.09.08
//*  Modify Record：()
//*  <author>           <time>          <TaskID>                <desc>
//*    榮剛            2016.09.08          N/A                    
//*******************************************************************
using System;
using System.Text.RegularExpressions;
using UIKit;
using CoreGraphics;
using Foundation;
using System.Collections.Generic;
using MessageClient_ios;
using MessageClient_ios.Util;

namespace Util
{
	public class MoneySQiOSHelper
	{
		/// <summary>
		/// 顯示彈出的選擇器.
		/// </summary>
		/// <returns></returns>
		/// <param name="view">從哪個View顯示</param>
		/// <param name="size">彈出框的大小</param>
		/// <param name="dataArray">顯示的內容</param>
        /// <param name="isAllowChange">是否允許修改年為公元年或民國年</param>
		/// <param name="selectedDelegate">選擇完成後執行的方法</param>
        public static void showPopPicker(UIView view, CGSize size, string[] dataArray,int rowIndex, bool isAllowChange, PickerStyle style, PickerSelectedDelegate selectedDelegate ,Action selectedSureDelegate)
		{
			var vc = new PickerViewController(style);
            vc.AllowTypeYearChange = isAllowChange;
			vc.pDataArray = dataArray;
			vc.selectedDelegate = selectedDelegate;
			vc.selectedSureDelegate = selectedSureDelegate;
			vc.PreferredContentSize = size;
			vc.rowIndex = rowIndex;
			UIWindow window = UIApplication.SharedApplication.KeyWindow;
			CGRect rect = view.ConvertRectToView(view.Bounds, window);
			nfloat hight = window.Frame.Height - rect.GetMaxY();

			UIPopoverController popVC = new UIPopoverController(vc);
			if (hight < size.Height)
			{
			   popVC.PresentFromRect(view.Bounds, view, UIPopoverArrowDirection.Down, true);
			}
			else 
			{
				popVC.PresentFromRect(view.Bounds, view, UIPopoverArrowDirection.Up, true);
			}

			vc.sureDelegate = () => {
				popVC.Dismiss(true);
			};

		}

        /// <summary>
        /// 顯示彈出的選擇器.
        /// </summary>
        /// <returns></returns>
        /// <param name="view">從哪個View顯示</param>
        /// <param name="size">彈出框的大小</param>
        /// <param name="dataArray">顯示的內容</param>
        /// <param name="selectedDelegate">選擇完成後執行的方法</param>
        public static void showPopPicker(UIView view, CGSize size, string[] dataArray, int rowIndex, PickerStyle style, PickerSelectedDelegate selectedDelegate, Action selectedSureDelegate)
        {
            showPopPicker(view, size, dataArray, rowIndex, false, style, selectedDelegate, selectedSureDelegate);
        }

		/// <summary>
		/// 检查网络状态
		/// </summary>
		/// <returns>网络是否正常</returns>
		public static bool checkNetworkStatus(string newworkPath)
		{
			return Reachability.IsHostReachable(newworkPath);
		}

		/// <summary>
		/// 检查网络状态
		/// </summary>
		/// <returns>网络是否正常</returns>
		public static bool checkNetworkStatus()
		{
			return checkNetworkStatus("http://www.apple.com");
		}

		/// <summary>
		/// 获取本地当前时间
		/// </summary>
		/// <returns>本地当前时间</returns>
		public static NSDate localeDate()
		{
			var date = NSDate.Now;
			var zone = NSTimeZone.SystemTimeZone;
			nint interval = zone.SecondsFromGMT(date);
			return date.AddSeconds(interval);
		}

		/// <summary>
		/// 根据时间字符串和其格式，获取对应的时间
		/// </summary>
		/// <returns>对应的时间</returns>
		/// <param name="dateStr">时间字符串.</param>
		/// <param name="format">时间字符串格式（默认值为@"yyyy-MM-dd HH:mm"）</param>
		public static NSDate dateFromString(string dateStr, string format)
		{ 
			var fmt = new NSDateFormatter();
			fmt.TimeZone = NSTimeZone.FromAbbreviation("UTC");
			fmt.DateFormat = string.IsNullOrEmpty(format) ? "yyyy-MM-dd HH:mm" : format;
			return fmt.Parse(dateStr);
		}

		/// <summary>
		/// 根据时间和其格式，获取对应的时间字符串
		/// </summary>
		/// <returns>对应的时间字符串</returns>
		/// <param name="date">时间</param>
		/// <param name="format">时间字符串格式（默认值为@"yyyy-MM-dd HH:mm"）</param>
		public static string dateToString(NSDate date, string format)
		{ 
			var fmt = new NSDateFormatter();
			fmt.TimeZone = NSTimeZone.FromAbbreviation("UTC");
			fmt.DateFormat = string.IsNullOrEmpty(format) ? "yyyy-MM-dd HH:mm" : format;
			return fmt.ToString(date);
		}

		/// <summary>
		/// Gets the app delegate.
		/// </summary>
		/// <returns>The app delegate.</returns>
		public static AppDelegate getAppDelegate()
		{
			return UIApplication.SharedApplication.Delegate as AppDelegate;
		}

		/// <summary>
		/// 根據顔色生成指定大小的圖片
		/// </summary>
		/// <returns>The image from color.</returns>
		/// <param name="color">Color.</param>
		/// <param name="size">Size.</param>
		public static UIImage getImageFromColor(UIColor color, CGSize size)
		{
			UIGraphics.BeginImageContextWithOptions(size, false, 0);
			CGContext ctx = UIGraphics.GetCurrentContext();
			ctx.SetFillColor(color.CGColor);
			UIGraphics.RectFill(new CGRect(0, 0, size.Width, size.Height));
			UIImage image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
			return image;
		}

		/// <summary>
		/// 設置TextField樣式
		/// </summary>
		/// <returns>The text field style.</returns>
		public static void setupTextFieldStyle(UITextField textField, UITextBorderStyle style, UIColor textColor, bool isEnable, string text, UIImageView rightImage)
		{
			textField.UserInteractionEnabled = isEnable;
			textField.BorderStyle = style;
			textField.TextColor = textColor;
			textField.Text = text;
			if (rightImage != null)
			{
				textField.RightView = rightImage;
				textField.RightViewMode = UITextFieldViewMode.Always;
			}
			else {
				textField.RightView = null;
			}
		}

		/// <summary>
		/// 鍵盤處理
		/// </summary>
		public static void hideKeyboard(UIView view)
		{
			var tap = new UITapGestureRecognizer((UITapGestureRecognizer obj) =>
			{
				view.Superview.EndEditing(true);
			});
			view.AddGestureRecognizer(tap);
		}
	}
}

