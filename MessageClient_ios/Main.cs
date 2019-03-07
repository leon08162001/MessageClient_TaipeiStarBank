using Common;
using Foundation;
using System;
using System.IO;
using System.Reflection;
using UIKit;

namespace MessageClient_ios
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            //var assembly = IntrospectionExtensions.GetTypeInfo(typeof(Application)).Assembly;
            //Stream stream = assembly.GetManifestResourceStream("MessageClient_ios.Resources.common.ini");
            UIApplication.Main (args, null, "AppDelegate");
		}
	}
}