using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;

namespace MessageClinet.ViewHolder
{
    class LoginView : Java.Lang.Object
    {
        [InjectView(Resource.Id.txtUsername)] public EditText txtUsername { get; set; }
        [InjectView(Resource.Id.txtPassword)] public EditText txtPassword { get; set; }
        [InjectView(Resource.Id.btnLogin)] public Button btnLogin { get; set; }
        [InjectView(Resource.Id.btnClear)] public Button btnClear { get; set; }

        public LoginView(View view)
        {
            Cheeseknife.Inject(this, view);
        }
    }
}