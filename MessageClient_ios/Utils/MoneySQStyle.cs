//******************************************************************
//*  author：榮剛
//*  Function：文字樣式、背景顔色
//*  Create Date：2016.10.08
//*  Modify Record：()
//*  <author>           <time>          <TaskID>                <desc>
//*    榮剛            2016.10.08         N/A                    
//*******************************************************************

using MessageClient_ios.Util;
using System;
using UIKit;

namespace Util
{
	public class MoneySQStyle
	{
		//color
		public static UIColor color000000 = UIColor.Clear.FromHex(0x000000);
		public static UIColor color1586AD = UIColor.Clear.FromHex(0x1586AD);
		public static UIColor color2B89AC = UIColor.Clear.FromHex(0x2b89ac);
		public static UIColor color28B4A9 = UIColor.Clear.FromHex(0x28b4a9);
		public static UIColor color333333 = UIColor.Clear.FromHex(0x333333);
		public static UIColor color565656 = UIColor.Clear.FromHex(0x565656);
		public static UIColor color58595B = UIColor.Clear.FromHex(0x58595b);
		public static UIColor color595857 = UIColor.Clear.FromHex(0x595857);
		public static UIColor color999999 = UIColor.Clear.FromHex(0x999999);
		public static UIColor color9E9E9E = UIColor.Clear.FromHex(0x9e9e9e);
		public static UIColor colorB0B0B0 = UIColor.Clear.FromHex(0xb0b0b0);
		public static UIColor colorB1B1B2 = UIColor.Clear.FromHex(0xb1b1b2);
		public static UIColor colorDCE4EE = UIColor.Clear.FromHex(0xdce4ee);
		public static UIColor colorDCE5EE = UIColor.Clear.FromHex(0xdce5ee);
		public static UIColor colorE7E7E7 = UIColor.Clear.FromHex(0xe7e7e7);
		public static UIColor colorEEEEEE = UIColor.Clear.FromHex(0xeeeeee);
		public static UIColor colorEFEFEF = UIColor.Clear.FromHex(0xefefef);
		public static UIColor colorF0F0F0 = UIColor.Clear.FromHex(0xf0f0f0);
		public static UIColor colorF6F6F6 = UIColor.Clear.FromHex(0xf6f6f6);
		public static UIColor colorF6F5F5 = UIColor.Clear.FromHex(0xf6f5f5);
		public static UIColor colorF7F7F7 = UIColor.Clear.FromHex(0xf7f7f7);
		public static UIColor colorFFFFFF = UIColor.Clear.FromHex(0xffffff);

		//size
		public static int statusBarHeight = 20;
		public static int navBarHeight = 44;
		public static int toolBarHeight = 74;

		//font
		public static UIFont regularFont20 = UIFont.SystemFontOfSize(20);//預設標題
		public static UIFont regularFont18 = UIFont.SystemFontOfSize(18);//內文文字
		public static UIFont regularFont16 = UIFont.SystemFontOfSize(16);//可變動或highline之文字
		public static UIFont regularFont15 = UIFont.SystemFontOfSize(15);
		public static UIFont regularFont13 = UIFont.SystemFontOfSize(13);//不可變動或系統自動帶入之文字

		public static UIFont boldFont20 = UIFont.BoldSystemFontOfSize(20);
		public static UIFont boldFont18 = UIFont.BoldSystemFontOfSize(18);
		public static UIFont boldFont16 = UIFont.BoldSystemFontOfSize(16);
		public static UIFont boldFont13 = UIFont.BoldSystemFontOfSize(13);

		//text
		public static string promptText = "提示訊息";
		public static string warnText = "警告";
		public static string noAttachTreatyText = "無可附加之附約!";
		public static string deleteText = "確定要刪除嗎 ?";
	}
}

