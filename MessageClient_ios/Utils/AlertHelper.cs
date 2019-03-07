using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace MessageClient_ios.Utils
{
    public class AlertHelper
    {
        public delegate void AlertOKCancelDelegate(bool OK);
        public delegate void AlertTextInputDelegate(bool OK, string text);
        public static UIWindow AppWindow;
        public static void ShowOKAlert(string title, string description, UIAlertControllerStyle alertStyle, UIViewController controller = null, Action<UIAlertAction> act = null)
        {
            UIAlertController alert = UIAlertController.Create(title, description, alertStyle);

            // Configure the alert
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, act));
            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = alert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = controller.View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }
            // Display the alert
            if (controller != null)
            {
                controller.PresentViewController(alert, true, null);
            }
            else
            {
                AppWindow.MakeKeyAndVisible();
                AppWindow.RootViewController.PresentViewController(alert, true, null);
            }
        }
        public static void ShowOKCancelAlert(string title, string description, UIAlertControllerStyle alertStyle, UIViewController controller = null, Action<UIAlertAction> okAct = null, Action<UIAlertAction> cancelAct = null)
        {
            // No, inform the user that they must create a home first
            UIAlertController alert = UIAlertController.Create(title, description, alertStyle);
            // Add ok button
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, okAct));
            // Add cancel button
            alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, cancelAct));
            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = alert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = controller.View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }
            // Display the alert
            if (controller != null)
            {
                controller.PresentViewController(alert, true, null);
            }
            else
            {
                AppWindow.MakeKeyAndVisible();
                AppWindow.RootViewController.PresentViewController(alert, true, null);
            }
        }
        public static void ShowTextInputAlert(string title, string description, UIAlertControllerStyle alertStyle, string placeholder, string text, UIViewController controller = null, AlertTextInputDelegate okAct = null, Action<UIAlertAction> cancelAct = null)
        {
            // No, inform the user that they must create a home first
            UIAlertController alert = UIAlertController.Create(title, description, alertStyle);
            UITextField field = null;

            // Add and configure text field
            alert.AddTextField((textField) =>
            {
                // Save the field
                field = textField;

                // Initialize field
                field.Placeholder = placeholder;
                field.Text = text;
                field.AutocorrectionType = UITextAutocorrectionType.No;
                field.KeyboardType = UIKeyboardType.Default;
                field.ReturnKeyType = UIReturnKeyType.Done;
                field.ClearButtonMode = UITextFieldViewMode.WhileEditing;

            });
            // Add cancel button
            if (cancelAct == null)
            {
                alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (actionCancel) => { }));
            }
            else
            {
                alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, cancelAct));
            }
            // Add ok button
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, (actionOK) =>
            {
                if (okAct != null && field != null)
                {
                    okAct(true, field.Text);
                }
            }));
            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = alert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = controller.View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }
            // Display the alert
            if (controller != null)
            {
                controller.PresentViewController(alert, true, null);
            }
            else
            {
                AppWindow.MakeKeyAndVisible();
                AppWindow.RootViewController.PresentViewController(alert, true, null);
            }
        }
    }
}