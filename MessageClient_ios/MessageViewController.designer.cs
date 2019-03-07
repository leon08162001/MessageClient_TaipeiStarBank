// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace MessageClient_ios
{
    [Register ("MessageViewController")]
    partial class MessageViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblAttachments { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblMessage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView MessageTab { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIScrollView ScrollAttachments { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIScrollView ScrollMessage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIScrollView ScrollSubject { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel txtAttachments { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView txtMessage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel txtReceivedMessageTime { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel txtSendedMessageTime { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel txtSubject { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (lblAttachments != null) {
                lblAttachments.Dispose ();
                lblAttachments = null;
            }

            if (lblMessage != null) {
                lblMessage.Dispose ();
                lblMessage = null;
            }

            if (MessageTab != null) {
                MessageTab.Dispose ();
                MessageTab = null;
            }

            if (ScrollAttachments != null) {
                ScrollAttachments.Dispose ();
                ScrollAttachments = null;
            }

            if (ScrollMessage != null) {
                ScrollMessage.Dispose ();
                ScrollMessage = null;
            }

            if (ScrollSubject != null) {
                ScrollSubject.Dispose ();
                ScrollSubject = null;
            }

            if (txtAttachments != null) {
                txtAttachments.Dispose ();
                txtAttachments = null;
            }

            if (txtMessage != null) {
                txtMessage.Dispose ();
                txtMessage = null;
            }

            if (txtReceivedMessageTime != null) {
                txtReceivedMessageTime.Dispose ();
                txtReceivedMessageTime = null;
            }

            if (txtSendedMessageTime != null) {
                txtSendedMessageTime.Dispose ();
                txtSendedMessageTime = null;
            }

            if (txtSubject != null) {
                txtSubject.Dispose ();
                txtSubject = null;
            }
        }
    }
}