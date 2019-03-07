//******************************************************************
//*  author：榮剛
//*  Function：资料选择器
//*  Create Date：2016.09.08
//*  Modify Record：()
//*  <author>           <time>          <TaskID>                <desc>
//*    榮剛            2016.09.08          N/A                    
//*******************************************************************
using System;
using CoreGraphics;
using UIKit;
using Foundation;
using System.Collections.Generic;

namespace Util
{
	public delegate void PickerSelectedDelegate(PickerModel model);
	/// <summary>
	/// Picker style.
	/// </summary>
	public enum PickerStyle
	{ 
		NormalStyle,
		DateStyle
	}

	public partial class PickerViewController : UIViewController, IUIPickerViewDataSource, IUIPickerViewDelegate
	{
		UIPickerView _pickerView;
		private NSDate _minLimitedDate;//最小限制时间
		private NSDate _maxLimitedDate;//最大限制时间
		//private NSDate _defaultLimitedDate;//默认限制时间
		private NSDate _scrollToDate;//滚动到指定时间

		// 最小和最大限制时间、滚动到指定时间实体对象实例
		private PickerModel _dateMinLimitedModel;
		private PickerModel _dateMaxLimitedModel;
		private PickerModel _dateScrollToModel;

		// 存储时间数据源的数组
		List<string> _typeList = new List<string>();
		List<string> _yearList = new List<string>();
		List<string> _traditionalYearList = new List<string>();
		List<string> _monthList = new List<string>();
		List<string> _dayList = new List<string>();

		// 时间数据源的数组中，选中元素的索引
		//YearType _typeIndex;
		int _yearIndex;
		int _monthIndex;
		int _dayIndex;
		public int rowIndex;
        int _currentYearIndex;
        int _currentMonthIndex;
        int _currentdayIndex;

		private const string _defaultMinLimitedDate = "1912-01-01 00:00";
		private const string _defaultMaxLimitedDate = "2060-12-31 23:59";
		private const int _traditionalYear = 1911;
		private const int _monthPerYear = 12;
		private const int _hourPerDay = 24;
		private const int _minutePerHour = 60;

		private PickerStyle _pickerStyle = PickerStyle.NormalStyle;

		private PickerModel model = new PickerModel();
		public UIButton cancelBtn;
		public UIButton confirmBtn;

		private string[] _dataArray;
		public string[] pDataArray 
		{
			set
			{
				_dataArray = value;
				if (_pickerView != null) {
					_pickerView.ReloadAllComponents();
				}
			}
		}

        //是否允許修改民國年或公元年
        public bool AllowTypeYearChange { get; set; }

		public PickerSelectedDelegate selectedDelegate;
		public Action selectedSureDelegate;
		public Action sureDelegate;
		public PickerViewController(PickerStyle style) : base("PickerViewController", null)
		{
			_pickerStyle = style;
            AllowTypeYearChange = false;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			this.View.BackgroundColor = UIColor.White;
			if (_pickerStyle == PickerStyle.DateStyle)
			{
				loadData();
				setupPickerView();
				scrollToDateIndexPosition();
			}
			else { 
				setupPickerView();
			}


		}

        /// <summary>
        /// 初始化日期数据
        /// </summary>
        /// <returns>The data.</returns>
        public void loadData()
        {
            if (_minLimitedDate == null)
            {
                _minLimitedDate = MoneySQiOSHelper.dateFromString(_defaultMinLimitedDate, null);
            }
            _dateMinLimitedModel = new PickerModel(_minLimitedDate);

            if (_maxLimitedDate == null)
            {
                _maxLimitedDate = MoneySQiOSHelper.dateFromString(_defaultMaxLimitedDate, null);
            }
            _dateMaxLimitedModel = new PickerModel(_maxLimitedDate);

            // 滚动到指定时间；默认值为当前时间。
            // 如果是使用自定义时间小于最小限制时间，这时就以最小限制时间为准；
            // 如果是使用自定义时间大于最大限制时间，这时就以最大限制时间为准
            if (_scrollToDate == null)
            {
                _scrollToDate = MoneySQiOSHelper.localeDate();
            }
            if (_scrollToDate.Compare(_minLimitedDate) == NSComparisonResult.Ascending)
            {
                _scrollToDate = _minLimitedDate;
            }
            else if (_scrollToDate.Compare(_maxLimitedDate) == NSComparisonResult.Descending)
            {
                _scrollToDate = _maxLimitedDate;
            }
            _dateScrollToModel = new PickerModel(_scrollToDate);

            // 初始化存储时间数据源
            //西元、民国
            _typeList.AddRange(new string[] { "西元", "民國" });
            //年
            for (int beginVal = Convert.ToInt32(_dateMinLimitedModel.pYear); beginVal <= Convert.ToInt32(_dateMaxLimitedModel.pYear); beginVal++)
            {
                _yearList.Add(beginVal.ToString().PadLeft(2, '0'));
            }
            foreach (string year in _yearList)
            {
                _traditionalYearList.Add((Convert.ToInt32(year) - _traditionalYear).ToString());
            }
            // 月
            for (int i = 1; i <= _monthPerYear; i++)
            {
                _monthList.Add(i.ToString().PadLeft(2, '0'));
            }
            // 日
            reloadDayList();

            _currentYearIndex = Convert.ToInt32(_dateScrollToModel.pYear) - Convert.ToInt32(_dateMinLimitedModel.pYear);
            _currentMonthIndex = Convert.ToInt32(_dateScrollToModel.pMonth) - 1;
            _currentdayIndex = Convert.ToInt32(_dateScrollToModel.pDay) - 1;

            if (_dataArray == null)
            {
                //_typeIndex = LoginInfo.CurrentUser.PYearType;
                _yearIndex = _currentYearIndex;
                _monthIndex = _currentMonthIndex;
                _dayIndex = _currentdayIndex;
            }
            else
            {
                string date = _dataArray[0];
                if (string.IsNullOrEmpty(date))
                {
                    //_typeIndex = LoginInfo.CurrentUser.PYearType;
                    _yearIndex = _currentYearIndex;
                    _monthIndex = _currentMonthIndex;
                    _dayIndex = _currentdayIndex;
                }
                else
                {
                    string[] dateArray = date.Split('/');
                    if (dateArray[0].Length > 3)
                    {
                        //_typeIndex = YearType.CommonYear;
                        _yearIndex = _yearList.IndexOf(dateArray[0]);

                    }
                    else
                    {
                        //_typeIndex = YearType.TraditionalYear;
                        _yearIndex = Convert.ToInt32(dateArray[0]) - 1;

                    }
                    _monthIndex = Convert.ToInt32(dateArray[1]) - 1;
                    _dayIndex = Convert.ToInt32(dateArray[2]) - 1;
                }

            }
        }
		public void scrollToDateIndexPosition()
		{
			int[] arrIndex = null;
			switch (_pickerStyle) 
			{
				case PickerStyle.NormalStyle:

					break;
				case PickerStyle.DateStyle:
					//arrIndex = new int[] { (int)_typeIndex, _yearIndex, _monthIndex, _dayIndex };
					break;
			}
			for (int i = 0; i < arrIndex.Length; i++)
			{
				_pickerView.Select(arrIndex[i], i, true);
			}
		}

		public void reloadDayList()
		{
			_dayList.Clear();
			for (int i = 1; i <= (int)dayOfMonth(); i++)
			{
				_dayList.Add(i.ToString().PadLeft(2, '0'));
			}
		}

		public nint dayOfMonth()
		{
			string dateStr = string.Format("{0}-{1}-01", _yearList[_yearIndex], _monthList[_monthIndex]);
			return MoneySQiOSHelper.dateFromString(dateStr, "yyyy-MM-dd").daysOfMonth();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			setupTopLine();
			_pickerView.Frame = new CGRect(0, 44, this.View.Bounds.Width, this.View.Bounds.Height - 44);
		}

		public void setupTopLine()
		{
			UIView titleView = new UIView(new CGRect(0,0,this.View.Bounds.Width,43));
		    confirmBtn = UIButton.FromType(UIButtonType.Custom);
			confirmBtn.Frame = new CGRect(View.Frame.Width -71, 0, 71, 40);
			confirmBtn.Font = MoneySQStyle.regularFont18;
			confirmBtn.SetTitle("確認", UIControlState.Normal);
			confirmBtn.SetTitleColor(MoneySQStyle.color2B89AC, UIControlState.Normal);
			confirmBtn.TouchUpInside += (sender, e) =>
			{
				selectedDelegate(model);
				if (sureDelegate !=null) 
				{
					sureDelegate();
				}
				if (selectedSureDelegate != null)
				{
					selectedSureDelegate();
				}
			};

			titleView.AddSubview(confirmBtn);
			View.AddSubview(titleView);

			UIView lineView = new UIView(new CGRect(0, 43, this.View.Bounds.Width, 1));
			lineView.BackgroundColor = UIColor.LightGray;
			View.AddSubview(lineView);
		}

		public void setupPickerView()
		{
			_pickerView = new UIPickerView();
			_pickerView.BackgroundColor = UIColor.White;
			this.View.AddSubview(_pickerView);
			_pickerView.DataSource = this;
			_pickerView.Delegate = this;
			if (_pickerStyle == PickerStyle.DateStyle)
				{
				//model.pYear = _typeIndex == YearType.CommonYear ? _yearList[_yearIndex] : _traditionalYearList[_yearIndex].PadLeft(3, '0');
				model.pMonth = _monthList[_monthIndex];
				model.pDay = _dayList[_dayIndex];

				}
			else { 
				var pickerModel = new PickerModel(_dataArray[rowIndex]);
				model = pickerModel;
				model.pIndex = rowIndex;
			}
			_pickerView.Select(rowIndex,0,false);
		}

		#region IUIPickerViewDataSource
		[Export("numberOfComponentsInPickerView:")]
		public nint GetComponentCount(UIPickerView pickerView)
		{
			nint numberOfComponents = 0;
			switch (_pickerStyle) 
			{
				case PickerStyle.DateStyle:
					numberOfComponents = 4;
					break;
				case PickerStyle.NormalStyle:
					return 1;
			}
			return numberOfComponents;
		}

		[Export("pickerView:numberOfRowsInComponent:")]
		public nint GetRowsInComponent(UIPickerView pickerView, nint component)
		{
			nint numberOfRows = 0;
			switch (_pickerStyle)
			{
				case PickerStyle.DateStyle:
				{
					switch (component) 
					{
						case 0:
							numberOfRows = _typeList.Count;
							break;
						case 1:
							numberOfRows = _yearList.Count;
							break;
						case 2:
							numberOfRows = _monthPerYear;
							break;
						case 3:
							numberOfRows = dayOfMonth();
							break;
					}
					
				}
					break;
				case PickerStyle.NormalStyle:
					numberOfRows = _dataArray.Length;
					break;
			}
			return numberOfRows;
		}

		[Export("pickerView:titleForRow:forComponent:")]
		public string GetTitle(UIPickerView pickerView, nint row, nint component)
		{
			string title = "";
			switch (_pickerStyle) 
			{
				case PickerStyle.NormalStyle:
					title = _dataArray[row];
					break;
				case PickerStyle.DateStyle:
					switch (component)
					{
						case 0:
							title = _typeList[(int)row];
							break;
						case 1:
							//title = _typeIndex == YearType.CommonYear ? _yearList[(int)row] : _traditionalYearList[(int)row];
							break;
						case 2:
							title = _monthList[(int)row];

							break;
						case 3:
							title = _dayList[(int)row];
							break;
					}
					break;
			}
			return title;
		}

		[Export("pickerView:didSelectRow:inComponent:")]
		public void Selected(UIPickerView pickerView, nint row, nint component)
		{
			int rows = (int)row;
			switch (_pickerStyle)
			{
				case PickerStyle.NormalStyle:
					if (selectedDelegate != null)
					{
						var pickerModel = new PickerModel(_dataArray[rows]);
						model = pickerModel;
						model.pIndex = rows;
						selectedDelegate(model);
					}
					break;
				case PickerStyle.DateStyle:
					switch (component)
					{
						case 0:
                            if (AllowTypeYearChange)
                            {
                                //_typeIndex = rows == 0 ? YearType.CommonYear : YearType.TraditionalYear;
                                //LoginInfo.CurrentUser.PYearType = _typeIndex;
                            }
                            else
                            {
                                //_typeIndex = LoginInfo.CurrentUser.PYearType;
                                //pickerView.Select(_typeIndex == YearType.CommonYear ? 0 : 1, 0, true);
                            }

							break;
						case 1:
                            if (_currentYearIndex < rows)
                            {
                                _yearIndex = _currentYearIndex;
                                pickerView.Select(_yearIndex,1,true);
                            }
                            else 
                            {
                                _yearIndex = rows;
                            }
							
							break;
						case 2:
                            if (_currentYearIndex == _yearIndex)
                            {
                                if (rows > _currentMonthIndex)
                                {
                                    _monthIndex = _currentMonthIndex;
                                    pickerView.Select(_monthIndex, 2, true);
                                }
                            }
                            else
                            {
                                _monthIndex = rows;
                            }
							break;
						case 3:
                            if (_currentYearIndex == _yearIndex && _currentMonthIndex == _monthIndex)
                            {
                                if (rows > _currentdayIndex)
                                {
                                    _dayIndex = _currentdayIndex;
                                    pickerView.Select(_dayIndex, 3, true);
                                }
                            }
                            else
                            {
                                _dayIndex = rows;
                            }
							break;
					}
                    if (_yearIndex >_currentYearIndex && _monthIndex >_currentMonthIndex && _dayIndex>_currentdayIndex) 
                    {
                        _yearIndex = _currentYearIndex;
                        _monthIndex = _currentMonthIndex;
                        _dayIndex = _currentdayIndex;
                        pickerView.ReloadAllComponents();
                    }

					if (component == 0)
					{
						pickerView.ReloadComponent(1);
					}
					else if (component == 1 || component == 2)
					{
						reloadDayList();
						if (_dayList.Count - 1 < _dayIndex)
						{
							_dayIndex = _dayList.Count - 1;
						}
						pickerView.ReloadAllComponents();
					}
					string dateStr = string.Format("{0}-{1}-{2}", _yearList[_yearIndex], _monthList[_monthIndex], _dayList[_dayIndex]);
					_scrollToDate = MoneySQiOSHelper.dateFromString(dateStr, "yyyy-MM-dd");
					_dateScrollToModel = new PickerModel(_scrollToDate);

					if (selectedDelegate != null)
					{
						//model.pYear = _typeIndex == YearType.CommonYear ? _yearList[_yearIndex] : _traditionalYearList[_yearIndex].PadLeft(3, '0');
						model.pMonth = _monthList[_monthIndex];
						model.pDay = _dayList[_dayIndex];
						selectedDelegate(model);
					}

					break;
			}
		}
		#endregion

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

	}

	/// <summary>
	/// Picker模型
	/// </summary>
	public class PickerModel
	{
		public string pYear { get; set; }
		public string pMonth { get; set; }
		public string pDay { get; set; }
		public string pText { get; set; }
		public int pIndex { get; set; }

		public PickerModel()
		{ 
		
		}

		public PickerModel(string text)
		{
			pText = text;
		}

		public PickerModel(NSDate date)
		{
			var fmt = new NSDateFormatter();
			fmt.TimeZone = NSTimeZone.FromAbbreviation("UTC");
			fmt.DateFormat = "yyyyMMdd";
			string dateStr = fmt.ToString(date);

			pYear = dateStr.Substring(0, 4);
			pMonth = dateStr.Substring(4, 2);
			pDay = dateStr.Substring(6, 2);
		}
	}
}


