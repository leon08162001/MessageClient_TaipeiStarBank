using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using Common;
using Common.LinkLayer;
using DBLogic;
using DBModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MessageClinet.ViewHolder;
using System;
using System.IO;
using System.Reflection;
using Color = Android.Graphics.Color;

namespace MessageClinet
{
    [Activity(Label = "My Activity")]
    public class MessageTabActivity : BaseActivity
    {
        [AndroidView]
        private TextView textView1, txtMessage, txtReceivedMessageTime, txtSendedMessageTime, txtSubject, txtAttachments;
        [AndroidView]
        private Button  btnCreatePdf;
        [AndroidView]
        private TableRow tableRow5;
        //[AndroidView]
        //private Button btnClearMsg, btnIDSetting;
        [AndroidView]
        private ScrollView scrollMessage;
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
            AttachmentsSetting();
            //txtMessage.MovementMethod = new Android.Text.Method.ScrollingMovementMethod();
            try
            {
                if (this.Intent.Extras == null)
                {
                    txtSendedMessageTime.Text = MainApp.GlobalVariable.SendedMessageTime == "" ? "" : MainApp.GlobalVariable.SendedMessageTime;
                    txtReceivedMessageTime.Text = MainApp.GlobalVariable.ReceivedMessageTime == "" ? "" : MainApp.GlobalVariable.ReceivedMessageTime;
                    txtSubject.Text = MainApp.GlobalVariable.Subject;
                    txtMessage.Text = MainApp.GlobalVariable.MessageText;
                    //txtMessage.ScrollTo(0, 0);
                    scrollMessage.ScrollTo(0, 0);
                }
                else if (this.Intent.HasExtra("MQJefferiesExcuReportMessage"))
                {
                    txtSendedMessageTime.Text = MainApp.GlobalVariable.SendedMessageTime;
                    txtReceivedMessageTime.Text = MainApp.GlobalVariable.ReceivedMessageTime;
                    txtSubject.Text = MainApp.GlobalVariable.Subject;
                    txtMessage.Text = MainApp.GlobalVariable.MessageText;
                    //txtMessage.ScrollTo(0, 0);
                    scrollMessage.ScrollTo(0, 0);
                    this.Intent.RemoveExtra("MQJefferiesExcuReportMessage");
                }
                else if (this.Intent.HasExtra("EMSJefferiesExcuReportMessage"))
                {
                    txtSendedMessageTime.Text = MainApp.GlobalVariable.SendedMessageTime;
                    txtReceivedMessageTime.Text = MainApp.GlobalVariable.ReceivedMessageTime;
                    txtSubject.Text = MainApp.GlobalVariable.Subject;
                    txtMessage.Text = MainApp.GlobalVariable.MessageText;
                    //txtMessage.ScrollTo(0, 0);
                    scrollMessage.ScrollTo(0, 0);
                    this.Intent.RemoveExtra("EMSJefferiesExcuReportMessage");
                }
                //String value = "<a href='file:////sdcard/DocumentsMoneySQ.MessageClient/Screenshot.jpg'>file</a>";
                //txtAttachments.TextFormatted = Android.Text.Html.FromHtml(value);
                ////txtAttachments.SetText(value,TextView.BufferType.Normal);
                //Linkify.AddLinks(txtAttachments,MatchOptions.All);
                ////txtAttachments.MovementMethod = new Android.Text.Method.LinkMovementMethod();
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message);
                Common.LogHelper.MoneySQLogger.LogError<MessageTabActivity>(ex);
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }
        protected override void OnResume()
        {
            base.OnResume();
            try
            {
                if (this.Parent.Intent.HasExtra("HistroyMessage"))
                {
                    Android.OS.Bundle bd = this.Parent.Intent.GetBundleExtra("HistroyMessage");
                    MainApp.GlobalVariable.MessageID = bd.GetString("MessageID");
                    MainApp.GlobalVariable.Subject = bd.GetString("Subject");
                    MainApp.GlobalVariable.MessageText = bd.GetString("Message");
                    MainApp.GlobalVariable.Attachments = bd.GetString("Attachments", "");
                    MainApp.GlobalVariable.SendedMessageTime = bd.GetString("SendedMessageTime");
                    MainApp.GlobalVariable.ReceivedMessageTime = bd.GetString("ReceivedMessageTime");
                    txtSendedMessageTime.Text = MainApp.GlobalVariable.SendedMessageTime;
                    txtReceivedMessageTime.Text = MainApp.GlobalVariable.ReceivedMessageTime;
                    txtSubject.Text = MainApp.GlobalVariable.Subject;
                    txtMessage.Text = MainApp.GlobalVariable.MessageText;
                    AttachmentsSetting();
                    //txtMessage.ScrollTo(0, 0);
                    scrollMessage.ScrollTo(0, 0);
                    this.Parent.Intent.RemoveExtra("HistroyMessage");
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message);
                Common.LogHelper.MoneySQLogger.LogError<MessageTabActivity>(ex);
            }
        }
        protected override void OnDestroy()
        {
            //btnClearMsg.Click -= BtnClearMsg_Click;
            //btnIDSetting.Click -= BtnIDSetting_Click;
            btnCreatePdf.Click -= BtnCreatePdf_Click;
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
            //txtMessage.ScrollTo(0, 0);
            scrollMessage.ScrollTo(0, 0);
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
                Intent EmsService = new Intent(this, typeof(Services.EMSService));
                StopService(EmsService);
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                {
                    StartForegroundService(EmsService);
                }
                else
                {
                    StartService(EmsService);
                }
                Toast.MakeText(this, "已重設接收推播的ID!", ToastLength.Long).Show();   
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
            try
            {
                Bitmap screen;
                View view = this.Window.DecorView.RootView;
                view.DrawingCacheEnabled = true;
                screen = Bitmap.CreateBitmap(view.DrawingCache);
                view.DrawingCacheEnabled = false;
                string FolderPath = "";
                FolderPath = Application.Context.GetExternalFilesDir("").AbsolutePath;
                DirectoryInfo FolderInfo = new DirectoryInfo(FolderPath + @"/Screen");
                if (!FolderInfo.Exists)
                {
                    FolderInfo.Create();
                }
                FileInfo PdfFile = new FileInfo(System.IO.Path.Combine(FolderPath + @"/Screen/", "Message_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf"));
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
                Common.LogHelper.MoneySQLogger.LogError<MessageTabActivity>(ex);
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }
        private void TxtAttachments_Click(object sender, EventArgs e)
        {
            string FolderPath = "";
            string PackageName = Application.Context.PackageName;
            FolderPath = Application.Context.GetExternalFilesDir("").AbsolutePath;
            DirectoryInfo FolderInfo = new DirectoryInfo(FolderPath + @"/Attachment" + @"/" + MainApp.GlobalVariable.MessageID);
            Android.Net.Uri selectedUri = Android.Net.Uri.Parse(FolderInfo.FullName);

            Intent intent = null;
            if (PackageManager.GetLaunchIntentForPackage(Config.folderApp1) != null)
            {
                intent = PackageManager.GetLaunchIntentForPackage(Config.folderApp1);
            }
            else if (PackageManager.GetLaunchIntentForPackage(Config.folderApp2) != null)
            {
                intent = PackageManager.GetLaunchIntentForPackage(Config.folderApp2);
            }
            if (intent != null)
            {
                if (intent.Package.ToLower().Trim().Equals("org.openintents.filemanager"))
                {
                    intent.SetAction(Intent.ActionView);
                    intent.SetDataAndType(selectedUri, "*/*");
                    StartActivity(Intent.CreateChooser(intent, "Open folder"));
                }
                else
                {
                    intent = new Intent(Intent.ActionView);
                    intent.SetDataAndType(selectedUri, "resource/folder");
                    StartActivity(Intent.CreateChooser(intent, "Open folder"));
                }
            }
            else
            {
                Toast.MakeText(this, "尚未安裝指定的檔案總管app", ToastLength.Long).Show();
            }

            //Intent intent = new Intent();
            //Intent intent = PackageManager.GetLaunchIntentForPackage("org.openintents.filemanager");
            //intent.SetDataAndType(selectedUri, "*");
            //intent.SetFlags(ActivityFlags.NewTask);
            //Application.Context.StartActivity(intent);
        }
        public Bitmap GetBitmapFromView(ScrollView view)
        {
            //int width = screen.Width;
            //int height = screen.Height;
            //Matrix matrix = new Matrix();
            //matrix.PostScale(view.GetChildAt(0).Width / 20, view.GetChildAt(0).Height / 20);
            //Bitmap returnedBitmap = Bitmap.CreateBitmap(screen, 0, 0, width, height, matrix, true);
            //returnedBitmap.SetConfig(Bitmap.Config.Argb8888);
            Bitmap returnedBitmap = Bitmap.CreateBitmap(view.GetChildAt(0).Width, 2000, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(returnedBitmap);
            Drawable bgDrawable = view.Background;
            if (bgDrawable != null)
                bgDrawable.Draw(canvas);
            else
                canvas.DrawColor(Color.White);
            view.Draw(canvas);
            return returnedBitmap;
        }
        private void AttachmentsSetting()
        {
            if (MainApp.GlobalVariable.Attachments.Equals(""))
            {
                tableRow5.Visibility = ViewStates.Gone;
            }
            else
            {
                String content = "";
                foreach (string Attachment in MainApp.GlobalVariable.Attachments.Split(new char[] { ';' }))
                {
                    if (!Attachment.Equals(""))
                    {
                        content += "<u>" + Attachment + "</u>、";
                    }
                }
                content = content.Substring(0, content.Length - 1);
                txtAttachments.TextFormatted = Android.Text.Html.FromHtml(content);
                tableRow5.Visibility = ViewStates.Visible;
                txtAttachments.Click -= TxtAttachments_Click;
                txtAttachments.Click += TxtAttachments_Click;
            }
        }
    }
}