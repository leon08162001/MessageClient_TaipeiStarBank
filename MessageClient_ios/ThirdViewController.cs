using Foundation;
using MessageClient_ios.Util;
using System;
using UIKit;

namespace MessageClient_ios
{
    public partial class ThirdViewController : GesturesViewController
    {
        public ThirdViewController (IntPtr handle) : base (handle)
        {
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            //// Perform any additional setup after loading the view, typically from a nib.
            //var stackView = new UIStackView
            //{
            //    TranslatesAutoresizingMaskIntoConstraints = false,
            //    Axis = UILayoutConstraintAxis.Vertical,
            //    Alignment = UIStackViewAlignment.Fill,
            //    Distribution = UIStackViewDistribution.EqualSpacing,
            //    Spacing = 0,
            //    LayoutMargins = new UIEdgeInsets(0, 0, 0, 0), // apply padding
            //    LayoutMarginsRelativeArrangement = true // apply padding
            //};

            //var scroll = new UIScrollView { TranslatesAutoresizingMaskIntoConstraints = false };
            //Add(scroll);

            //scroll.AddSubview(stackView);
            //scroll.FullSizeOf(View);

            //stackView.FullSizeOf(scroll);
            //stackView.WidthAnchor.ConstraintEqualTo(scroll.WidthAnchor).Active = true;

            //var nested3 = new UIStackView
            //{
            //    TranslatesAutoresizingMaskIntoConstraints = false,
            //    Axis = UILayoutConstraintAxis.Horizontal,
            //    Alignment = UIStackViewAlignment.Fill,
            //    Distribution = UIStackViewDistribution.FillProportionally,
            //    Spacing = 4
            //};

            //for (var i = 0; i < 2; i++)
            //{
            //    //nested3.AddArrangedSubview(GetRandomView());
            //    nested3.AddArrangedSubview(GetLabelView("訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容訊息內容:",50,200));
            //}

            //stackView.AddArrangedSubview(GetNestedStackContainer(nested3,0,100));
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
        private static UIView GetNestedStackContainer(UIView stackView, int viewWidth = 0, int viewHeight = 0)
        {
            var view = new RoundedView();
            view.ScaleBackImage("Images/AppBg.jpg");
            view.Layer.BorderColor = UIColor.Gray.CGColor;
            if (viewWidth == 0)
            {
                view.WidthAnchor.ConstraintEqualTo(UIScreen.MainScreen.Bounds.Width).Active = true;
            }
            else
            {
                view.WidthAnchor.ConstraintEqualTo(viewWidth).Active = true;
            }
            if (viewHeight == 0)
            {
                view.HeightAnchor.ConstraintEqualTo(UIScreen.MainScreen.Bounds.Height).Active = true;
            }
            else
            {
                view.HeightAnchor.ConstraintEqualTo(viewHeight).Active = true;
            }

            var label = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            view.Add(label);
            view.Add(stackView);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                label.LeadingAnchor.ConstraintEqualTo(view.LeadingAnchor, 0),
                label.TrailingAnchor.ConstraintEqualTo(view.TrailingAnchor, 0),
                label.TopAnchor.ConstraintEqualTo(view.TopAnchor, 0),
                label.HeightAnchor.ConstraintEqualTo(0),
                stackView.LeadingAnchor.ConstraintEqualTo(label.LeadingAnchor),
                stackView.TrailingAnchor.ConstraintEqualTo(label.TrailingAnchor),
                stackView.TopAnchor.ConstraintEqualTo(label.BottomAnchor, 5),
                stackView.BottomAnchor.ConstraintEqualTo(view.BottomAnchor, -5)
            });

            return view;
        }

        private static UIView GetRandomView(float width = 50, float? height = null)
        {
            var view = new RoundedView { BackgroundColor = UIColor.White };
            view.WidthAnchor.ConstraintEqualTo(width).Active = true;

            if (height != null)
                view.HeightAnchor.ConstraintEqualTo(height.Value).Active = true;

            return view;
        }
        private static UIView GetLabelView(string text, float width = 50, float? height = null)
        {
            var view = new UILabel();
            view.Text = text;
            view.ContentMode = UIViewContentMode.TopLeft;
            view.WidthAnchor.ConstraintEqualTo(width).Active = true;

            if (height != null)
                view.HeightAnchor.ConstraintEqualTo(height.Value).Active = true;

            return view;
        }
    }
}