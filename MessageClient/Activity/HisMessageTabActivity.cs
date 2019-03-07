using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using DBLogic;
using DBModels;
using MessageClient.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace MessageClient
{
    [Activity(Label = "My Activity")]
    public class HisMessageTabActivity : BaseActivity
    {
        [AndroidView]
        private ListView listView1;
        [AndroidView]
        private Spinner selNumber;
        List<MessageAddressee> Messages = new List<MessageAddressee>();
        MessageAdapter messageAdapter = null;
        ArrayAdapter Adapter = null;
        Timer Timer = null;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.HisMessageTab);
            UpdatedLocalViewFileds();
            string[] messagecount = { "all", "50", "100", "150", "200" };

            Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, messagecount);
            Adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSelectableListItem);
            selNumber.Adapter = Adapter;
            selNumber.ItemSelected += SelNumber_ItemSelected;
        }
        protected override void OnDestroy()
        {
            selNumber.ItemSelected -= SelNumber_ItemSelected;
            listView1.ItemClick -= ListView1_ItemClick;
            listView1.Dispose();
            selNumber.Dispose();
            messageAdapter.Dispose();
            Adapter.Dispose();
            Messages.Clear();
            GC.Collect(0);
            base.OnDestroy();
        }
        private void SelNumber_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                Spinner ddl = sender as Spinner;
                if (ddl.SelectedItem.ToString().Equals("all"))
                {
                    MainApp.GlobalVariable.DBMessages = DBMessageAddressee.GetDBAllMessage(MainApp.GlobalVariable.DBFile.FullName);
                }
                else
                {
                    MainApp.GlobalVariable.DBMessages = DBMessageAddressee.GetDBTopMessage(Convert.ToInt32(ddl.SelectedItem.ToString()), MainApp.GlobalVariable.DBFile.FullName);
                }
                messageAdapter = new MessageAdapter(this, MainApp.GlobalVariable.DBMessages);
                listView1.ItemClick += ListView1_ItemClick;
                listView1.Adapter = messageAdapter;
                listView1.SetFriction(ViewConfiguration.ScrollFriction * (float)0.5);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message);
                Common.LogHelper.MoneySQLogger.LogError<HisMessageTabActivity>(ex);
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }
        protected override void OnResume()
        {
            base.OnResume();
            try
            {
                TimerCallback timerDelegate = new TimerCallback(CheckNewMessage);
                Timer = new Timer(timerDelegate, null, 0, 1000);
                BindHistoryMessage();
                listView1.SetSelection(MessageAdapter.LastSelectedPosition);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message);
                Common.LogHelper.MoneySQLogger.LogError<HisMessageTabActivity>(ex);
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }
        protected override void OnPause()
        {
            Timer.Dispose();
            base.OnPause();
        }
        private void ListView1_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (messageAdapter == null)
            {
                messageAdapter = new MessageAdapter(this, MainApp.GlobalVariable.DBMessages);
            }
            RadioButton rdoMessageSelect = e.View.FindViewById<RadioButton>(Resource.Id.rdoMessageSelect);
            messageAdapter.RdoMessageSelect_Click(rdoMessageSelect, e);
        }

        private void CheckNewMessage(Object state)
        {
            if (MQService.IsNewMessageData)
            {
                MQService.IsNewMessageData = false;
                RunOnUiThread(() =>
            {
                BindHistoryMessage();
            });
            }
        }
        private void BindHistoryMessage()
        {
            if (MainApp.GlobalVariable.DBMessages == null)
            {
                MainApp.GlobalVariable.DBMessages = DBMessageAddressee.GetDBAllMessage(MainApp.GlobalVariable.DBFile.FullName);
            }
            messageAdapter = new MessageAdapter(this, MainApp.GlobalVariable.DBMessages);
            listView1.Adapter = messageAdapter;
        }
    }
}