using Android.App;
using Android.OS;
using Android.Runtime;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MessageClient
{
    public class BaseActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            GlobalException GE = new GlobalException(this);
            AndroidEnvironment.UnhandledExceptionRaiser += GlobalException.UnhandledException;
            TaskScheduler.UnobservedTaskException += GlobalException.TaskSchedulerOnUnobservedTaskException;
        }
        protected override void OnDestroy()
        {
            AndroidEnvironment.UnhandledExceptionRaiser -= GlobalException.UnhandledException;
            TaskScheduler.UnobservedTaskException -= GlobalException.TaskSchedulerOnUnobservedTaskException;
            GC.Collect(0);
            base.OnDestroy();
        }
        protected void UpdatedLocalViewFileds()
        {
            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)
                .Where(f => f.IsDefined(typeof(AndroidViewAttribute)));
            foreach (var field in fields)
            {
                var idField = typeof(Resource.Id).GetField(field.Name);
                if (idField != null)
                {
                    int id = (int)idField.GetValue(null);
                    field.SetValue(this, FindViewById(id));
                }
                else
                {
                    throw new InvalidOperationException($"Resource Id {field.Name} not found.");
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class AndroidViewAttribute : Attribute
    {

    }
}