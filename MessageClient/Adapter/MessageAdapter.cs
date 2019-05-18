using Android.Content;
using Android.Views;
using Android.Widget;
using DBModels;
using MessageClient.ViewHolder;
using MessageClinet;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MessageClient
{
    public class MessageAdapter : BaseAdapter<MessageAddressee>
    {
        private List<MessageAddressee> messages;
        Context context;
        private LayoutInflater myInflater;
        int mSelectedPosition = -1;
        public static int lastSelectedPosition = -1;
        RadioButton mSelectedRB;

        public MessageAdapter(Context context, List<MessageAddressee> Messages) : base()
        {
            this.context = context;
            myInflater = LayoutInflater.From(context);
            this.messages = Messages;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override MessageAddressee this[int position]
        {
            get { return messages[position]; }
        }
        public override int Count
        {
            get { return messages.Count; }
        }
        public static int LastSelectedPosition
        {
            get { return lastSelectedPosition; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            try
            {
                MessageView MessageView;
                convertView = LayoutInflater.From(context).Inflate(Resource.Layout.MessageListView, parent, false);
                MessageView = new MessageView(convertView);
                convertView.Tag = MessageView;
                MessageView.rdoMessageSelect.Tag = position;
                MessageView.rdoMessageSelect.Click += RdoMessageSelect_Click;
                if (lastSelectedPosition != -1 && lastSelectedPosition == position)
                {
                    MessageView.rdoMessageSelect.Checked = true;
                    mSelectedRB = MessageView.rdoMessageSelect;
                }
                MessageView.listNo.Text = "No." + (position + 1).ToString();
                MessageView.listSubject.Text = messages[position].Subject;
                MessageView.listTime.Text = messages[position].ReceivedMessageTime;
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message);
                Common.LogHelper.MoneySQLogger.LogError<MessageTabActivity>(ex);
            }
            return convertView;
        }
        public void RdoMessageSelect_Click(object sender, EventArgs e)
        {
            if (mSelectedRB!= null && mSelectedRB.Checked)
            {
                mSelectedRB.Checked = false;
            }
            int position = Convert.ToInt32((sender as RadioButton).Tag);
            mSelectedPosition = position;
            lastSelectedPosition = position;
            mSelectedRB = sender as RadioButton;

            MainActivity ta = (((HisMessageTabActivity)context).Parent) as MainActivity;
            Android.OS.Bundle bd = new Android.OS.Bundle();
            bd.PutString("MessageID", messages[mSelectedPosition].PushMessageID);
            bd.PutString("Subject", messages[mSelectedPosition].Subject);
            bd.PutString("Addressee", messages[mSelectedPosition].Addressee);
            bd.PutString("Message", messages[mSelectedPosition].Message);
            bd.PutString("Attachments", messages[mSelectedPosition].Attachments);
            bd.PutString("SendedMessageTime", messages[mSelectedPosition].SendedMessageTime);
            bd.PutString("ReceivedMessageTime", messages[mSelectedPosition].ReceivedMessageTime);
            bd.PutString("sendedmessagetime", messages[mSelectedPosition].SendedMessageTime);
            bd.PutString("CreatedDate", messages[mSelectedPosition].CreatedDate);
            ta.Intent.PutExtra("HistroyMessage", bd);
            TabHost th = ta.GetMyTabHost();
            th.SetCurrentTabByTag("訊息公告");
        }
    }
}