using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.Res;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Common;
using Common.LinkLayer;
using Common.Utility;
using DBLogic;
using DBModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using MessageClinet.Services;
using MessageClinet.ViewHolder;
using System;
using System.Reflection;
using Android.Graphics.Drawables;

namespace MessageClinet
{
    [Activity(Label = "My Activity")]
    public class MessageTabActivity : BaseActivity
    {
        [AndroidView]
        private TextView textView1, txtMessage, txtReceivedMessageTime, txtSendedMessageTime, txtSubject;
        [AndroidView]
        private Button btnCreatePdf;
        //[AndroidView]
        //private Button btnClearMsg, btnIDSetting;
        //[AndroidView]
        //private ScrollView scrollMessage;
        AlertDialog builder;
        LoginView LoginView;

        IMQAdapter JefferiesExcuReport = TopicMQFactory.GetMQAdapterInstance(MQAdapterType.BatchMQAdapter);

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MessageTab);
            UpdatedLocalViewFileds();
            //btnClearMsg.Click += BtnClearMsg_Click;
            //btnIDSetting.Click += BtnIDSetting_Click;
            btnCreatePdf.Click += BtnCreatePdf_Click;
            txtMessage.MovementMethod = new Android.Text.Method.ScrollingMovementMethod();
            try
            {
                if (this.Intent.Extras == null)
                {
                    txtSendedMessageTime.Text = MainApp.GlobalVariable.SendedMessageTime == "" ? "" : MainApp.GlobalVariable.SendedMessageTime;
                    txtReceivedMessageTime.Text = MainApp.GlobalVariable.ReceivedMessageTime == "" ? "" : MainApp.GlobalVariable.ReceivedMessageTime;
                    txtSubject.Text = MainApp.GlobalVariable.Subject;
                    txtMessage.Text = MainApp.GlobalVariable.MessageText;
                    txtMessage.ScrollTo(0, 0);
                    //scrollMessage.ScrollTo(0, 0);
                }
                else if (this.Intent.HasExtra("MQJefferiesExcuReportMessage"))
                {
                    txtSendedMessageTime.Text = MainApp.GlobalVariable.SendedMessageTime;
                    txtReceivedMessageTime.Text = MainApp.GlobalVariable.ReceivedMessageTime;
                    txtSubject.Text = MainApp.GlobalVariable.Subject;
                    txtMessage.Text = MainApp.GlobalVariable.MessageText;
                    txtMessage.ScrollTo(0, 0);
                    //scrollMessage.ScrollTo(0, 0);
                    this.Intent.RemoveExtra("MQJefferiesExcuReportMessage");
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message);
                Logger.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message, Java.Lang.Throwable.FromException(ex));
                Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
            }
        }
        protected override void OnDestroy()
        {
            textView1.Dispose();
            txtMessage.Dispose();
            txtReceivedMessageTime.Dispose();
            txtSendedMessageTime.Dispose();
            txtSubject.Dispose();
            //btnClearMsg.Dispose();
            //btnIDSetting.Dispose();
            GC.Collect(0);
            base.OnDestroy();
        }
        protected override void OnResume()
        {
            base.OnResume();
            try
            {
                if (this.Parent.Intent.HasExtra("HistroyMessage"))
                {
                    Android.OS.Bundle bd = this.Parent.Intent.GetBundleExtra("HistroyMessage");
                    MainApp.GlobalVariable.Subject = bd.GetString("Subject");
                    MainApp.GlobalVariable.MessageText = bd.GetString("Message");
                    MainApp.GlobalVariable.SendedMessageTime = bd.GetString("SendedMessageTime");
                    MainApp.GlobalVariable.ReceivedMessageTime = bd.GetString("ReceivedMessageTime");
                    txtSendedMessageTime.Text = MainApp.GlobalVariable.SendedMessageTime;
                    txtReceivedMessageTime.Text = MainApp.GlobalVariable.ReceivedMessageTime;
                    txtSubject.Text = MainApp.GlobalVariable.Subject;
                    txtMessage.Text = MainApp.GlobalVariable.MessageText;
                    txtMessage.ScrollTo(0, 0);
                    //scrollMessage.ScrollTo(0, 0);
                    this.Parent.Intent.RemoveExtra("HistroyMessage");
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message);
                Logger.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message, Java.Lang.Throwable.FromException(ex));
            }
        }
        private int ConvertPixelsToDp(int pixelValue)
        {
            var dp = (int)((pixelValue) / Resources.DisplayMetrics.Density);
            return dp;
        }
        private void ShowLoginDialog()
        {
            var metrics = Resources.DisplayMetrics;
            LayoutInflater inflater = (LayoutInflater)Application.Context.GetSystemService(Context.LayoutInflaterService);
            View view = inflater.Inflate(Resource.Layout.LoginAlertDialog, null);
            LoginView = new LoginView(view);

            view.SetMinimumWidth((int)(ConvertPixelsToDp(metrics.WidthPixels) * 0.9f));
            view.SetMinimumHeight((int)(ConvertPixelsToDp(metrics.HeightPixels) * 0.6f));
            builder = new AlertDialog.Builder(this).Create();
            builder.SetView(view);
            builder.SetCanceledOnTouchOutside(false);
            LoginView.btnClear.Click += BtnClear_Click;
            LoginView.btnLogin.Click += BtnLogin_Click;
            builder.Show();
            builder.Window.SetLayout(Convert.ToInt16(metrics.WidthPixels * 0.9f), Convert.ToInt16(metrics.HeightPixels * 0.45f));
        }
        private void BtnClearMsg_Click(object sender, EventArgs e)
        {
            MainApp.GlobalVariable.SendedMessageTime = "";
            MainApp.GlobalVariable.ReceivedMessageTime = "";
            MainApp.GlobalVariable.Subject = "";
            MainApp.GlobalVariable.MessageText = "";
            txtSendedMessageTime.Text = "";
            txtReceivedMessageTime.Text = "";
            txtSubject.Text = "";
            txtMessage.Text = "";
            txtMessage.ScrollTo(0, 0);
            //scrollMessage.ScrollTo(0, 0);
        }
        private void BtnIDSetting_Click(object sender, EventArgs e)
        {
            ShowLoginDialog();
        }
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (!LoginView.txtID.Text.Trim().Equals(""))
            {
                Profile Profile = new Profile();
                Profile.ID = LoginView.txtID.Text.Trim();
                DBProfile.ClearProfile(MainApp.GlobalVariable.DBFile.FullName);
                DBProfile.InsertProfile(Profile, MainApp.GlobalVariable.DBFile.FullName);
                builder.Dismiss();
                Intent MqService = new Intent(this, typeof(Services.MQService));
                StopService(MqService);
                StartService(MqService);
                Toast.MakeText(this, "已重設接收推撥的ID!", ToastLength.Long).Show();   
            }
            else
            {
                Toast.MakeText(this, "請輸入您的身分ID!", ToastLength.Long).Show();
            }
        }
        private void BtnClear_Click(object sender, EventArgs e)
        {
            builder.Dismiss();
        }
        private void BtnCreatePdf_Click(object sender, EventArgs e)
        {
            Bitmap screen;
            View view = this.Window.DecorView.RootView;
            view.DrawingCacheEnabled = true;
            screen = Bitmap.CreateBitmap(view.DrawingCache);
            view.DrawingCacheEnabled = false;
            DirectoryInfo FolderInfo = new DirectoryInfo(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath + @"/" + this.PackageName);
            if(!FolderInfo.Exists)
            {
                FolderInfo.Create();
            }
            FileInfo PdfFile = new FileInfo(System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath + @"/" + this.PackageName, "Message_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf"));
            try
            {
                MemoryStream stream = new MemoryStream();
                screen.Compress(Bitmap.CompressFormat.Png, 100, stream);
                Image Pdfimage = Image.GetInstance(stream.ToArray());
                Pdfimage.SetAbsolutePosition(0, 0);
                Document document = new Document(Pdfimage);
                PdfWriter.GetInstance(document, new FileStream(PdfFile.FullName, FileMode.Create));
                document.Open();
                document.Add(Pdfimage);
                document.Close();
                Toast.MakeText(this, "已成功擷取目前畫面至pdf檔", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "BtnCreatePdf_Click() Error({0})", ex.Message);
                Common.Utility.Logger.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "BtnCreatePdf_Click() Error " + ex.Message, Java.Lang.Throwable.FromException(ex));
            }
        }
    }
}