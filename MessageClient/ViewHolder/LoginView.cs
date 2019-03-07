using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using MessageClient;

namespace MessageClinet.ViewHolder
{
    class LoginView : Java.Lang.Object
    {
        [InjectView(Resource.Id.txtID)] public EditText txtID { get; set; }
        //[InjectView(Resource.Id.txtPassword)] public EditText txtPassword { get; set; }
        [InjectView(Resource.Id.btnLogin)] public Button btnLogin { get; set; }
        [InjectView(Resource.Id.btnClear)] public Button btnClear { get; set; }

        public LoginView(View view)
        {
            Cheeseknife.Inject(this, view);
        }
    }
}