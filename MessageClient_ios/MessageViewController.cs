using CoreGraphics;
using Foundation;
using MessageClient_ios.Services;
using MessageClient_ios.Utils;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using UIKit;

namespace MessageClient_ios
{
    public partial class MessageViewController : GesturesViewController
    {
        System.Timers.Timer t = new System.Timers.Timer();
        public MessageViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            t.Interval = 1000;
            t.Elapsed += new System.Timers.ElapsedEventHandler(T_Elapsed);
            t.Start();

            //txtAttachments
            UITapGestureRecognizer labelTap = new UITapGestureRecognizer(() => {
                // Do something in here
                try
                {
                    var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string PackageName = NSBundle.MainBundle.BundleIdentifier;
                    var PackageFolderPath = Path.Combine(documentsPath, PackageName);

                    bool result = UIHelper.LaunchApp("file://" + AppDelegate.GlobalVariable.filepath);
                    AlertHelper.ShowOKAlert("附件欄Click事件", "你已點選附件欄!", UIAlertControllerStyle.Alert, null, null);
                }
                catch (Exception ex)
                {
                    Common.LogHelper.MoneySQLogger.LogError<MessageViewController>(ex);
                }
            });

            txtAttachments.UserInteractionEnabled = true;
            txtAttachments.AddGestureRecognizer(labelTap);

            UIHelper.SetViewBackgroundImage(View, "Images/AppBg.jpg");
        }
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            try
            {
                if (MQService.ErrorInMainApp != null && MQService.ErrorInMainApp != "")
                {
                    AlertHelper.ShowOKAlert("MQService初始化錯誤通知", MQService.ErrorInMainApp, UIAlertControllerStyle.Alert, this, a => Thread.CurrentThread.Abort());
                    AlertHelper.ShowOKAlert("MQService初始化錯誤通知", MQService.ErrorInMainApp, UIAlertControllerStyle.Alert, this, a => { Thread.CurrentThread.Abort(); });
                }
                txtSendedMessageTime.Text = AppDelegate.GlobalVariable.SendedMessageTime;
                txtReceivedMessageTime.Text = AppDelegate.GlobalVariable.ReceivedMessageTime;
                txtSubject.Text = AppDelegate.GlobalVariable.Subject;
                txtAttachments.Text = AppDelegate.GlobalVariable.Attachments.Equals("") ? "-": AppDelegate.GlobalVariable.Attachments;
                txtMessage.Text = AppDelegate.GlobalVariable.MessageText;
                InitialScrollUI();
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<MessageViewController>(ex);
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            coordinator.AnimateAlongsideTransition((IUIViewControllerTransitionCoordinatorContext obj) => {
                // Define any animations you want to perform (equivilent to willRotateToInterfaceOrientation)
            }, (IUIViewControllerTransitionCoordinatorContext obj) => {
                // Completition executed after transistion finishes (equivilent to didRotateFromInterfaceOrientation)
                UIHelper.SetViewBackgroundImage(View, "Images/AppBg.jpg");
                InitialScrollUI();
            });
            base.ViewWillTransitionToSize(toSize, coordinator);
        }
        /// <summary>
        /// 設定附件和訊息內容欄位可上下捲動
        /// </summary>
        private void InitialScrollUI()
        {
            txtAttachments.ResizeHeigthWithText();
            //if (txtAttachments.Frame.Size.Height > 210)
            //{
            //    txtAttachments.Frame = new RectangleF(0, 0, (float)(ScrollAttachments.Frame.Size.Width), 210);
            //}
            //else
            //{
            //    txtAttachments.Frame = new RectangleF(0, 0, (float)(ScrollAttachments.Frame.Size.Width), (float)txtAttachments.Frame.Size.Height);
            //}
            txtAttachments.Frame = new RectangleF(0, 0, (float)(ScrollAttachments.Frame.Size.Width), (float)txtAttachments.Frame.Size.Height);
            ScrollAttachments.ContentSize = txtAttachments.Frame.Size;

            txtSubject.ResizeHeigthWithText();
            txtSubject.Frame = new RectangleF(0, 0, (float)(ScrollSubject.Frame.Size.Width), (float)txtSubject.Frame.Size.Height);
            ScrollSubject.ContentSize = txtSubject.Frame.Size;

            //txtMessage.ResizeHeigthWithText();
            //txtMessage.Frame = new RectangleF(0, 0, (float)(ScrollMessage.Frame.Size.Width), (float)txtMessage.Frame.Size.Height);
            //ScrollMessage.ContentSize = txtMessage.Frame.Size;
        }
        private void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                InvokeOnMainThread(() =>
                {
                    if (txtSendedMessageTime.Text != AppDelegate.GlobalVariable.SendedMessageTime || txtReceivedMessageTime.Text != AppDelegate.GlobalVariable.ReceivedMessageTime)
                    //if (txtSubject.Text != AppDelegate.GlobalVariable.Subject || txtAttachments.Text != AppDelegate.GlobalVariable.Attachments)
                    {

                        txtSendedMessageTime.Text = AppDelegate.GlobalVariable.SendedMessageTime;
                        txtReceivedMessageTime.Text = AppDelegate.GlobalVariable.ReceivedMessageTime;
                        txtSubject.Text = AppDelegate.GlobalVariable.Subject;
                        txtAttachments.Text = AppDelegate.GlobalVariable.Attachments.Equals("") ? "-" : AppDelegate.GlobalVariable.Attachments;
                        txtMessage.Text = AppDelegate.GlobalVariable.MessageText;
                        InitialScrollUI();
                    }
                });
            }
            catch (Exception ex)
            {
                Common.LogHelper.MoneySQLogger.LogError<MessageViewController>(ex);
            }
        }
    }
}