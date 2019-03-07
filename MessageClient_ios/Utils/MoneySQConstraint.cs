//******************************************************************
//*  author：榮剛
//*  Function：UI布局約束輔助類
//*  Create Date：2016.11.30
//*  Modify Record：()
//*  <author>           <time>          <TaskID>                <desc>
//*    榮剛            2016.11.30         N/A                    
//*******************************************************************
using System;
using UIKit;

namespace Util
{
	public static class MoneySQConstraint
	{
		/// <summary>
		/// 設定寬度
		/// </summary>
		/// <param name="width">view的宽度</param>
		public static UIView EqualWidth(this UIView self, float width) 
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, width).Active = true;
			return self;
		}

		public static UIView EqualWidthToView(this UIView self, UIView other, float multiply, float constant)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Width, NSLayoutRelation.Equal, other, NSLayoutAttribute.Width, multiply, constant).Active = true;
			return self;
		}

		public static UIView EqualWidthToView(this UIView self, UIView other)
		{
			return self.EqualWidthToView(other, 1.0f, 0);
		}

		public static UIView EqualWidthToSuper(this UIView self)
		{
			return self.EqualWidthToView(self.Superview, 1.0f, 0);
		}

		/// <summary>
		/// 設定高度
		/// </summary>
		/// <param name="width">view的高度</param>
		public static UIView EqualHeight(this UIView self, float height)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, height).Active = true;
			return self;
		}

		public static UIView EqualHeightToView(this UIView self, UIView other, float multiply, float constant)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Height, NSLayoutRelation.Equal, other, NSLayoutAttribute.Height, multiply, constant).Active = true;
			return self;
		}

		public static UIView EqualHeightToView(this UIView self, UIView other)
		{
			return self.EqualHeightToView(other, 1.0f, 0);
		}

		public static UIView EqualHeightToSuper(this UIView self)
		{
			return self.EqualHeightToView(self.Superview, 1.0f, 0);
		}

		/// <summary>
		/// 等于父View的CenterX
		/// </summary>
		public static UIView EqualSuperCenterX(this UIView self)
		{
			return self.EqualViewCenterX(self.Superview, 0);
		}

		public static UIView EqualSuperCenterX(this UIView self, float offset)
		{
			return self.EqualViewCenterX(self.Superview, offset);
		}

		public static UIView EqualViewCenterX(this UIView self, UIView other)
		{
			return self.EqualViewCenterX(other, 0);
		}

		public static UIView EqualViewCenterX(this UIView self, UIView other, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, other, NSLayoutAttribute.CenterX, 1.0f, offset).Active = true;
			return self;
		}

		/// <summary>
		/// 等于父View的CenterY
		/// </summary>
		public static UIView EqualSuperCenterY(this UIView self)
		{
			return self.EqualViewCenterY(self.Superview, 0);
		}

		public static UIView EqualSuperCenterY(this UIView self, float offset)
		{
			return self.EqualViewCenterY(self.Superview, offset);
		}

		public static UIView EqualViewCenterY(this UIView self, UIView other)
		{
			return self.EqualViewCenterY(other, 0);
		}

		public static UIView EqualViewCenterY(this UIView self, UIView other, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, other, NSLayoutAttribute.CenterY, 1.0f, offset).Active = true;
			return self;
		}

		/// <summary>
		/// 等于父View的左边加上offset
		/// </summary>
		/// <param name="offset">Offset.</param>
		public static UIView EqualSuperLeft(this UIView self, float offset)
		{
			return self.AlignViewLeft(self.Superview, offset);
		}

		public static UIView AlignViewLeft(this UIView self, UIView other, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, other, NSLayoutAttribute.Leading, 1.0f, offset).Active = true;
			return self;
		}

		public static UIView EqualViewLeft(this UIView self, UIView other, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, other, NSLayoutAttribute.Leading, 1.0f, offset).Active = true;
			return self;
		}

		public static UIView GreaterThanOrEqualSuperLeft(this UIView self, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Leading, NSLayoutRelation.GreaterThanOrEqual, self.Superview, NSLayoutAttribute.Leading, 1.0f, offset).Active = true;
			return self;
		}

		public static UIView GreaterThanOrEqualViewLeft(this UIView self, UIView other, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Trailing, NSLayoutRelation.GreaterThanOrEqual, other, NSLayoutAttribute.Leading, 1.0f, offset).Active = true;
			return self;
		}

		/// <summary>
		/// 等于父View的右边减去offset
		/// </summary>
		/// <param name="offset">Offset.</param>
		public static UIView EqualSuperRight(this UIView self, float offset)
		{
			return self.AlignViewRight(self.Superview, offset);
		}

		public static UIView AlignViewRight(this UIView self, UIView other, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, other, NSLayoutAttribute.Trailing, 1.0f, offset).Active = true;
			return self;
		}

		public static UIView EqualViewRight(this UIView self, UIView other, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, other, NSLayoutAttribute.Trailing, 1.0f, offset).Active = true;
			return self;
		}

		public static UIView GreaterThanOrEqualSuperRight(this UIView self, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Trailing, NSLayoutRelation.GreaterThanOrEqual, self.Superview, NSLayoutAttribute.Trailing, 1.0f, offset).Active = true;
			return self;
		}

		public static UIView GreaterThanOrEqualViewRight(this UIView self, UIView other, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Leading, NSLayoutRelation.GreaterThanOrEqual, other, NSLayoutAttribute.Trailing, 1.0f, offset).Active = true;
			return self;
		}

		/// <summary>
		/// 等于父View的顶部加上offset
		/// </summary>
		/// <param name="offset">Offset.</param>
		public static UIView EqualSuperTop(this UIView self, float offset)
		{
			return self.AlginViewTop(self.Superview, offset);
		}

		public static UIView AlginViewTop(this UIView self, UIView other, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Top, NSLayoutRelation.Equal, other, NSLayoutAttribute.Top, 1.0f, offset).Active = true;
			return self;
		}


		public static UIView EqualViewTop(this UIView self, UIView other, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, other, NSLayoutAttribute.Top, 1.0f, offset).Active = true;
			return self;
		}

		/// <summary>
		/// 等于父View的底部减去offset
		/// </summary>
		/// <param name="offset">Offset.</param>
		public static UIView EqualSuperBottom(this UIView self, float offset)
		{
			return self.AlignViewBottom(self.Superview, offset);
		}

		public static UIView EqualViewBottom(this UIView self, UIView other, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Top, NSLayoutRelation.Equal, other, NSLayoutAttribute.Bottom, 1.0f, offset).Active = true;
			return self;
		}

		public static UIView AlignViewBottom(this UIView self, UIView other, float offset)
		{
			self.TranslatesAutoresizingMaskIntoConstraints = false;
			NSLayoutConstraint.Create(self, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, other, NSLayoutAttribute.Bottom, 1.0f, offset).Active = true;
			return self;
		}
	}
}

