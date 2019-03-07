using Android.Content;
using Android.Runtime;
using Android.Widget;
using Common;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MessageClinet
{
    class GlobalException
    {
        private static Context context;
        public GlobalException(Context c)
        {
            context = c;
        }
        public static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            var newExc = new Exception("TaskSchedulerOnUnobservedTaskException", unobservedTaskExceptionEventArgs.Exception);
            LogUnhandledException(newExc);
        }

        public static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var newExc = new Exception("CurrentDomainOnUnhandledException", unhandledExceptionEventArgs.ExceptionObject as Exception);
            LogUnhandledException(newExc);
        }

        public static void UnhandledException(object sender, RaiseThrowableEventArgs e)
        {
            e.Handled = true;
            LogUnhandledException(e.Exception);
        }

        internal static void LogUnhandledException(Exception ex)
        {
            try
            {
                const string errorFileName = "Fatal.log";
                var libraryPath = context.GetExternalFilesDir(@Config.logDir).AbsolutePath;
                var errorFilePath = Path.Combine(libraryPath, errorFileName);
                var errorMessage = String.Format("Time: {0}\r\nError: Unhandled Exception\r\n{1}",
                DateTime.Now, ex.ToString());
                File.WriteAllText(errorFilePath, errorMessage);
                Toast.MakeText(context, ex.ToString(), ToastLength.Long).Show();
                // Log to Android Device Logging.
                Android.Util.Log.Error("UnhandledException Report", errorMessage);
            }
            catch
            {
                // just suppress any error logging exceptions
            }
        }
    }
}