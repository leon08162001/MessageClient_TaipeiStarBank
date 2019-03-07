//******************************************************************
//*  author：Comouse
//*  Function：AirPrint
//*  Create Date：2016.11.01
//*  Modify Record：()
//*  <author>           <time>          <TaskID>                <desc>
//*    Comouse         2016.11.01          N/A                    
//*******************************************************************
using System;
using Foundation;
using UIKit;

namespace Util
{
	public class AirPrintClass
	{
		public AirPrintClass()
		{
		}

		#region AirPrint

		/// <summary>
		/// AirPrint
		/// </summary>
		/// <param name="sFilePath">檔案路徑</param>
		public static void AirPrintObject(string sFilePath)
		{
			var printInfo = UIPrintInfo.PrintInfo;

			printInfo.Duplex = UIPrintInfoDuplex.LongEdge;

			printInfo.OutputType = UIPrintInfoOutputType.General;

			printInfo.JobName = "wistronits";

			var printer = UIPrintInteractionController.SharedPrintController;

			printer.PrintInfo = printInfo;

			printer.PrintingItem = NSData.FromFile(sFilePath);

			printer.ShowsPageRange = true;

			printer.Present(true, (handler, completed, err) =>
			{
				if (!completed && err != null)
				{
					Console.WriteLine("Printer Error");
				}
				else
				{
					Console.WriteLine("Printer OK");
				}
			});
		}

		#endregion
	}
}
