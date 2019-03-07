using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;

namespace MessageClient.ViewHolder
{
    class MessageView : Java.Lang.Object
    {
        [InjectView(Resource.Id.listNo)]
        public TextView listNo { get; set; }
        [InjectView(Resource.Id.listSubject)]
        public TextView listSubject { get; set; }
        [InjectView(Resource.Id.listTime)]
        public TextView listTime { get; set; }
        [InjectView(Resource.Id.rdoMessageSelect)]
        public RadioButton rdoMessageSelect { get; set; }

        public MessageView(View view)
        {
            Cheeseknife.Inject(this, view);
        }
    }
}