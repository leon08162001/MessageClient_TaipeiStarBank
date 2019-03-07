using System.Drawing;
using UIKit;
using Foundation;

namespace MessageClient_ios.Utils
{
    public static class UITextViewExtension
    {
        public static void AdjustFontSizeToFit(this UITextView label)
        {
            var font = label.Font;
            var size = label.Frame.Size;

            for (var maxSize = label.Font.PointSize; maxSize >= label.MinimumZoomScale * label.Font.PointSize; maxSize -= 1f)
            {
                font = font.WithSize(maxSize);
                var constraintSize = new SizeF((float)size.Width, float.MaxValue);
                var labelSize = (new NSString(label.Text)).StringSize(font, constraintSize, UILineBreakMode.WordWrap);

                if (labelSize.Height <= size.Height)
                {
                    label.Font = font;
                    label.SetNeedsLayout();
                    break;
                }
            }

            // set the font to the minimum size anyway
            label.Font = font;
            label.SetNeedsLayout();
        }

        public static void ResizeHeigthWithText(this UITextView label, float maxHeight = 240000f)
        {
            float width = (float)label.Frame.Width;
            SizeF size = (SizeF)((NSString)label.Text).StringSize(label.Font, constrainedToSize: new SizeF(width, maxHeight),
                    lineBreakMode: UILineBreakMode.WordWrap);
            var labelFrame = label.Frame;
            labelFrame.Size = new SizeF(width, size.Height);
            label.Frame = labelFrame;
        }
    }
}