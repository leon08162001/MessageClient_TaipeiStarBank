using System;
using UIKit;

namespace MessageClient_ios.Util
{
	public static class UIColorHex
	{
		public static UIColor FromHex(this UIColor color, int hexValue)
		{
			return 
				UIColor.FromRGB(
				(((float)((hexValue & 0xFF0000) >> 16)) / 255.0f),
				(((float)((hexValue & 0xFF00) >> 8)) / 255.0f),
				(((float)(hexValue & 0xFF)) / 255.0f)
			);
		}
	}
}

